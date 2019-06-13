using System;
using UnityEngine;

namespace ProjectZ.UI.Inventory
{
    public struct ImageSize
    {
        public int x;
        public int y;
    
        // @bug 之所以不能用自己作为Max，是因为max也要new，在这个过程中就会判断是否小于0
        private static Vector2Int Max = new Vector2Int(10,6);

        public ImageSize(int x, int y)
        {
            if (x > Max.x || y > Max.y) throw new ArgumentOutOfRangeException(nameof(ImageSize));
            this.x = x;
            this.y = y;
        }
    }
}