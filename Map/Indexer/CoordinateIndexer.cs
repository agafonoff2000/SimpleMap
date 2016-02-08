using System.Collections.Generic;

namespace ProgramMain.Map.Indexer
{
    public class CoordinateIndexer
    {
        private List<Coordinate> _values;

        public Coordinate this[int index]
        {
            get { return index >= 0 && index < _values.Count ? _values[index] : null; }
        }

        public int Count {get { return _values != null ? _values.Count : 0; }}
        
        public void Add(Coordinate coordinate)
        {
            if (_values == null)
                _values = new List<Coordinate>();
            _values.Add(coordinate);
        }

        public void Remove(Coordinate coordinate)
        {
            if (_values != null)
            {
                _values.Remove(coordinate);
            }
        }

        public void Destroy()
        {
            if (_values != null)
            {
                _values.Clear();
                _values = null;
            }
        }

        public bool HasChilds
        {
            get { return _values != null && _values.Count > 0; }
        }
    }
}
