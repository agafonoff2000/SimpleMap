using ProgramMain.Map.Spatial.Types;

namespace ProgramMain.Map.Spatial.Indexer
{
    public class SpatialLevelPowerIndexer<TNode> where TNode : ISpatialTreeNode
    {
        private readonly SpatialSheetPowerTypes[] _power;
        private readonly SpatialTree<TNode> _tree;

        internal SpatialLevelPowerIndexer(SpatialTree<TNode> tree)
        {
            _tree = tree;
            _power = new SpatialSheetPowerTypes[_tree.SpatialDepth];
        }

        public SpatialSheetPowerTypes this[int level]
        {
            get
            {
                if (level > 0 && level <= _power.Length)
                    return _power[level - 1];
                return SpatialSheetPowerTypes.None;
            }
            set
            {
                if (_tree.NodeCount > 0) return;

                if (level > 0 && level <= _power.Length)
                    _power[level - 1] = value;
            }
        }

        //Zero level is virtual and always "None"
        public int Length 
        {
            get { return _power.Length + 1; }
        }
    }
}
