
using Windows.Foundation;
namespace IWalker.Util
{
    /// <summary>
    /// We can't put a Foundation Size into JSON, so this will take its place in the cache.
    /// </summary>
    public class IWalkerSize
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    static class IWalkerSizeHelpers
    {
        /// <summary>
        /// Return the Size as a IWalkerSize.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IWalkerSize ToIWalkerSize(this Size s)
        {
            return new IWalkerSize() { Height = s.Height, Width = s.Width };
        }
    }
}
