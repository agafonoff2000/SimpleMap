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
            //вставка элемента в индекс, или его удаление из индекса
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
                //если дошли до нижнего уровня, то добавить/удалить элемент
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
                //поиск для точки дочернего элемента в индексе
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

            //поиск для линии дочерних элементов в индексе(линия может входить в несколько элементов индекса)
            for (var x = rect.Left; (deltaX == 1 && x <= rect.Right) || (deltaX == -1 && x >= rect.Right); x += deltaX)
            {
                for (var y = rect.Top; (deltaY == 1 && y <= rect.Bottom) || (deltaY == -1 && y >= rect.Bottom); y += deltaY)
                {
                    var block = new GoogleBlock(x, y, blockViewLevel);
                    var googleRect = (GoogleRectangle)block;
                    
                    //проверка вхождения линии в каждый из потенциальных дочерних элементов индекса
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

            //поиск для прямоугольника дочерних элементов в индексе(прямоугольник может входить в несколько элементов индекса)
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

            //поиск для линии дочерних элементов в индексе(линия может входить в несколько элементов индекса)
            for (var x = rect.Left; (deltaX == 1 && x <= rect.Right) || (deltaX == -1 && x >= rect.Right); x += deltaX)
            {
                for (var y = rect.Top; (deltaY == 1 && y <= rect.Bottom) || (deltaY == -1 && y >= rect.Bottom); y += deltaY)
                {
                    var block = new GoogleBlock(x, y, blockViewLevel);
                    var googleRect = (GoogleRectangle)block;

                    //проверка вхождения линии в каждый из потенциальных дочерних элементов индекса
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
                        //при удалении объектов удалить текущюю ветку идекса, если она пустая
                        if (sheet.IsEmpty)
                            Sheets.Remove(block);
                    } break;
            }
        }

        public void Query(HashSet<ISpatialTreeNode> hashSet, CoordinateRectangle rectangle, InterseptResult parentResult, SpatialQueryIterator i)
        {
            //запрос на поиск элементов в индексе в заданном квадрате координат
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

                            //проверка на вхождение текущего элемента в индексе в заданном квадрате координат
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
            //запрос на поиск элементов в индексе удаленных на определенное растояние (в метрах) от заданной точки
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

                        //расчет растояния для текущего элемента (в метрах) от заданной точки
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