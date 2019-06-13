using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace ProjectZ.UI.Inventory
{
    public class Dragger : MonoBehaviour, IDragHandler, IBeginDragHandler,IEndDragHandler
    {
        private GameObject m_draggingObject; 
        public void OnBeginDrag(PointerEventData eventData)
        {
            m_draggingObject = eventData.pointerDrag;
        }
    
        public void OnDrag(PointerEventData eventData)
        {
            if (m_draggingObject !=null)    
            {
                m_draggingObject.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        
        }
    }

    public class OnDrop : MonoBehaviour, IDropHandler
    {
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            var objectDroppedOn = eventData.pointerDrag;
            if (objectDroppedOn != null)
            {
                print("objectDroppedOn" +objectDroppedOn);
            }
        }
    }
}