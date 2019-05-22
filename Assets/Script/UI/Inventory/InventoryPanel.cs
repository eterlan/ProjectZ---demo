using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    public GameObject item1x1;
    public GameObject item2x2;
    
    [HideInInspector]
    public Cell[] cells;
    [HideInInspector]
    public List<Item> items;
    
    public const int GridWidth = 10;
    public const int GridHeight = 6;
    public const int GridVolume = 60;
    public static float cellSize = 25;

    private void Awake()
    {
        item1x1 = Resources.Load<GameObject>("UI/item1x1");
        cells = GetComponentsInChildren<Cell>();
    }

    public void Initialize()
    {
        foreach (var cell in cells)
        {
            cell.Initialize();
            items.Clear();
        }
    }

    public static Vector2 CalInstancePosition(int cellIndex)
    {
        var instantiationPosition = new Vector2();
        var pivotPoint            = new Vector2(0.5f,0.5f);

        var indexOnGrid = To2dIndex(cellIndex);
        instantiationPosition.x = cellSize * (indexOnGrid.x + pivotPoint.x); // get from pivot point?
        instantiationPosition.y = -(cellSize * (indexOnGrid.y + pivotPoint.y));
        return instantiationPosition;
    }

    public static int To1DIndex(Vector2Int indexOnGrid)
    {
        return indexOnGrid.x  + indexOnGrid.y * GridWidth;
    }

    public static Vector2Int To2dIndex(int indexOnGrid)
    {
        var index2d = new Vector2Int();
        index2d.x= indexOnGrid % GridWidth;
        index2d.y = indexOnGrid / GridWidth;
        return index2d;
    }

    public GameObject InstantiateItemWithCellIndex(int cellIndex)
    {
        var prefab = Resources.Load<GameObject>("UI/Item1x1");
        var parent = GetComponent<RectTransform>();
        var instance = Instantiate(prefab,parent);
        
        var instantiatePosition = CalInstancePosition(cellIndex); 
        instance.GetComponent<RectTransform>().anchoredPosition = instantiatePosition;
        return instance;
    }

    public bool CanStoreImage(int cellIndex, ImageSize imageSize)
    {
        for (int i = 0; i < imageSize.x; i++)
        {
            for (int j = 0; j < imageSize.y; j++)
            {
                // Not empty or outOfRange -> can't store.
                if (!cells[i +GridWidth *j +cellIndex].IsEmpty || IsOutOfRange(cellIndex,imageSize))
                {
                    print($"i{i},j{j}");
                    return false;
                }
            }
        }
        return true;
    }
    
    public bool IsOutOfRange(int index,ImageSize imageSize)
    {
        if (To2dIndex(index).x + imageSize.x > GridWidth || To2dIndex(index).y + imageSize.y > GridHeight)
        {
            throw new IndexOutOfRangeException();
            return true;
        }
        return false;
    }
    
    public bool ChangeItemPosition(int cellIndex, Item item)
    {
        var imageSize = item.ImageSize;
        var itemIndex = item.Index;
        
        item.SetFirstCellIndex(cellIndex);
        for (int i = 0; i < imageSize.x; i++)
        {
            for (int j = 0; j < imageSize.y; j++)
            {
                var insertIndex = i + GridWidth * j + cellIndex;
                if (!cells[insertIndex].IsEmpty)
                    return false;
                cells[insertIndex].SetItemIndex(itemIndex);
            }
        }
        return true;
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

    /// <summary>
    /// Automatically find a place and add a new item
    /// </summary>
    /// <param name="item">new item to be add</param>
    /// <returns></returns>
    public bool AddNewItem(Item item)
    {
        if (!FindStorableIndex(item.ImageSize, out int index)) 
            return false;
        items.Add(item);
        return InstantiateItemWithCellIndex(index) != null;
    }
}