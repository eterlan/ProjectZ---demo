using System.Collections.Generic;
using UnityEngine;

namespace ProjectZ.UI
{
    public class CustomCursor : MonoBehaviour
    {
        private Dictionary<int, int> idsad;
        public  Texture2D            cursorTexture;
        public  CursorMode           cursorMode = CursorMode.Auto;
        public  Vector2              hotSpot    = Vector2.zero;

        void OnMouseEnter()
        {
            print("1");
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        }

        void OnMouseExit()
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }
}
