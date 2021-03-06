using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VitretTool.EvaluationServer.Controls {
    public interface IChartLine {
        IEnumerable<Point> Points { get; }
        Color Color { get; }
    }
}
