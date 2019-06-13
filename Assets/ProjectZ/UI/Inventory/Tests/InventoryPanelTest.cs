using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectZ.UI.Inventory;
using UnityEngine;
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
            var index1d = ImageUtil.To1DIndex(indexOnGrid);
            var instantiationPosition = ImageUtil.CalInstancePosition(index1d);
            Assert.AreEqual(
             new Vector2(37.5f,-37.5f), instantiationPosition);
        }
        
        [Test]
        public void _1_Position_Equal_Grid_Position()
        {
            var indexOnGrid           = new Vector2Int(2, 1);
            var index1d = ImageUtil.To1DIndex(indexOnGrid);
            var instantiationPosition = ImageUtil.CalInstancePosition(index1d);
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
                 var index1d = ImageUtil.To1DIndex(indexOnUI);
                 var target = PanelBg.GetComponentsInChildren<RectTransform>()[1+ImageUtil.To1DIndex(indexOnUI)]
                 .anchoredPosition;
                 
                 var instanceRect = Instance.GetComponent<RectTransform>();
                 instanceRect.anchoredPosition = ImageUtil.CalInstancePosition(index1d);
                 var instanceAnchorPos = instanceRect.anchoredPosition;
                 
                 Assert.LessOrEqual((target-instanceAnchorPos).sqrMagnitude,Mathf.Epsilon);
             }
             
         }

    public class TestInstantiateItemWithCellIndexMethod : InventoryPanelTest
    {
        [Test]
        public void _0_Instantiate_Not_Null()
        {
            var go = InventoryPanel.InstantiateItem();
            Assert.IsNotNull(go);
        }

        [Test]
        public void _1_Instantiate_Position_Equal_Cell_Position()
        {
            var cellIndex = 3;
            var go = InventoryPanel.InstantiateItem();
            var target = go.GetComponent<RectTransform>().anchoredPosition;
            var cellPosition = ImageUtil.CalInstancePosition(cellIndex);
            Assert.AreEqual(cellPosition, target);
        }
        
    }
    
    public class TestTo1dIndexMethod
    {
        [Test]
        public void _0_2d_To_1d()
        {
            var index2d = new Vector2Int(0,1);
            var index1d = ImageUtil.To1DIndex(index2d);
            Assert.AreEqual(10,index1d);
        }
    }

    public class TestTo2dIndexMethod
    {
        [Test]
        public void _0_1d_to_2d()
        {
            var index2d = ImageUtil.To2dIndex(3);
            Assert.AreEqual(new Vector2Int(3,0),index2d);
        }
    }

    // TODO : How to test capsuled method? use protected modifier instead of private.
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
            InventoryPanel.cells[cellIndex].AddItem(0);
            
            var storedSuccess = InventoryPanel.CanStoreImage(cellIndex, size);
            Assert.IsFalse(storedSuccess);
        }
        
        [Test]
        public void _2_Test_Size_4_Image_Out_Of_Width()
        {
            var cellIndex = 9;
            var size      = new ImageSize(2,2);
            
            var storedSuccess = InventoryPanel.CanStoreImage(cellIndex, size);
            Assert.IsFalse(storedSuccess);
        }
        
        [Test]
        public void _2_Test_Size_4_Image_Out_Of_Height()
        {
            var cellIndex = 50;
            var size      = new ImageSize(2,2);
            InventoryPanel.cells[cellIndex].AddItem(0);
            
            var storedSuccess = InventoryPanel.CanStoreImage(cellIndex, size);
            Assert.IsFalse(storedSuccess);
        }
        
        [Test]
        public void _3_Test_Not_Blocked()
        {
            var size = new ImageSize(2,2);
            var pass = ImageUtil.IsOutOfRange(0,size);
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
            InventoryPanel.cells[1].AddItem(0);
            InventoryPanel.FindStorableIndex(size,out int index);
            Assert.AreEqual(2, index);
        }
    }

    public class TestAddNewItemMethod : InventoryPanelTest
    {
        private Item target;
        Item         item1;
        Item         item2;

        [SetUp]
        public void Prepare()
        {
            target = new GameObject().AddComponent<Item>();
            item1  = new GameObject().AddComponent<Item>();
            item2  = new GameObject().AddComponent<Item>();
        }

        [Test]
        public void _0_Method_Implemented()
        {
            Assert.DoesNotThrow(() => InventoryPanel.AddNewItem(target));
        }

        [Test]
        public void _1_New_Item_Size_Equal_1_1()
        {
            var target = this.target.ImageSize;
            Assert.AreEqual(target.x, 1);
        }

        [Test]
        public void _2_New_Item_Position_On_Grid()
        {
            var target = this.target.FirstCellIndex;
            Assert.AreEqual(0, target);
        }

        [Test]
        public void _2_Item_Position_On_index_1()
        {
            InventoryPanel.cells[0].AddItem(0);
            InventoryPanel.AddNewItem(this.target);
            Assert.AreEqual(1, target.FirstCellIndex);
        }

        [Test]
        public void _2_Item_Block_By_2_Item_Position_On_index_2()
        {
            InventoryPanel.cells[0].AddItem(0);
            InventoryPanel.cells[1].AddItem(1);
            InventoryPanel.AddNewItem(this.target);
            Assert.AreEqual(2, target.FirstCellIndex);
        }

        [Test]
        public void _3_Item_Block_By_2_Item_Position_On_index_2()
        {
            InventoryPanel.cells[0].AddItem(0);
            InventoryPanel.cells[1].AddItem(1);
            InventoryPanel.AddNewItem(this.target);
            Assert.AreEqual(2, target.FirstCellIndex);
        }

        [Test]
        public void _4_Combind_Test()
        {
            item1.ImageSize = new ImageSize(2, 2);

            InventoryPanel.AddNewItem(item1);
            InventoryPanel.AddNewItem(item2);
            InventoryPanel.AddNewItem(target);

            Assert.AreEqual(3, target.FirstCellIndex);
        }

        [Test]
        public void _4_Combind_Test_2_Mixed_Image_Size()
        {
            item1.ImageSize  = new ImageSize(6, 5);
            item2.ImageSize  = new ImageSize(3, 1);
            target.ImageSize = new ImageSize(2, 2);

            InventoryPanel.AddNewItem(item1);
            InventoryPanel.AddNewItem(item2);
            InventoryPanel.AddNewItem(target);

            Assert.AreEqual(ImageUtil.To1DIndex(new Vector2Int(6, 1)), target.FirstCellIndex);
        }

        [Test]
        public void _4_Combind_Test_3_Out_Of_Range()
        {
            item1.ImageSize  = new ImageSize(6, 3);
            item2.ImageSize  = new ImageSize(3, 3);
            target.ImageSize = new ImageSize(2, 2);

            InventoryPanel.AddNewItem(item1);
            InventoryPanel.AddNewItem(item2);
            InventoryPanel.AddNewItem(target);

            Assert.AreEqual(ImageUtil.To1DIndex(new Vector2Int(0, 3)), target.FirstCellIndex);
        }


        [Test]
        public void _5_To_Much_Item_Throw_OverFlow_exception()
        {
            item1.ImageSize  = new ImageSize(5, 5);
            item2.ImageSize  = new ImageSize(5, 5);
            target.ImageSize = new ImageSize(2, 2);

            InventoryPanel.AddNewItem(item1);
            InventoryPanel.AddNewItem(item2);

            Assert.IsFalse(InventoryPanel.AddNewItem(target));
        }
    }

    public class TestChangeItemPositionMethod : InventoryPanelTest
    {
        
    }
}