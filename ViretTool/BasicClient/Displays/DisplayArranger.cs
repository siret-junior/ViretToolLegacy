using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    public enum DisplayArrangement
    {
        Ranking, Sequential, Semantic, Color
    }

    public class DisplayArranger
    {
        // TODO: horizontal/vertical sort
        
        public static RankedFrame[,] ArrangeDisplay(List<RankedFrame> frames, int nRows, int nCols, DisplayArrangement displayArrangement)
        {
            switch (displayArrangement)
            {
                case DisplayArrangement.Ranking:
                    return SortByRanking(frames, nRows, nCols);
                case DisplayArrangement.Sequential:
                    return SortByID(frames, nRows, nCols);
                case DisplayArrangement.Semantic:
                    return SortBySemantics(frames, nRows, nCols);
                case DisplayArrangement.Color:
                    return SortByColor(frames, nRows, nCols);
                    break;
                default:
                    throw new NotImplementedException("Unknown display arrangement!");
                    break;

            }
        }

        
        private static RankedFrame[,] SortByRanking(List<RankedFrame> frames, int nRows, int nCols)
        {
            // assumes ranking engine already returned frames sorted by rank
            return FillHorizontally(frames, nRows, nCols);
        }

        private static RankedFrame[,] SortByID(List<RankedFrame> frames, int nRows, int nCols)
        {
            IEnumerable<RankedFrame> sortedList = frames.OrderBy(x => x.Frame.ID);
            return FillHorizontally(frames, nRows, nCols);
        }

        private static RankedFrame[,] FillHorizontally(IEnumerable<RankedFrame> inputFrames, int nRows, int nCols)
        {
            RankedFrame[,] arrangement = new RankedFrame[nRows, nCols];

            using (IEnumerator<RankedFrame> frameEnumerator = inputFrames.GetEnumerator())
            {

                for (int iRow = 0; iRow < nRows; iRow++)
                {
                    for (int iCol = 0; iCol < nCols; iCol++)
                    {
                        frameEnumerator.MoveNext();
                        arrangement[iRow, iCol] = frameEnumerator.Current;
                    }
                }
            }
            return arrangement;
        }

        private static RankedFrame[,] SortBySemantics(List<RankedFrame> frames, int nRows, int nCols)
        {
            double[,] distanceMatrix = ComputeDistanceMatrix(frames, RankedFrame.SemanticDistance);
            RankedFrame[,] arrangedFrames = SortDisplay(frames, nRows, nCols, distanceMatrix);
            return arrangedFrames;
        }

        private static RankedFrame[,] SortByColor(List<RankedFrame> frames, int nRows, int nCols)
        {
            double[,] distanceMatrix = ComputeDistanceMatrix(frames, RankedFrame.ColorDistance);
            RankedFrame[,] arrangedFrames = SortDisplay(frames, nRows, nCols, distanceMatrix);
            return arrangedFrames;
        }
        
        private static double[,] ComputeDistanceMatrix(
            List<RankedFrame> frames, Func<RankedFrame, RankedFrame, double> distanceFunction)
        {
            // create matrix, TODO: reuse this matrix
            double[,] distanceMatrix = new double[frames.Count, frames.Count];
            int nRows = distanceMatrix.GetLength(0);
            int nCols = distanceMatrix.GetLength(1);

            // compute matrix values
            for (int iRow = 0; iRow < nRows; iRow++)
            {
                // matrix is symmetric, compute upper triangle only
                for (int iCol = iRow + 1; iCol < nCols; iCol++)
                {
                    distanceMatrix[iRow, iCol] = distanceFunction(frames[iRow], frames[iCol]);
                }
            }

            return distanceMatrix;
        }

        private static RankedFrame[,] SortDisplay(List<RankedFrame> frames, int nRows, int nCols, double[,] distanceMatrix)
        {
            // create and initialize display by sorting to three columns based on brightness
            int[,] display = InitializeDisplay(nRows, nCols);
            int nDisplayFrames = display.Length;

            // finetune display -> swap two images if it improves layout similarity
            Random r = new Random();
            System.Drawing.Point p1 = new System.Drawing.Point();
            System.Drawing.Point p2 = new System.Drawing.Point();
            double distancesBefore;
            double distancesAfter;
            int temp;
            int radius = 3;
            int iterationCount = 250000;
            if (nDisplayFrames > frames.Count) iterationCount = 0;

            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                p1.X = r.Next() % nRows; p1.Y = r.Next() % nCols;
                p2.X = r.Next() % nRows; p2.Y = r.Next() % nCols;

                if (display[p1.X, p1.Y] >= frames.Count) continue;
                if (display[p2.X, p2.Y] >= frames.Count) continue;

                distancesBefore = NeighborhoodSimilarity(distanceMatrix, p1, radius, display)
                                    + NeighborhoodSimilarity(distanceMatrix, p2, radius, display);

                temp = display[p1.X, p1.Y];
                display[p1.X, p1.Y] = display[p2.X, p2.Y];
                display[p2.X, p2.Y] = temp;

                distancesAfter = NeighborhoodSimilarity(distanceMatrix, p1, radius, display)
                                    + NeighborhoodSimilarity(distanceMatrix, p2, radius, display);

                if (distancesAfter > distancesBefore)
                {
                    temp = display[p1.X, p1.Y];
                    display[p1.X, p1.Y] = display[p2.X, p2.Y];
                    display[p2.X, p2.Y] = temp;
                }
            }

            // map 2D array of indexes to 2D array of frames
            RankedFrame[,] sortedFrames = new RankedFrame[nRows, nCols];
            for (int i = 0; i < nRows; i++)
                for (int j = 0; j < nCols; j++)
                    if (display[i, j] < frames.Count)
                        sortedFrames[i, j] = frames[display[i, j]];

            return sortedFrames;
        }

        private static int[,] InitializeDisplay(int nRows, int nCols)
        {
            int[,] display = new int[nRows, nCols];
            int c = 0, third = nCols / 3;
            for (int i = 0; i < nRows; i++)
                for (int j = 0; j < third; j++)
                    display[i, j] = c++;

            for (int i = 0; i < nRows; i++)
                for (int j = third; j < third * 2; j++)
                    display[i, j] = c++;

            for (int i = 0; i < nRows; i++)
                for (int j = third * 2; j < nCols; j++)
                    display[i, j] = c++;
            return display;
        }

        private static double NeighborhoodSimilarity(double[,] distances, System.Drawing.Point p, int radius, int[,] display)
        {
            double result = 0.0f;
            int nRows = display.GetLength(0);

            int fromRow = Math.Max(0, p.X - radius);
            int fromColumn = Math.Max(0, p.Y - radius);
            int toRow = Math.Min(nRows, p.X + radius);
            int toColumn = Math.Min(nRows, p.Y + radius);
            int pIdxToDistances = display[p.X, p.Y];

            for (int x = fromRow; x < toRow; x++)
                for (int y = fromColumn; y < toColumn; y++)
                    result += distances[display[x, y], pIdxToDistances];

            return result;
        }
    }
}
