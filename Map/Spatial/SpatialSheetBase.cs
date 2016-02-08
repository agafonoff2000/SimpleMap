using ProgramMain.Map.Google;
using ProgramMain.Map.Spatial.Indexer;
using ProgramMain.Map.Spatial.Types;

namespace ProgramMain.Map.Spatial
{
    internal class SpatialSheetBase<TNode> where TNode : ISpatialTreeNode
    {
        public int Level { get; private set; }
        public int GoogleLevel { get; private set; }

        //массив для хранения дочерних уровней в индекса
        public readonly SpatialSheetIndexer<TNode> Sheets;

        //массив для храрения элементов (поддерживаются точки, линии и прямоугольники)
        public readonly SpatialContentIndexer<TNode> Content;

        public bool IsEmpty
        {
            get
            {
                return !Sheets.HasChilds && !Content.HasChilds;
            }
        }

        public bool IsBottomSheet
        {
            get { return Level >= Sheets.PowerLength; }
        }

        public SpatialSheetBase(SpatialTree<TNode> tree, int level, int googleLevel)
        {
            Level = level;
            GoogleLevel = googleLevel;

            Sheets = new SpatialSheetIndexer<TNode>(this, tree);
            Content = new SpatialContentIndexer<TNode>();
        }

        private int GoogleNumLevel(int level)
        {
            return (int)GoogleMapUtilities.NumLevel((int)Sheets.Power(level));
        }

        private int GoogleNextLevelAddon()
        {
            return GoogleNumLevel(Level) - 1;
        }

        public int NextGoogleLevel
        {
            get { return GoogleLevel + GoogleNextLevelAddon(); }
        }

        public void Clear()
        {
            lock (this)
            {
                if (!IsBottomSheet)
                {
                    if (Sheets.HasChilds)
                    {
                        foreach (var sheet in Sheets.Values)
                        {
                            sheet.Clear();
                        }
                        Sheets.Destroy();
                    }
                }
                else
                {
                    Content.Destroy();
                }
            }
        }
    }
}