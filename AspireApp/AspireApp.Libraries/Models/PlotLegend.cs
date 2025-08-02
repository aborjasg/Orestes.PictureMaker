using SkiaSharp;

namespace AspireApp.Libraries.Models
{
    public class PlotLegend
    {
        /// <summary>
        /// 
        /// </summary>
        public int[] Point { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public int[] Size { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public SKColor[] Colors { get; set; } = [];
    }
}
