using System.Collections.Generic;
using ProgramMain.Map.Google;
using ProgramMain.Map.Spatial.Types;
using ProgramMain.Map.Types;

namespace ProgramMain.Map.Spatial
{
    internal class SpatialSheet<TNode> : SpatialSheetBase<TNode> where TNode : ISpatialTreeNode
    {
        public CoordinateRectangle Rectangle { get; private set; }

        public SpatialSheet(SpatialTree<TNode> tree, int level, int googleLevel, CoordinateRectangle rectangle)
            : base(tree, level, googleLevel)
        {
            Rectangle = rectangle;
        }

        public enum SheetActionType { Insert, Delete };

        internal void SheetAction(TNode node, SheetActionType actionType)
        {
            //Insert node to spatial index or remove node from spatial index
            if (!IsBottomSheet)
            {
                switch (node.NodeType)
                {
                    case SpatialTreeNodeTypes.Point:
                        PointSheetAction(node, actionType);
                        break;
                    case SpatialTreeNodeTypes.Line:
                        LineSheetAction(node, actionType);
                        break;
                    case SpatialTreeNodeTypes.Rectangle:
                        RectangleSheetAction(node, actionType);
                        break;
                    case SpatialTreeNodeTypes.Poligon:
                        PoligonSheetAction(node, actionType);
                        break;
                }
            }
            else
            {
                //Just do it on bottom level
                lock (this)
                {
                    switch (actionType)
                    {
                        case SheetActionType.Insert:
                            Content.Add(node);
                            break;
                        case SheetActionType.Delete:
                            Content.Remove(node);
                            break;
                    }
                }
            }
        }

        private void PointSheetAction(TNode node, SheetActionType actionType)
        {
            var block = node.Coordinate.GetGoogleBlock(NextGoogleLevel);
            lock (this)
            {
                //point search in daughter sheet 
                var sheet = Sheets[block];
                
                sheet.SheetAction(node, actionType);

                PostSheetAction(block, sheet, actionType);
            }
        }

        private void LineSheetAction(TNode node, SheetActionType actionType)
        {
            var blockViewLevel = NextGoogleLevel;
            
            var line = node.Rectangle;
            var rect = new GoogleRectangle(line, blockViewLevel).BlockView;

            var deltaX = (rect.Left <= rect.Right) ? 1 : -1;
            var deltaY = (rect.Top <= rect.Bottom) ? 1 : -1;

            //line search in daughter sheet 
            for (var x = rect.Left; (deltaX == 1 && x <= rect.Right) || (deltaX == -1 && x >= rect.Right); x += deltaX)
            {
                for (var y = rect.Top; (deltaY == 1 && y <= rect.Bottom) || (deltaY == -1 && y >= rect.Bottom); y += deltaY)
                {
                    var block = new GoogleBlock(x, y, blockViewLevel);
                    var googleRect = (GoogleRectangle)block;
                    
                    if (googleRect.LineContains(line) != InterseptResult.None)
                    {
                        lock (this)
                        {
                            var sheet = Sheets[block];

                            sheet.SheetAction(node, actionType);

                            PostSheetAction(block, sheet, actionType);
                        }
                    }
                }
            }
        }

        private void RectangleSheetAction(TNode node, SheetActionType actionType)
        {
            var blockViewLevel = NextGoogleLevel;

            var rect = new GoogleRectangle(node.Rectangle, blockViewLevel).BlockView;

            var deltaX = (rect.Left <= rect.Right) ? 1 : -1;
            var deltaY = (rect.Top <= rect.Bottom) ? 1 : -1;

            //rectangle search in daughter sheet 
            for (var x = rect.Left; (deltaX == 1 && x <= rect.Right) || (deltaX == -1 && x >= rect.Right); x += deltaX)
            {
                for (var y = rect.Top; (deltaY == 1 && y <= rect.Bottom) || (deltaY == -1 && y >= rect.Bottom); y += deltaY)
                {
                    var block = new GoogleBlock(x, y, blockViewLevel);
                    lock (this)
                    {
                        var sheet = Sheets[block];

                        sheet.SheetAction(node, actionType);

                        PostSheetAction(block, sheet, actionType);
                    }
                }
            }
        }

        private void PoligonSheetAction(TNode node, SheetActionType actionType)
        {
            var blockViewLevel = NextGoogleLevel;

            var poligon = node.Poligon;
            var rect = new GoogleRectangle(poligon, blockViewLevel).BlockView;

            var deltaX = (rect.Left <= rect.Right) ? 1 : -1;
            var deltaY = (rect.Top <= rect.Bottom) ? 1 : -1;

            //poligon search in daughter sheet 
            for (var x = rect.Left; (deltaX == 1 && x <= rect.Right) || (deltaX == -1 && x >= rect.Right); x += deltaX)
            {
                for (var y = rect.Top; (deltaY == 1 && y <= rect.Bottom) || (deltaY == -1 && y >= rect.Bottom); y += deltaY)
                {
                    var block = new GoogleBlock(x, y, blockViewLevel);
                    var googleRect = (GoogleRectangle)block;

                    if (googleRect.PoligonContains(poligon) != InterseptResult.None)
                    {
                        lock (this)
                        {
                            var sheet = Sheets[block];

                            sheet.SheetAction(node, actionType);

                            PostSheetAction(block, sheet, actionType);
                        }
                    }
                }
            }
        }

        private void PostSheetAction(GoogleBlock block, SpatialSheet<TNode> sheet, SheetActionType actionType)
        {
            switch (actionType)
            {
                case SheetActionType.Delete:
                    {
                        //delete sheet from index without elements
                        if (sheet.IsEmpty)
                            Sheets.Remove(block);
                    } break;
            }
        }

        public void Query(HashSet<ISpatialTreeNode> hashSet, CoordinateRectangle rectangle, InterseptResult parentResult, SpatialQueryIterator i)
        {
            //Query elements on the map by coordinate ractengle(indexed search)
            lock (this)
            {
                if (!IsBottomSheet)
                {
                    if (Sheets.HasChilds)
                    {
                        foreach (var sheet in Sheets.Values)
                        {
                            var res = parentResult == InterseptResult.Supersets ? InterseptResult.Supersets : InterseptResult.None;
                            
                            if (res != InterseptResult.Supersets)
                            {
                                i.Next();

                                res = sheet.Rectangle.RectangleContains(rectangle);
                            }
                            if (res != InterseptResult.None)
                            {
                                sheet.Query(hashSet, rectangle, res, i);
                            }
                            if (res == InterseptResult.Contains) break;
                        }
                    }
                }
                else if (Content.HasChilds)
                {
                    foreach (var node in Content.Values)
                    {
                        var res = parentResult == InterseptResult.Supersets ? InterseptResult.Supersets : InterseptResult.None;
                        if (res != InterseptResult.Supersets)
                        {
                            i.Next();

                            switch (node.NodeType)
                            {
                                case SpatialTreeNodeTypes.Point:
                                    res = rectangle.PointContains(node.Coordinate);
                                    break;
                                case SpatialTreeNodeTypes.Line:
                                    res = rectangle.LineContains(node.Rectangle);
                                    break;
                                case SpatialTreeNodeTypes.Rectangle:
                                    res = rectangle.RectangleContains(node.Rectangle);
                                    break;
                                case SpatialTreeNodeTypes.Poligon:
                                    res = rectangle.PoligonContains(node.Poligon);
                                    break;
                            }
                        }
                        if (res != InterseptResult.None)
                        {
                            hashSet.Add(node);
                        }
                    }
                }
            }
        }

        public void Distance(HashSet<ISpatialTreeNode> hashSet, Coordinate coordinate, double variance, SpatialQueryIterator i)
        {
            //Query elements on the map close to coordinate (indexed search)
            lock (this)
            {
                if (!IsBottomSheet)
                {
                    if (Sheets.HasChilds)
                    {
                        foreach (var sheet in Sheets.Values)
                        {
                            i.Next();

                            if (sheet.Rectangle.RectangeDistance(coordinate) <= variance)
                            {
                                sheet.Distance(hashSet, coordinate, variance, i);
                            }
                        }
                    }
                }
                else if (Content.HasChilds)
                {
                    foreach (var node in Content.Values)
                    {
                        i.Next();

                        double distance = -1;
                        switch (node.NodeType)
                        {
                            case SpatialTreeNodeTypes.Point:
                                distance = node.Coordinate.Distance(coordinate);
                                break;
                            case SpatialTreeNodeTypes.Line:
                                distance = node.Rectangle.LineDistance(coordinate);
                                break;
                            case SpatialTreeNodeTypes.Rectangle:
                                distance = node.Rectangle.RectangeDistance(coordinate);
                                break;
                            case SpatialTreeNodeTypes.Poligon:
                                distance = node.Poligon.PoligonDistance(coordinate);
                                break;
                        }

                        if (distance >= 0 && distance <= variance)
                        {
                            hashSet.Add(node);
                        }
                    }
                }
            }
        }
    }
}