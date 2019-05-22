using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using Object = UnityEngine.Object;

public class InventoryPanelTest
{
    public GameObject Instance;
    public RectTransform PanelBg;
    public RectTransform PanelFg;
    public InventoryPanel InventoryPanel;

    [SetUp]
    public void SetBeforeTest()
    {
        PanelBg = GameObject.Find("UI_InventoryPanel_Bg").GetComponent<RectTransform>();
        PanelFg = GameObject.Find("UI_InventoryPanel_Fg").GetComponent<RectTransform>();
        InventoryPanel = PanelBg.GetComponent<InventoryPanel>();
        
        InventoryPanel.cells = InventoryPanel.GetComponentsInChildren<Cell>();
        InventoryPanel.Initialize();
        Debug.Log(1);
        
        var prefab   = Resources.Load<GameObject>("UI/Item1x1");
        Instance = Object.Instantiate(prefab,PanelFg);
    }
    public class TestCalInstancePositionMethod
    {
        [Test]
        public void _0_Position_Equal_Grid_Position()
        {
            //#Note GetComponentsInChildren会返回包括parent在内的所有GameObject的Component
            var indexOnGrid = new Vector2Int(1, 1);
            var index1d = InventoryPanel.To1DIndex(indexOnGrid);
            var instantiationPosition = InventoryPanel.CalInstancePosition(index1d);
            Assert.AreEqual(
             new Vector2(37.5f,-37.5f), instantiationPosition);
        }
        
        [Test]
        public void _1_Position_Equal_Grid_Position()
        {
            var indexOnGrid           = new Vector2Int(2, 1);
            var index1d = InventoryPanel.To1DIndex(indexOnGrid);
            var instantiationPosition = InventoryPanel.CalInstancePosition(index1d);
            Assert.AreEqual(
                new Vector2(62.5f,-37.5f), instantiationPosition);
        }
    }
    
    public class TryInstantiateNewItem : InventoryPanelTest
         {
             [Test]
             public void _0_Instantiate_Item()
             {
                 var instance = Resources.Load<GameObject>("UI/Item1x1");
                 Assert.IsNotNull(instance);
             }
             
             [Test]
             public void _1_Instantiate_Under_Parent()
             {
                 var parent   = Instance.transform.parent;
                 
                 Assert.AreEqual(PanelFg,parent);
             }
     
             [Test]
             public void _2_Instance_Anchor_on_Grid()
             {
                 var target = PanelBg.GetComponentsInChildren<RectTransform>()[1].anchoredPosition;
                 var instanceAnchorPos = Instance.GetComponent<RectTransform>().anchoredPosition;
                 
                 Assert.LessOrEqual(target.sqrMagnitude-instanceAnchorPos.sqrMagnitude,Mathf.Epsilon);
             }
     
             [Test]
             public void _2_Instance_Anchor_On_Grid()
             {
                 var indexOnUI = new Vector2Int(0,1);
                 var index1d = InventoryPanel.To1DIndex(indexOnUI);
                 var target = PanelBg.GetComponentsInChildren<RectTransform>()[1+InventoryPanel.To1DIndex(indexOnUI)]
                 .anchoredPosition;
                 
                 var instanceRect = Instance.GetComponent<RectTransform>();
                 instanceRect.anchoredPosition = InventoryPanel.CalInstancePosition(index1d);
                 var instanceAnchorPos = instanceRect.anchoredPosition;
                 
                 Assert.LessOrEqual((target-instanceAnchorPos).sqrMagnitude,Mathf.Epsilon);
             }
             
         }

    public class TestInstantiateItemWithCellIndexMethod : InventoryPanelTest
    {
        [Test]
        public void _0_Instantiate_Not_Null()
        {
            var go = InventoryPanel.InstantiateItemWithCellIndex(0);
            Assert.IsNotNull(go);
        }

        [Test]
        public void _1_Instantiate_Position_Equal_Cell_Position()
        {
            var cellIndex = 3;
            var go = InventoryPanel.InstantiateItemWithCellIndex(cellIndex);
            var target = go.GetComponent<RectTransform>().anchoredPosition;
            var cellPosition = InventoryPanel.CalInstancePosition(cellIndex);
            Assert.AreEqual(cellPosition, target);
        }
        
    }
    
    public class TestTo1dIndexMethod
    {
        [Test]
        public void _0_2d_To_1d()
        {
            var index2d = new Vector2Int(0,1);
            var index1d = InventoryPanel.To1DIndex(index2d);
            Assert.AreEqual(10,index1d);
        }
    }

    public class TestTo2dIndexMethod
    {
        [Test]
        public void _0_1d_to_2d()
        {
            var index2d = InventoryPanel.To2dIndex(3);
            Assert.AreEqual(new Vector2Int(3,0),index2d);
        }
    }

    public class TestCanStoreImageMethod : InventoryPanelTest
    {
        [Test]
        public void _0_test_size_4_image_Not_Blocked()
        {
            var cellIndex = 0;
            var size = new ImageSize(2,2);
            
            var storedSuccess = InventoryPanel.CanStoreImage(cellIndex,size);
            Assert.IsTrue(storedSuccess);
        }
//
        [Test]
        public void _1_Test_Size_4_Image_Block_by_0_0()
        {
            var cellIndex = 0;
            var size = new ImageSize(2,2);
            InventoryPanel.cells[cellIndex].SetItemIndex(0);
            
            var storedSuccess = InventoryPanel.CanStoreImage(cellIndex, size);
            Assert.IsFalse(storedSuccess);
        }
        
        // @TODO : debug.
        [Test]
        public void _2_Test_Size_4_Image_Out_Of_Width()
        {
            var cellIndex = 9;
            var size      = new ImageSize(2,2);
            InventoryPanel.cells[cellIndex].SetItemIndex(0);
            
            var storedSuccess = InventoryPanel.CanStoreImage(cellIndex, size);
            Assert.IsFalse(storedSuccess);
        }
        
        [Test]
        public void _2_Test_Size_4_Image_Out_Of_Height()
        {
            var cellIndex = 50;
            var size      = new ImageSize(2,2);
            InventoryPanel.cells[cellIndex].SetItemIndex(0);
            
            var storedSuccess = InventoryPanel.CanStoreImage(cellIndex, size);
            Assert.IsFalse(storedSuccess);
        }
        
        [Test]
        public void _3_Test_Not_Blocked()
        {
            var size = new ImageSize(2,2);
            var pass = InventoryPanel.IsOutOfRange(0,size);
            Assert.IsFalse(pass);
        }
    }

    public class TestFindStorableIndexMethod : InventoryPanelTest
    {
        [Test]
        public void _0_Test_Not_Blocked()
        {
            var size = new ImageSize(2,2);
            InventoryPanel.FindStorableIndex(size,out int index);
            Assert.AreEqual(0,index);
        }
        
        [Test]
        public void _1_Test_Size_2_2_Not_Blocked()
        {
            var size = new ImageSize(2,2);
            InventoryPanel.cells[1].SetItemIndex(0);
            InventoryPanel.FindStorableIndex(size,out int index);
            Assert.AreEqual(2, index);
        }
    }
    
//    public class TestChangeItemPositionMethod : InventoryPanelTest
//    {
//        
//    }
}