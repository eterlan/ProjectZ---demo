using System;
using UnityEngine;

namespace ProjectZ.UI.Inventory
{
    public class Cell : MonoBehaviour
    {
        #region Field
        private bool m_isEmpty   = true;
        private int  m_itemIndex = -1;
        private int  m_cellIndex;

        #endregion
    
        #region Property

        public int CellIndex => m_cellIndex;

        public int ItemIndex => m_itemIndex;

        public bool IsEmpty => m_isEmpty;

        #endregion

        #region Public Method
        /// <summary>
        /// Default : Cell->Empty, itemIndex = -1, CellIndex = parameter
        /// </summary>
        /// <param name="cellIndex"></param>
        public void Initialize(int cellIndex)
        {
            Clear();
            m_cellIndex = cellIndex;
        }

        public void Clear()
        {
            m_isEmpty   = true;
            m_itemIndex = -1;
        }

        public void AddItem(int itemIndex)
        {
            if (itemIndex > InventoryPanel.GridVolume) 
                throw new ArgumentOutOfRangeException(nameof(itemIndex));
            m_isEmpty   = false;
            m_itemIndex = itemIndex;
        }

        public void ClearItemIndex()
        {
            m_isEmpty   = true;
            m_itemIndex = -1;
        }

        #endregion
    }
}