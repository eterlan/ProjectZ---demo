using System.Collections.Generic;
using UnityEngine;

namespace ProjectZ.UI.Inventory
{
    public class InventoryPanel : MonoBehaviour
    {
        public GameObject     item1X1;
        public GameObject     item2X2;
        public InventoryPanel inventoryPanel;
    
        [HideInInspector]
        public Cell[] cells;
        [HideInInspector]
        public List<Item> items;
    
        public const  int   GridWidth  = 10;
        public const  int   GridHeight = 6;
        public const  int   GridVolume = 60;
        public static float cellSize   = 25;

        private void Awake()
        {
//        item1X1 = Resources.Load<GameObject>("UI/item1x1");
            cells = GetComponentsInChildren<Cell>();
            Initialize();
        }
        // Is switch scece would run Awake() every time? 
        public void Initialize()
        {
            items.Clear();

            for (var index = 0; index < cells.Length; index++)
            {
                var cell = cells[index];
                cell.Initialize(index);
            }
        }

        //@Todo : change to private after test.

        /// <summary>
        /// Automatically find a place and add a new item
        /// </summary>
        /// <param name="item">new item to be add</param>
        /// <returns></returns>
        public bool AddNewItem(Item item)
        {
            if (!FindStorableIndex(item.ImageSize, out int cellIndex)) 
                return false;
            item.SetItemIndex(items.Count);
            items.Add(item);
            return TryMoveItemInGrid(item,cellIndex);
        }
    
        public bool TryMoveItemInGrid(Item item, int cellIndex)
        {
            ClearOldCell(item);
            if (!CanStoreImage(cellIndex, item.ImageSize))
            {
                RestoreOldCellIndex(item);
                return false;
            }
            ConnectItemWithGrid(item,cellIndex);
            InstantiateItem();
            UpdatePosition(item,cellIndex);
            return true;
        }
    
        public void ConnectItemWithGrid(Item item, int cellIndex) // 这个connection他是怎么做的？
        {
            item.SetFirstCellIndex(cellIndex);
        
            var imageSize = item.ImageSize;
            for (int i = 0; i < imageSize.x; i++)
            {
                for (int j = 0; j < imageSize.y; j++)
                {
                    var insertIndex = i + GridWidth * j + cellIndex;

                    cells[insertIndex].AddItem(item.ItemIndex);
                }
            }
        }

        private void ClearOldCell(Item item)
        {
            var oldFirstIndex = item.FirstCellIndex;
        
            var imageSize = item.ImageSize;
            for (int i = 0; i < imageSize.x; i++)
            {
                for (int j = 0; j < imageSize.y; j++)
                {
                    var oldIndex = i + GridWidth * j + oldFirstIndex;
                    cells[oldIndex].Clear();
                }
            }
        }
    
        private void RestoreOldCellIndex(Item item)
        {
            var oldFirstIndex = item.FirstCellIndex;
        
            var imageSize = item.ImageSize;
            for (int i = 0; i < imageSize.x; i++)
            {
                for (int j = 0; j < imageSize.y; j++)
                {
                    var oldIndex = i + GridWidth * j + oldFirstIndex;
                    cells[oldIndex].Clear();
                }
            }
        }

        public bool FindStorableIndex(ImageSize imageSize, out int index)
        {
            for (int i = 0; i < GridVolume; i++)
            {
                if (CanStoreImage(i, imageSize))
                {
                    index = i;
                    return true;
                };
            }
            index = -1;
            return false;
        }

        public bool CanStoreImage(int cellIndex, ImageSize imageSize)
        {
            if (ImageUtil.IsOutOfRange(cellIndex, imageSize))
                return false;
            for (int i = 0; i < imageSize.x; i++)
            {
                for (int j = 0; j < imageSize.y; j++)
                {
                    if (!cells[i +GridWidth *j +cellIndex].IsEmpty)
                        return false;
                }
            }
            return true;
        }
    
        public List<Item> OverLapDetect(int cellIndex, ImageSize imageSize)
        {
            // 如果出界，Warning并返回空值
            if (ImageUtil.IsOutOfRange(cellIndex, imageSize))
                return null;
            var overLapItems  = new List<Item>();
            var lastItemIndex = -1;
        
            for (var i = 0; i < imageSize.x; i++)
            {
                for (var j = 0; j < imageSize.y; j++)
                {
                    var insertIndex = i +GridWidth *j +cellIndex;
                    // 如果空的，检测下一格
                    if (cells[insertIndex].IsEmpty) 
                        continue;
                    // 如果重复，检测下一格
                    var itemIndex = cells[insertIndex].ItemIndex;
                    if (itemIndex == lastItemIndex || itemIndex == -1) 
                        continue;
                    overLapItems.Add(items[itemIndex]);
                    lastItemIndex = itemIndex;
                }
            }
            return overLapItems;
        }

        public GameObject InstantiateItem()
        {
            var prefab   = Resources.Load<GameObject>("UI/Item1x1");
            var parent   = GetComponent<RectTransform>();
            var instance = Instantiate(prefab,parent);
            return instance;
        }

        private void UpdatePosition(Item item, int cellIndex)
        {
            var instantiatePosition = ImageUtil.CalInstancePosition(cellIndex); 
            item.GetComponent<RectTransform>().anchoredPosition = instantiatePosition;
        }

    }
}