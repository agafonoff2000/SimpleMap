using System;

namespace ProgramMain.Map
{
    internal class EarthUtilities
    {
        #region Вспомогательные функциии для расчета растояний для земного шара с допуском, что все объекты находятся на высоте 0 метров над уровнем моря

        /// <summary>
        /// Приблизительное растояние между двумя точками
        /// Передаваемые широта/долгота в градусах и сотых долях
        /// </summary>
        public static double GetLength(Coordinate c1, Coordinate c2)
        {
            // Константы, используемые для вычисления смещения и расстояния
            const double d2R = Math.PI / 180; // Константа для преобразования градусов в радианы
            const double a = 6378137.0; // Основные полуоси
            //const double b = 6356752.314245; // Неосновные полуоси
            const double e2 = 0.006739496742337; // Квадрат эксцентричности эллипсоида

            // Вычисляем разницу между двумя долготами и широтами и получаем среднюю широту
            var fdLambda = (c1.Longitude - c2.Longitude) * d2R; // Разница между двумя значениями долготы
            var fdPhi = (c1.Latitude - c2.Latitude) * d2R; // Разница между двумя значениями широты
            var fPhimean = ((c1.Latitude + c2.Latitude) / 2.0) * d2R; // Средняя широта

            // Меридианский радиус кривизны
            var fRho = (a * (1 - e2)) / Math.Pow(1 - e2 * (Math.Pow(Math.Sin(fPhimean), 2)), 1.5);
            // Поперечный радиус кривизны
            var fNu = a / (Math.Sqrt(1 - e2 * (Math.Sin(fPhimean) * Math.Sin(fPhimean))));

            // Вычисляем угловое расстояние от центра сфероида
            var fz = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(fdPhi / 2.0), 2) + Math.Cos(c2.Latitude * d2R) * Math.Cos(c1.Latitude * d2R) * Math.Pow(Math.Sin(fdLambda / 2.0), 2)));

            // Вычисляем смещение
            var fAlpha = Math.Asin(Math.Cos(c2.Latitude * d2R) * Math.Sin(fdLambda) * 1 / Math.Sin(fz));
            // Вычисляем радиус Земли
            var fR = (fRho * fNu) / ((fRho * Math.Pow(Math.Sin(fAlpha), 2)) + (fNu * Math.Pow(Math.Cos(fAlpha), 2)));

            // Вычисленное расстояния в метрах
            var distance = fz * fR;

            return distance;
        }

        /// <summary>
        /// Приблизительное растояние от точки до отрезка прямой
        /// Передаваемые широта/долгота в градусах и сотых долях
        /// </summary>
        public static double GetDistance(CoordinateRectangle line, Coordinate pt)
        {
            const double r2D = 180 / Math.PI; // Константа для преобразования радиан в градусы 

            var a = GetLength(line.LeftTop, line.RightBottom);

            var b = GetLength(line.LeftTop, pt);
            var c = GetLength(line.RightBottom, pt);
            if (a <= 0)
                return (b + c) / 2;

            var enB = Math.Acos((Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2)) / (2 * a * b)) * r2D;
            if (enB >= 90)
                return b;
            var enC = Math.Acos((Math.Pow(a, 2) + Math.Pow(c, 2) - Math.Pow(b, 2)) / (2 * a * c)) * r2D;
            if (enC >= 90)
                return c;

            var s = (a + b + c) / 2;
            var ar = Math.Sqrt(s * (s - a) * (s - b) * (s - c));

            return ar * 2 / a;
        }

        public static Coordinate GetNearestPoint(CoordinateRectangle line, Coordinate pt)
        {

            const double r2D = 180 / Math.PI; // Константа для преобразования радиан в градусы 

           var a = GetLength(line.LeftTop, line.RightBottom);

            var b = GetLength(line.LeftTop, pt);
            var c = GetLength(line.RightBottom, pt);
            if (a <= 0)
                return pt;

            var enB = Math.Acos((Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2)) / (2 * a * b)) * r2D;
            if (enB >= 90)
                return pt;
            var enC = Math.Acos((Math.Pow(a, 2) + Math.Pow(c, 2) - Math.Pow(b, 2)) / (2 * a * c)) * r2D;
            if (enC >= 90)
                return pt;


            var x = ((line.Right - line.Left)*(line.Bottom - line.Top)*(pt.Latitude - line.Top) +
                        line.Left*Math.Pow(line.Bottom - line.Top, 2) + pt.Longitude*Math.Pow(line.Right - line.Left, 2))/
                       (Math.Pow(line.Bottom - line.Top, 2) + Math.Pow(line.Right - line.Left, 2));
            var y =(line.Bottom - line.Top)*(x-line.Left)/(line.Right - line.Left)+line.Top;

            return new Coordinate(x,y);
        }

        #endregion
    }
}
