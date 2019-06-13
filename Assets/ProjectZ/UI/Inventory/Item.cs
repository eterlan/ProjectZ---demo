using UnityEngine;

namespace ProjectZ.UI.Inventory
{
    public class Item : MonoBehaviour
    {
        private int m_firstCellIndex;
        private int m_itemIndex;

        public Item() : this(new ImageSize(1, 1))
        {
        } // @TODO: What's this means?-> Call another constructor with it's parameter
    
        public Item(ImageSize imageSize)
        {
            ImageSize = imageSize;
        }

        public ImageSize ImageSize { get; set; }

        public int FirstCellIndex
        {
            get => m_firstCellIndex;
        }

        public int ItemIndex => m_itemIndex;

        public void SetFirstCellIndex(int cellIndex)
        {
            m_firstCellIndex = cellIndex;
        }

        public void SetItemIndex(int itemIndex)
        {
            m_itemIndex = itemIndex;
        }
    }
}