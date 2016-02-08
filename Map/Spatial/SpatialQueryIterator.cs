using System.Globalization;

namespace ProgramMain.Map.Spatial
{
    internal class SpatialQueryIterator
    {
        public int Value { get; private set; }
        
        private SpatialQueryIterator()
        {
            Value = 0;
        }

        public static SpatialQueryIterator Start()
        {
            return new SpatialQueryIterator();
        }

        public void Next()
        {
            lock (this)
            {
                Value++;
            }
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
