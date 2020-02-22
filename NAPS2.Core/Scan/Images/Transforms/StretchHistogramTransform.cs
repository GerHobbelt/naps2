using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class StretchHistogramTransform : Transform
    {
        public HistogramStretchParams Parameters { get; set; } = new HistogramStretchParams();

        public override Bitmap Perform(Bitmap bitmap)
        {
            EnsurePixelFormat(ref bitmap);
            UnsafeImageOps.StretchHistogram(bitmap, 
                (byte)Parameters.RedMin, (byte)Parameters.RedMax, 
                (byte)Parameters.GreenMin, (byte)Parameters.GreenMax, 
                (byte)Parameters.BlueMin, (byte)Parameters.BlueMax);
            return bitmap;
        }

        public override bool IsNull => Parameters == null || Parameters.IsNull;
    }
}
