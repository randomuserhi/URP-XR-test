using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.Menus
{
    public static partial class MTK
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            int layer = LayerMask.NameToLayer("MTK");
            if (layer < 0) Debug.LogError("MTK layer does not exist, please create it in project settings and disable all collisions with it.");
        }
    }
}
