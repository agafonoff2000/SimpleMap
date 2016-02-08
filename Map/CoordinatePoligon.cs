using ProgramMain.Map.Google;
using ProgramMain.Map.Indexer;
using ProgramMain.Map.Types;

namespace ProgramMain.Map
{
    public class CoordinatePoligon
    {
        public static readonly CoordinatePoligon Empty = new CoordinatePoligon();

        public readonly CoordinateIndexer Coordinates;

        public Coordinate First
        {
            get { return Coordinates[0]; }
        }

        public Coordinate Last
        {
            get { return Coordinates[Coordinates.Count - 1]; }
        }

        public CoordinateRectangle this[int index]
        {
            get
            {
                if (index < 0 || index > Count - 1)
                    return null;
                var leftTop = Coordinates[index];
                var rightBottom = index == Count - 1 ? Coordinates[0] : Coordinates[index + 1];
                return new CoordinateRectangle(leftTop, rightBottom);
            }
        }

        public int Count
        {
            get { return Coordinates.Count; }
        }

        public CoordinatePoligon()
        {
            Coordinates = new CoordinateIndexer();
        }

        public bool Add(Coordinate coordinate)
        {
            if (Count > 2)
            {
                var line1 = new CoordinateRectangle(First, coordinate);
                var line2 = new CoordinateRectangle(Last, coordinate);
                for (var i = 0; i < Count - 1; i++)
                {
                    if (GoogleMapUtilities.CheckLinesInterseption(this[i], line1)
                        || GoogleMapUtilities.CheckLinesInterseption(this[i], line2))
                        return false;
                }
            }

            Coordinates.Add(coordinate);
            return true;
        }

        public double PoligonDistance(Coordinate coordinate)
        {
            double res = 0;

            for (var i = 0; i < Count; i++)
            {
                var distance = this[i].LineDistance(coordinate);
                if (i == 0 || distance < res)
                    res = distance;
            }
            return res;
        }

        public bool IncludeTo(CoordinateRectangle rect)
        {
            for (var i = 0; i < Coordinates.Count; i++)
            {
                if (rect.PointContains(Coordinates[i]) != InterseptResult.Contains)
                    return false;
            }
            return true;
        }

        public InterseptResult PointContains(Coordinate coordinate)
        {
            //to do

            return InterseptResult.None;
        }

        public InterseptResult LineContains(CoordinateRectangle coordinate)
        {
            //to do

            return InterseptResult.None;
        }

        public InterseptResult RectangleContains(CoordinateRectangle coordinate)
        {
            //to do

            return InterseptResult.None;
        }

        public InterseptResult PoligonContains(CoordinateRectangle coordinate)
        {
            //to do

            return InterseptResult.None;
        }
    }
}
