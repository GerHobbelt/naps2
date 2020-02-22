using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAPS2.Scan
{
    [Serializable]
    public class HistogramStretchParams
    {
        // Default: Samsung SCX4500
        public int RedMin { get; set; } = 45;
        public int RedMax { get; set; } = 225;
        public int GreenMin { get; set; } = 50;
        public int GreenMax { get; set; } = 250;
        public int BlueMin { get; set; } = 55;
        public int BlueMax { get; set; } = 250;

        public bool IsNull => 
            RedMin == 0 && RedMax == 255 &&
            GreenMin == 0 && GreenMax == 255 &&
            BlueMin == 0 && BlueMax == 255;

        internal HistogramStretchParams Clone() => (HistogramStretchParams)MemberwiseClone();
    }
}
