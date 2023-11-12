using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfAppIiko
{
    public class RTable
    {
        public Guid id { get; set; }
        public string? name { get; set; }

        public string? status { get; set; }

        public SolidColorBrush GetColor()
        {
            switch (status)
            {
                case "OK":
                    return Brushes.White;
                case "INORDER":
                    return Brushes.Red;
                case "RESERVERD":
                    return Brushes.Orange;
                case "DISABLED":
                    return Brushes.Gray;
                default:
                    return Brushes.Blue;
            }
        }
    }
}
