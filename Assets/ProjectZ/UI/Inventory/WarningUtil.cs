using System.Collections.Generic;
using UnityEngine;

namespace ProjectZ.UI.Inventory
{
    public static class WarningUtil
    {
        public static void ThrowWarning(Warning warning)
        {
            Debug.LogWarning(_warnings[(int) warning]);
        }

        private static List<string> _warnings = new List<string>
        {
            "This item is too big to put in your inventory.",
            "There are too many items in your inventory."
        };

    }

    public enum Warning
    {
        ItemTooBig,
        TooMuchItem,
    }
}