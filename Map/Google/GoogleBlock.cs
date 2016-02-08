using System;
using System.Drawing;

namespace ProgramMain.Map.Google
{
    public class GoogleBlock : IComparable, ICloneable
    {
        public static readonly GoogleBlock Empty = new GoogleBlock();

        public const int BlockSize = 256;

        public Point Pt
        {
            get
            {
                return new Point(X, Y);
            }
        }

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Level { get; private set; }

        private GoogleBlock()
        {
            Level = 0;
            Y = 0;
            X = 0;
        }

        public GoogleBlock(int pX, int pY, int pLevel)
        {
            X = pX;
            Y = pY;
            Level = pLevel;
        }

        public GoogleBlock(Point pt, int pLevel)
        {
            X = pt.X;
            Y = pt.Y;

            Level = pLevel;
        }

        #region ICloneable Members
        public object Clone()
        {
            return new GoogleBlock(X, Y, Level);
        }
        #endregion

        #region IComparable Members
        public int CompareTo(Object obj)
        {
            var coords = (GoogleBlock)obj;
            if (coords.Level < Level)
                return -1;
            if (coords.Level > Level)
                return 1;
            if (coords.Y < Y)
                return -1;
            if (coords.Y > Y)
                return 1;
            if (coords.X < X)
                return -1;
            if (coords.X > X)
                return 1;
            return 0;
        }
        #endregion

        public static implicit operator GoogleCoordinate(GoogleBlock block)
        {
            return new GoogleCoordinate(block.X * BlockSize, block.Y * BlockSize, block.Level);
        }

        public static implicit operator GoogleRectangle(GoogleBlock block)
        {
            return new GoogleRectangle(
                block.X * BlockSize, block.Y * BlockSize,
                (block.X + 1) * BlockSize, (block.Y + 1) * BlockSize,
                block.Level);
        }
    }
}
