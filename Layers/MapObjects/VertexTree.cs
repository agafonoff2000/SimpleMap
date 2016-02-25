using System.Data;
using System.Threading.Tasks;
using ProgramMain.ExampleDb;
using ProgramMain.Layers.MapObjects.TreeNodes;
using ProgramMain.Map.Spatial;
using ProgramMain.Map.Spatial.Types;

namespace ProgramMain.Layers.MapObjects
{
    public class VertexTree : SpatialTree<VertexNode>
    {
        public SimpleMapDb.VertexesDataTable VertexDbRows { get; private set;}

        public VertexTree()
            : base(SpatialSheetPowerTypes.Ultra, SpatialSheetPowerTypes.Extra, SpatialSheetPowerTypes.Medium, SpatialSheetPowerTypes.Low)
        {
            VertexDbRows = new SimpleMapDb.VertexesDataTable();
        }

        public void Insert(SimpleMapDb.VertexesRow row)
        {
            Insert(new VertexNode(row));
        }

        public void Delete(SimpleMapDb.VertexesRow row)
        {
            Delete(new VertexNode(row));
        }

        public void LoadData()
        {
            Clear();

            //read data from db here

            Parallel.ForEach(VertexDbRows, Insert);
        }

        public void MergeData(SimpleMapDb.VertexesDataTable vertexes)
        {
            if (VertexDbRows == null) return;

            VertexDbRows.Merge(vertexes, false, MissingSchemaAction.Error);

            Parallel.ForEach(vertexes, row =>
            {
                var newRow = VertexDbRows.FindByID(row.ID);
                Insert(newRow);
            });

            //apply changes to db here
        }

        public bool RemoveVertex(int objectId)
        {
            var row = (VertexDbRows == null) ? null : VertexDbRows.FindByID(objectId);
            if (row != null)
            {
                Delete(row);

                VertexDbRows.Rows.Remove(row);
                //apply changes to db here

                return true;
            }
            return false;
        }

        public SimpleMapDb.VertexesRow GetVertex(int objectId)
        {
            if (VertexDbRows == null) return null;

            var row = VertexDbRows.FindByID(objectId);
            if (row != null)
            {
                var dt = (SimpleMapDb.VertexesDataTable)VertexDbRows.Clone();
                dt.ImportRow(row);
                return (SimpleMapDb.VertexesRow)dt.Rows[0];
            }
            return null;
        }
    }
}
