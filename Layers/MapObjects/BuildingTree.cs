using System.Data;
using System.Threading.Tasks;
using ProgramMain.ExampleDb;
using ProgramMain.Layers.MapObjects.TreeNodes;
using ProgramMain.Map.Spatial;
using ProgramMain.Map.Spatial.Types;

namespace ProgramMain.Layers.MapObjects
{
    public class BuildingTree : SpatialTree<BuildingNode>
    {
        public SimpleMapDb.BuildingsDataTable BuildingDbRows { get; private set; }

        public BuildingTree()
            : base(SpatialSheetPowerTypes.Ultra, SpatialSheetPowerTypes.Extra, SpatialSheetPowerTypes.Medium, SpatialSheetPowerTypes.Low)
        {
            BuildingDbRows = new SimpleMapDb.BuildingsDataTable();
        }

        public void Insert(SimpleMapDb.BuildingsRow row)
        {
            Insert(new BuildingNode(row));
        }

        public void Delete(SimpleMapDb.BuildingsRow row)
        {
            Delete(new BuildingNode(row));
        }
        
        public void LoadData()
        {
            Clear();
            
            //read data from db here

            Parallel.ForEach(BuildingDbRows.Cast<SimpleMapDb.BuildingsRow>(), Insert);
        }

        public void MergeData(SimpleMapDb.BuildingsDataTable buildings)
        {
            if (BuildingDbRows == null) return;

            BuildingDbRows.Merge(buildings, false, MissingSchemaAction.Error);

            Parallel.ForEach(buildings.Cast<SimpleMapDb.BuildingsRow>(), delegate(SimpleMapDb.BuildingsRow row)
            {
                var newRow = BuildingDbRows.FindByID(row.ID);
                Insert(newRow);
            });
            //apply changes to db here
        }

        public bool RemoveBuilding(int objectId)
        {
            var row = (BuildingDbRows == null) ? null : BuildingDbRows.FindByID(objectId);
            if (row != null)
            {
                Delete(row);

                BuildingDbRows.Rows.Remove(row);
                //apply changes to db here

                return true;
            }
            return false;
        }

        public SimpleMapDb.BuildingsRow GetBuilding(int objectId)
        {
            if (BuildingDbRows == null) return null;

            var row = BuildingDbRows.FindByID(objectId);
            if (row != null)
            {
                var dt = (SimpleMapDb.BuildingsDataTable)BuildingDbRows.Clone();
                dt.ImportRow(row);
                return (SimpleMapDb.BuildingsRow)dt.Rows[0];
            }
            return null;
        }
    }
}
