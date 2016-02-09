using System;
using System.Drawing;
using ProgramMain.Map.Google;
using ProgramMain.Map.Types;

namespace ProgramMain.Map
{
    public class CoordinateRectangle : ICloneable
    {
        public static readonly CoordinateRectangle Empty = new CoordinateRectangle();

        public Coordinate LeftTop
        {
            get
            {
                return new Coordinate(Left, Top);
            }
        }

        public Coordinate RightBottom
        {
            get
            {
                return new Coordinate(Right, Bottom);
            }
        }

        public Coordinate LeftBottom
        {
            get
            {
                return new Coordinate(Left, Bottom);
            }
        }

        public Coordinate RightTop
        {
            get
            {
                return new Coordinate(Right, Top);
            }
        }

        public double Width
        {
            get { return Right - Left; }
        }

        public double Height
        {
            get { return Bottom - Top; }
        }

        public double Left { get; private set; }

        public double Right { get; private set; }

        public double Top { get; private set; }

        public double Bottom { get; private set; }

        private CoordinateRectangle()
        {
            Bottom = 0;
            Top = 0;
            Right = 0;
            Left = 0;
        }

        public CoordinateRectangle(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public CoordinateRectangle(decimal left, decimal top, decimal right, decimal bottom)
        {
            Left = (double)left;
            Top = (double)top;
            Right = (double)right;
            Bottom = (double)bottom;
        }

        public CoordinateRectangle(Coordinate pLeftTop, Coordinate pRightBottom)
        {
            Left = pLeftTop.Longitude;
            Top = pLeftTop.Latitude;
            Right = pRightBottom.Longitude;
            Bottom = pRightBottom.Latitude;
        }

        #region ICloneable Members
        public object Clone()
        {
            return new CoordinateRectangle(Left, Top, Right, Bottom);
        }
        #endregion


        public override string ToString()
        {
            return (String.Format("E{0:F5} N{1:F5} - E{2:F5} N{3:F5} : {4:n}m", Left, Top, Right, Bottom, LineLength));
        }

        public Rectangle GetScreenRect(GoogleRectangle screenView)
        {
            return new GoogleRectangle(this, screenView.Level).GetScreenRect(screenView);
        }

        public InterseptResult PointContains(Coordinate point)
        {
            return (point.Longitude >= Left
                && point.Longitude <= Right
                && point.Latitude <= Top
                && point.Latitude >= Bottom) ? InterseptResult.Contains : InterseptResult.None;
        }

        public InterseptResult RectangleContains(CoordinateRectangle rectangle)
        {
            var iLeftTop = PointContains(rectangle.LeftTop) != InterseptResult.None;
            var iRightBottom = PointContains(rectangle.RightBottom) != InterseptResult.None;

            if (iLeftTop && iRightBottom)
                return InterseptResult.Contains;
            if (iLeftTop || iRightBottom)
                return InterseptResult.Intersepts;

            if (PointContains(rectangle.LeftBottom) != InterseptResult.None)
                return InterseptResult.Intersepts;
            if (PointContains(rectangle.RightTop) != InterseptResult.None)
                return InterseptResult.Intersepts;

            iLeftTop = rectangle.PointContains(LeftTop) != InterseptResult.None;
            iRightBottom = rectangle.PointContains(RightBottom) != InterseptResult.None;

            if (iLeftTop && iRightBottom)
                return InterseptResult.Supersets;
            if (iLeftTop || iRightBottom)
                return InterseptResult.Intersepts;

            if (rectangle.PointContains(LeftBottom) != InterseptResult.None)
                return InterseptResult.Intersepts;
            if (rectangle.PointContains(RightTop) != InterseptResult.None)
                return InterseptResult.Intersepts;

            if (GoogleMapUtilities.CheckLinesInterseption(new CoordinateRectangle(Left, Top, Left, Bottom),
                    new CoordinateRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top)))
                return InterseptResult.Intersepts;
            if (GoogleMapUtilities.CheckLinesInterseption(new CoordinateRectangle(Left, Top, Right, Top),
                    new CoordinateRectangle(rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom)))
                return InterseptResult.Intersepts;

            return InterseptResult.None;
        }

        public InterseptResult LineContains(CoordinateRectangle line)
        {
            var iLeftTop = PointContains(line.LeftTop) != InterseptResult.None;
            var iRightBottom = PointContains(line.RightBottom) != InterseptResult.None;

            if (iLeftTop && iRightBottom)
                return InterseptResult.Contains;
            if (iLeftTop || iRightBottom)
                return InterseptResult.Intersepts;
            if (GoogleMapUtilities.CheckLinesInterseption(new CoordinateRectangle(Left, Top, Right, Top), line))
                return InterseptResult.Intersepts;
            if (GoogleMapUtilities.CheckLinesInterseption(new CoordinateRectangle(Right, Top, Right, Bottom), line))
                return InterseptResult.Intersepts;
            if (GoogleMapUtilities.CheckLinesInterseption(new CoordinateRectangle(Left, Bottom, Right, Bottom), line))
                return InterseptResult.Intersepts;
            if (GoogleMapUtilities.CheckLinesInterseption(new CoordinateRectangle(Left, Top, Left, Bottom), line))
                return InterseptResult.Intersepts;
            return InterseptResult.None;
        }

        public InterseptResult PoligonContains(CoordinatePoligon poligon)
        {
            if (poligon.IncludeTo(this))
                return InterseptResult.Contains;

            for (var i = 0; i < poligon.Count; i++)
            {
                if (LineContains(poligon[i]) != InterseptResult.None)
                    return InterseptResult.Intersepts;
            }

            if (poligon.PointContains(LeftTop) != InterseptResult.None
                && poligon.PointContains(RightTop) != InterseptResult.None
                && poligon.PointContains(RightBottom) != InterseptResult.None
                && poligon.PointContains(LeftBottom) != InterseptResult.None)
                return InterseptResult.Supersets;

            return InterseptResult.None;
        }

        public double RectangeDistance(Coordinate coordinate)
        {
            if (PointContains(coordinate) != InterseptResult.None)
                return 0;

            var min = EarthUtilities.GetDistance(new CoordinateRectangle(Left, Top, Right, Top), coordinate);
            var res = EarthUtilities.GetDistance(new CoordinateRectangle(Right, Top, Right, Bottom), coordinate);
            if (res < min) min = res;
            res = EarthUtilities.GetDistance(new CoordinateRectangle(Right, Bottom, Left, Bottom), coordinate);
            if (res < min) min = res;
            res = EarthUtilities.GetDistance(new CoordinateRectangle(Left, Bottom, Left, Top), coordinate);
            if (res < min) min = res;

            return min;
        }

        public double LineDistance(Coordinate coordinate)
        {
            return EarthUtilities.GetDistance(this, coordinate);
        }

        public Coordinate LineMiddlePoint
        {
            get { return GoogleMapUtilities.GetMiddlePoint(LeftTop, RightBottom); }
        }

        public double LineLength
        {
            get { return LeftTop.Distance(RightBottom); }
        }

        public double LineAngle
        {
            get { return Math.Atan2(Width, Height); }
        }

        public void LineGrow(double meter)
        {
            var len = LineLength;
            var ang = LineAngle;
            Right = Left + (len + meter) * Math.Cos(ang);
            Bottom = Top + (len + meter) * Math.Sin(ang);
        }

        public Coordinate GetNearestPoint(Coordinate pt)
        {
            return EarthUtilities.GetNearestPoint(this, pt);
        }
    }
}