using System.Data;

namespace ProgramMain.Map.Spatial.Types
{
    public interface ISpatialTreeNode
    {
        SpatialTreeNodeTypes NodeType { get; }
        Coordinate Coordinate { get; }
        CoordinateRectangle Rectangle { get; }
        CoordinatePoligon Poligon { get; }
        int RowId { get; }
        DataRow Row { get; } //!!!absolute
    }
}
