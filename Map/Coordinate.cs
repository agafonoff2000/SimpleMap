using System;
using System.Drawing;
using ProgramMain.Map.Google;

namespace ProgramMain.Map
{
    public class Coordinate : IComparable, ICloneable
    {
        public static readonly Coordinate Empty = new Coordinate();

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        private Coordinate()
        {
            Latitude = 0;
            Longitude = 0;
        }

        public Coordinate(double pLongitude, double pLatitude)
        {
            Longitude = pLongitude;
            Latitude = pLatitude;
        }

        public Coordinate(decimal pLongitude, decimal pLatitude)
        {
            Longitude = (double)pLongitude;
            Latitude = (double)pLatitude;
        }

        #region ICloneable Members
        public object Clone()
        {
            return new Coordinate(Longitude, Latitude);
        }
        #endregion

        #region IComparable Members
        public int CompareTo(Object obj)
        {
            var coords = (Coordinate)obj;
            if (coords.Latitude < Latitude)
                return -1;
            if (coords.Latitude > Latitude)
                return 1;
            if (coords.Longitude < Longitude)
                return -1;
            if (coords.Longitude > Longitude)
                return 1;
            return 0;
        }
        #endregion


        public override string ToString()
        {
            return (String.Format("E{0:F5} N{1:F5}", Longitude, Latitude));
        }

        public static Coordinate operator + (Coordinate coordinate, GoogleCoordinate addon)
        {
            return new GoogleCoordinate(coordinate, addon.Level) + addon;
        }
        
        private GoogleCoordinate GetLeftTopGoogle(int screenWidth, int screenHeight, int level)
        {
            return new GoogleCoordinate(
                GoogleMapUtilities.GetGoogleX(this, level) - ((screenWidth + 1) / 2 - 1),
                GoogleMapUtilities.GetGoogleY(this, level) - ((screenHeight + 1) / 2 - 1),
                level);
        }

        private GoogleCoordinate GetRightBottomGoogle(int screenWidth, int screenHeight, int level)
        {
            return new GoogleCoordinate(
                GoogleMapUtilities.GetGoogleX(this, level) + ((screenWidth - 1) / 2 + 1),
                GoogleMapUtilities.GetGoogleY(this, level) + ((screenHeight - 1) / 2 + 1),
                level);
        }

        public GoogleRectangle GoogleScreenViewFromCenter(int screenWidth, int screenHeight, int level)
        {
            return new GoogleRectangle(GetLeftTopGoogle(screenWidth, screenHeight, level), GetRightBottomGoogle(screenWidth, screenHeight, level));
        }

        public Point GetScreenPoint(GoogleRectangle googleScreenView)
        {
            return new GoogleCoordinate(this, googleScreenView.Level).GetScreenPoint(googleScreenView);
        }

        public static Coordinate CoordinateFromScreen(GoogleRectangle googleScreenView, Point point)
        {
            return googleScreenView.LeftTop + new GoogleCoordinate(point.X, point.Y, googleScreenView.Level);
        }

        public GoogleBlock GetGoogleBlock(int level)
        {
            return new GoogleCoordinate(this, level);
        }

        public double Distance(Coordinate coordinate)
        {
            return EarthUtilities.GetLength(this, coordinate);
        }
    }
}
