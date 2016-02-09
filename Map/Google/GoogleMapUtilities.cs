using System;
using System.Net;

namespace ProgramMain.Map.Google
{
    internal class GoogleMapUtilities
    {
        #region Helpers to work with Google Coordinate system
        /// <summary>
        /// Block count on the side of google level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long NumTiles(int level)
        {
            return Convert.ToInt64(Math.Pow(2, (level - 1)));
        }

        /// <summary>
        /// Block count on the google level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long CountTiles(int level)
        {
            var numTiles = NumTiles(level);
            return numTiles * numTiles;
        }

        /// <summary>
        /// Translate block count to google level
        /// </summary>
        /// <param name="countTiles"></param>
        /// <returns></returns>
        public static long NumLevel(int countTiles)
        {
            return Convert.ToInt64(Math.Log(Math.Sqrt(countTiles), 2)) + 1;
        }

        /// <summary>
        /// Pixel count on the side of google level bitmap
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long BitMapSize(int level)
        {
            return NumTiles(level) * GoogleBlock.BlockSize;
        }

        /// <summary>
        /// Pixel count on the google level bitmap 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long BitmapOrigo(int level)
        {
            return BitMapSize(level) / 2;
        }

        /// <summary>
        /// Pixel count per degree on the google level bitmap
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static double PixelsPerDegree(int level)
        {
            return (double)BitMapSize(level) / 360;
        }

        /// <summary>
        /// Pixel count per radian on the google level bitmap
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static double PixelsPerRadian(int level)
        {
            const double p2 = 2 * Math.PI;
            return BitMapSize(level) / p2;
        }

        /// <summary>
        /// Translate longitude to X coordinate of the google level bitmap
        /// </summary>
        public static long GetGoogleX(Coordinate coordinate, int level)
        {
            return (long)(Math.Floor(BitmapOrigo(level) + coordinate.Longitude * PixelsPerDegree(level)));
        }

        /// <summary>
        /// Translate latitude to Y coordinate of the google level bitmap
        /// </summary>
        public static long GetGoogleY(Coordinate coordinate, int level)
        {
            const double d2R = Math.PI / 180;
            var z = (1 + Math.Sin(coordinate.Latitude * d2R)) / (1 - Math.Sin(coordinate.Latitude * d2R));
            return (long)(Math.Floor(BitmapOrigo(level) - 0.5 * Math.Log(z) * PixelsPerRadian(level)));
        }

        /// <summary>
        /// Translate X coordinate of the google level bitmap to longitude
        /// </summary>
        public static double GetLongitude(GoogleCoordinate google)
        {
            return Math.Round((google.X - BitmapOrigo(google.Level)) / PixelsPerDegree(google.Level), 5);
        }

        /// <summary>
        /// Translate Y coordinate of the google level bitmap to latitude
        /// </summary>
        public static double GetLatitude(GoogleCoordinate google)
        {
            const double r2D = 180 / Math.PI; 
            const double p2 = Math.PI / 2;
            var z = (google.Y - BitmapOrigo(google.Level)) / (-1 * PixelsPerRadian(google.Level));
            return Math.Round((2 * Math.Atan(Math.Exp(z)) - p2) * r2D, 5);
        }

        /// <summary>
        /// Get google bitmap block number X by longitude
        /// </summary>
        public static long GetNumBlockX(Coordinate coordinate, int level)
        {
            return (long)Math.Floor((double)GetGoogleX(coordinate, level) / GoogleBlock.BlockSize);
        }

        /// <summary>
        /// Get google bitmap block number Y by latitude
        /// </summary>
        public static long GetNumBlockY(Coordinate coordinate, int level)
        {
            return (long)Math.Floor((double)GetGoogleY(coordinate, level) / GoogleBlock.BlockSize);
        }
        #endregion

        /// <summary>
        /// Line cross
        /// </summary>
        public static bool CheckLinesInterseption(CoordinateRectangle line1, CoordinateRectangle line2)
        {
            double d = (line1.Left - line1.Right) * (line2.Bottom - line2.Top) - (line1.Top - line1.Bottom) * (line2.Right - line2.Left);

            if (Math.Abs(d) < 0.000000001)
                return false;

            double da = (line1.Left - line2.Left) * (line2.Bottom - line2.Top) - (line1.Top - line2.Top) * (line2.Right - line2.Left);
            double db = (line1.Left - line1.Right) * (line1.Top - line2.Top) - (line1.Top - line1.Bottom) * (line1.Left - line2.Left);

            double ta = da / d;
            double tb = db / d;

            return ((0 <= ta) && (ta <= 1) && (0 <= tb) && (tb <= 1));
        }

        /// <summary>
        /// Line middle point
        /// </summary>
        public static Coordinate GetMiddlePoint(Coordinate c1, Coordinate c2)
        {
            const double d2R = Math.PI / 180;
            const double r2D = 180 / Math.PI; 

            var dLon = d2R * (c2.Longitude - c1.Longitude);
            var c1Rlat = d2R * (c1.Latitude);
            var c2Rlat = d2R * (c2.Latitude);
            var bX = Math.Cos(c2Rlat) * Math.Cos(dLon);
            var bY = Math.Cos(c2Rlat) * Math.Sin(dLon);

            var longitude = Math.Round(c1.Longitude + r2D * (Math.Atan2(bY, Math.Cos(c1Rlat) + bX)), 5);
            var latitude = Math.Round(r2D * (Math.Atan2(Math.Sin(c1Rlat) + Math.Sin(c2Rlat), Math.Sqrt((Math.Cos(c1Rlat) + bX) * (Math.Cos(c1Rlat) + bX) + bY * bY))), 5);

            return new Coordinate(longitude, latitude);
        }

        /// <summary>
        /// Create Url to get bitmap block from google bitmap cache
        /// </summary>
        public static string CreateUrl(GoogleBlock block)
        {
            return String.Format(Properties.Settings.Default.GoogleUrl, block.X, block.Y, block.Level - 1);
        }

        /// <summary>
        /// Create web request to get bitmap block from google bitmap cache
        /// </summary>
        public static HttpWebRequest CreateGoogleWebRequest(GoogleBlock block)
        {
            var urlGoogle = CreateUrl(block);
            var oRequest = (HttpWebRequest)WebRequest.Create(urlGoogle);
            oRequest.UserAgent = "www.simplemap.ru"; //!!!must have to retrieve image from google
            return oRequest;
        }
    }
}
