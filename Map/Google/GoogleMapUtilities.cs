using System;
using System.Net;

namespace ProgramMain.Map.Google
{
    internal class GoogleMapUtilities
    {
        #region Вспомогательные функциии для преобразование географических координат в координаты GoggleMaps
        /// <summary>
        /// Определяем количество блоков вдоль одной стороны битмапа уровня level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long NumTiles(int level)
        {
            return Convert.ToInt64(Math.Pow(2, (level - 1)));
        }

        /// <summary>
        /// Определяем количество блоков битмапа уровня level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long CountTiles(int level)
        {
            long numTiles = NumTiles(level);
            return numTiles * numTiles;
        }

        /// <summary>
        /// Определяем уровнень битмапа по колличеству блоков в level
        /// </summary>
        /// <param name="countTiles"></param>
        /// <returns></returns>
        public static long NumLevel(int countTiles)
        {
            return Convert.ToInt64(Math.Log(Math.Sqrt(countTiles), 2)) + 1;
        }

        /// <summary>
        /// Определяем размер в пикселях одной стороны битмапа уровня level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long BitMapSize(int level)
        {
            return NumTiles(level) * GoogleBlock.BlockSize;
        }

        /// <summary>
        /// Определяем координаты в пикселах середины Битмапа уровня level 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static long BitmapOrigo(int level)
        {
            return BitMapSize(level) / 2;
        }

        /// <summary>
        /// Определяем количество пикселей, приходящееся на один градус долготы на Битмапе уровня level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static double PixelsPerLonDegree(int level)
        {
            return (double)BitMapSize(level) / 360;
        }

        /// <summary>
        /// Определяем количество пикселей, приходящееся на один радиан долготы на Битмапе уровня level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static double PixelsPerLonRadian(int level)
        {
            const double p2 = 2 * Math.PI;
            return BitMapSize(level) / p2;
        }

        /// <summary>
        /// Преобразование географической долготы в координату X Googl-a
        /// </summary>
        public static long GetGoogleX(Coordinate coordinate, int level)
        {
            return (long)(Math.Floor(BitmapOrigo(level) + coordinate.Longitude * PixelsPerLonDegree(level)));
        }

        /// <summary>
        /// Преобразование географической широты в координату Y Googl-a
        /// </summary>
        public static long GetGoogleY(Coordinate coordinate, int level)
        {
            const double d2R = Math.PI / 180; // Константа для преобразования градусов в радианы
            var z = (1 + Math.Sin(coordinate.Latitude * d2R)) / (1 - Math.Sin(coordinate.Latitude * d2R));
            return (long)(Math.Floor(BitmapOrigo(level) - 0.5 * Math.Log(z) * PixelsPerLonRadian(level)));
        }

        /// <summary>
        /// Преобразование координаты X Googl-a в географическую долготу
        /// </summary>
        public static double GetLongitude(GoogleCoordinate google)
        {
            return Math.Round((google.X - BitmapOrigo(google.Level)) / PixelsPerLonDegree(google.Level), 5);
        }

        /// <summary>
        /// Преобразование координаты Y Googl-a в географическую широту
        /// </summary>
        public static double GetLatitude(GoogleCoordinate google)
        {
            const double r2D = 180 / Math.PI; // Константа для преобразования радиан в градусы 
            const double p2 = Math.PI / 2;
            var z = (google.Y - BitmapOrigo(google.Level)) / (-1 * PixelsPerLonRadian(google.Level));
            return Math.Round((2 * Math.Atan(Math.Exp(z)) - p2) * r2D, 5);
        }

        /// <summary>
        /// Номер блока для координаты X Googl-a по широте
        /// </summary>
        public static long GetNumBlockX(Coordinate coordinate, int level)
        {
            return (long)Math.Floor((double)GetGoogleX(coordinate, level) / GoogleBlock.BlockSize);
        }

        /// <summary>
        /// Номер блока для координаты Y Googl-a по долготе
        /// </summary>
        public static long GetNumBlockY(Coordinate coordinate, int level)
        {
            return (long)Math.Floor((double)GetGoogleY(coordinate, level) / GoogleBlock.BlockSize);
        }
        #endregion

        /// <summary>
        /// Пересечение отрезков c заданными координатами
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
        /// Центральная точка на отрезке
        /// Передаваемые широта/долгота в градусах и сотых долях
        /// </summary>
        public static Coordinate GetMiddlePoint(Coordinate c1, Coordinate c2)
        {
            // Константы, используемые для вычисления смещения и расстояния
            const double d2R = Math.PI / 180; // Константа для преобразования градусов в радианы
            const double r2D = 180 / Math.PI; // Константа для преобразования радиан в градусы 

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
        /// Создать URL для запроса картинки у Googl-a c заданными координатами
        /// </summary>
        public static string CreateUrl(GoogleBlock block)
        {
            return String.Format(Properties.Settings.Default.GoogleUrl, block.X, block.Y, block.Level - 1);
        }

        /// <summary>
        /// Создать запрос картинки у Googl-a c заданными координатами
        /// </summary>
        public static HttpWebRequest CreateGoogleWebRequest(GoogleBlock block)
        {
            var urlGoogle = CreateUrl(block);
            var oRequest = (HttpWebRequest)WebRequest.Create(urlGoogle);
            oRequest.UserAgent = "www.simplemap.ru"; //Это обязательно, иначе Google не вернет картинку!
            return oRequest;
        }
    }
}
