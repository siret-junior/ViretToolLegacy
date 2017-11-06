using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VitretTool.EvaluationServer {
    class ColorHelper {

        public static Color StringToColor(string color) {
            //https://stackoverflow.com/questions/309149/generate-distinctly-different-rgb-colors-in-graphs/309193#309193
            //int hash = 0;
            //for (var i = 0; i < color.Length; i++) {
            //    hash = color[i] + ((hash << 5) - hash);
            //}

            byte[] inputBytes = Encoding.Unicode.GetBytes(color);
            byte[] hashedBytes = MD5.Create().ComputeHash(inputBytes);
            int hash = BitConverter.ToInt32(hashedBytes, 0);

            var c = (hash & 0x00FFFFFF).ToString("X");
            var hex = "00000".Substring(0, 6 - c.Length) + c;

            return (Color)ColorConverter.ConvertFromString("#" + hex);
        }

        public static Color GetPredefiniedColor(int index) {
            index = index % 18;
            return (Color)ColorConverter.ConvertFromString("#" + ColourValues[index]);
        }

        public static string[] ColourValues = new string[] {
            "e6194b", "3cb44b", "ffe119", "0082c8", "f58231", "911eb4",
            "46f0f0", "f032e6", "d2f53c", "fabebe", "008080", "e6beff",
            "800000", "aaffc3", "808000", "000080", "aa6e28", "fffac8"
        };

    }
}
