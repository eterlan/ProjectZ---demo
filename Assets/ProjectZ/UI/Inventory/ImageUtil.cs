using UnityEngine;

namespace ProjectZ.UI.Inventory
{
    public static class ImageUtil
    {
        public static Vector2 CalInstancePosition(int cellIndex)
        {
            var instantiationPosition = new Vector2();
            var pivotPoint            = new Vector2(0.5f,0.5f);

            var indexOnGrid = To2dIndex(cellIndex);
            instantiationPosition.x = InventoryPanel.cellSize * (indexOnGrid.x + pivotPoint.x); // get from pivot point?
            instantiationPosition.y = -(InventoryPanel.cellSize * (indexOnGrid.y + pivotPoint.y));
            return instantiationPosition;
        }

        public static int To1DIndex(Vector2Int indexOnGrid)
        {
            return indexOnGrid.x + indexOnGrid.y * InventoryPanel.GridWidth;
        }

        public static Vector2Int To2dIndex(int indexOnGrid)
        {
            var index2d = new Vector2Int();
            index2d.x = indexOnGrid % InventoryPanel.GridWidth;
            index2d.y = indexOnGrid / InventoryPanel.GridWidth;
            return index2d;
        }

        public static bool IsOutOfRange(int cellIndex, ImageSize imageSize)
        {
            if (To2dIndex(cellIndex).x + imageSize.x > InventoryPanel.GridWidth || ImageUtil.To2dIndex(cellIndex).y + imageSize.y > InventoryPanel.GridHeight)
            {
                WarningUtil.ThrowWarning(Warning.TooMuchItem);
                return true;
            }
            return false;
        }
    }
}