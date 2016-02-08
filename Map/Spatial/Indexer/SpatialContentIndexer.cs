using System.Collections.Generic;
using ProgramMain.Map.Spatial.Types;

namespace ProgramMain.Map.Spatial.Indexer
{
    internal class SpatialContentIndexer<TNode> where TNode : ISpatialTreeNode
    {
        private SortedDictionary<int, TNode> _content;

        public void Add(TNode node)
        {
            if (_content == null)
                _content = new SortedDictionary<int, TNode>();
            _content[node.RowId] = node;
        }

        public void Remove(TNode node)
        {
            if (_content != null)
            {
                _content.Remove(node.RowId);
            }
        }

        public SortedDictionary<int, TNode>.ValueCollection Values
        {
            get { return _content != null ? _content.Values : null; }
        }

        public void Destroy()
        {
            if (_content != null)
            {
                _content.Clear();
                _content = null;
            }
        }

        public bool HasChilds
        {
            get { return _content != null && _content.Count > 0; }
        }
    }
}
