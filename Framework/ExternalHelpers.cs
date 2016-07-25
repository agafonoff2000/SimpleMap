using System.Drawing;

namespace ProgramMain.Framework
{
    public static class ExternalHelpers
    {

        public static object Clone(this Rectangle src)
        {
            return new Rectangle(src.Location, src.Size);
        }

        public static bool Contains(this Point pt, Rectangle rect)
        {
            return pt.X >= rect.Left && pt.X <= rect.Right
                && pt.Y >= rect.Top && pt.Y <= rect.Bottom;
        }
    }
}
