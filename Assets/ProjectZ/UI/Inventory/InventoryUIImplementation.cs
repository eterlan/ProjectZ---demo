using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectZ.UI.Inventory
{
    public class InventoryUIImplementation : MonoBehaviour
    {
        private Item           m_item;
        private InventoryPanel m_inventoryPanel;
        private bool           m_unsettled;
        private RectTransform  m_canvasRect;
        private Camera         m_camera;
        private RectTransform  m_itemFollowMouseRect;

        private void Start()
        {
            m_inventoryPanel = GetComponent<InventoryPanel>();
            m_canvasRect     = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            m_camera         = Camera.main;
        }

        private void Update()
        {
            if (!m_unsettled) return;
            // change item position
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle
                (m_canvasRect,Input.mousePosition,m_camera,out var mouseWorldPosition))
            {
                m_itemFollowMouseRect.position = mouseWorldPosition;
            }
        }

        private GameObject ObjectOnClick()
        {
            var results = new List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current) 
                {position = Input.mousePosition};
            EventSystem.current.RaycastAll(eventData,results );
            if (results.Count > 0)
                return results[0].gameObject;
            return null;
        }

        private void OnMouseDown()
        {
            var go = ObjectOnClick();
            if (!go) return;
        
            if (m_item)
            {
                OnSecondClick(m_item,go);
            }
            else if(go.CompareTag("Item"))
            {
                OnFirstClick(go.GetComponent<Item>());
            }
        }

        private void OnFirstClick(Item item)
        {
            m_item = item;
            // Show item info.
        }

        private void OnSecondClick(Item item, GameObject go)
        {
            int        cellIndex    = -1;
            List<Item> overLapItems = new List<Item>();

            if (go.CompareTag("Item"))
                cellIndex = go.GetComponent<Item>().FirstCellIndex;
            else if(go.CompareTag("Cell")) 
                cellIndex = go.GetComponent<Cell>().CellIndex;
            // Is there ary item block this item to move? 
            if (cellIndex != -1)
                overLapItems = m_inventoryPanel.OverLapDetect(cellIndex, item.ImageSize);
            // null & >1 item blocked-> do nothing.
            if (overLapItems == null) 
                return;
            // 0 -> Simply move item
            if (overLapItems.Count == 0)
            {
                m_inventoryPanel.TryMoveItemInGrid(item,cellIndex);
            }
            // 1 -> store new item and move new item with mouse.
            else if (overLapItems.Count == 1)
            {
                m_item                = overLapItems[0];
                m_unsettled           = true;
                m_itemFollowMouseRect = m_item.GetComponent<RectTransform>();
            }
        }
    }
}
    
