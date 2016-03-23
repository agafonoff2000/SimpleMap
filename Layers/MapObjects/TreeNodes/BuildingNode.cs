using System;
using System.Data;
using ProgramMain.ExampleDb;
using ProgramMain.Map;
using ProgramMain.Map.Spatial.Types;

namespace ProgramMain.Layers.MapObjects.TreeNodes
{
    public class BuildingNode : ISpatialTreeNode
    {
        private readonly SimpleMapDb.BuildingsRow _row;

        public SpatialTreeNodeTypes NodeType
        {
            get { return SpatialTreeNodeTypes.Rectangle; }
        }

        public Coordinate Coordinate
        {
            get { throw new Exception(); }
        }

        public CoordinateRectangle Rectangle
        {
            get
            {
                return new CoordinateRectangle(
                    _row.Longitude1,
                    _row.Latitude1,
                    _row.Longitude2,
                    _row.Latitude2);
            }
        }

        public CoordinatePoligon Poligon
        {
            get { throw new Exception(); }
        }

        public int RowId
        {
            get { return _row.ID; }
        }

        internal BuildingNode(SimpleMapDb.BuildingsRow row)
        {
            _row = row;    
        }

        public DataRow Row
        {
            get { return _row; }
        }

        public static implicit operator BuildingNode(SimpleMapDb.BuildingsRow row)
        {
            return new BuildingNode(row);
        }
    }
}