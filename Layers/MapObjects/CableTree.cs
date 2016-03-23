using System.Data;
using System.Threading.Tasks;
using ProgramMain.ExampleDb;
using ProgramMain.Layers.MapObjects.TreeNodes;
using ProgramMain.Map.Spatial;
using ProgramMain.Map.Spatial.Types;

namespace ProgramMain.Layers.MapObjects
{
    public class CableTree : SpatialTree<CableNode>
    {
        public SimpleMapDb.CablesDataTable CableDbRows { get; private set; }

        public CableTree()
            : base(SpatialSheetPowerTypes.Ultra, SpatialSheetPowerTypes.High, SpatialSheetPowerTypes.Low, SpatialSheetPowerTypes.Low)
        {
            CableDbRows = new SimpleMapDb.CablesDataTable();
        }

        protected void Insert(SimpleMapDb.CablesRow row)
        {
            base.Insert(row);
        }

        protected void Delete(SimpleMapDb.CablesRow row)
        {
            base.Delete(row);
        }
        
        public void LoadData()
        {
            Clear();

            //read data from db here

            Parallel.ForEach(CableDbRows, Insert);
        }

        public void MergeData(SimpleMapDb.CablesDataTable cables)
        {
            if (CableDbRows == null) return;

            CableDbRows.Merge(cables, false, MissingSchemaAction.Error);

            Parallel.ForEach(cables, row =>
            {
                var newRow = CableDbRows.FindByID(row.ID);
                Insert(newRow);
            });

            //apply changes to db here
        }

        public bool RemoveCable(int objectId)
        {
            var row = (CableDbRows == null) ? null : CableDbRows.FindByID(objectId);
            if (row != null)
            {
                Delete(row);

                CableDbRows.Rows.Remove(row);
                //apply changes to db here

                return true;
            }
            return false;
        }

        public SimpleMapDb.CablesRow GetCable(int objectId)
        {
            if (CableDbRows == null) return null;

            var row = CableDbRows.FindByID(objectId);
            if (row != null)
            {
                var dt = (SimpleMapDb.CablesDataTable)CableDbRows.Clone();
                dt.ImportRow(row);
                return (SimpleMapDb.CablesRow)dt.Rows[0];
            }
            return null;
        }
    }
}
