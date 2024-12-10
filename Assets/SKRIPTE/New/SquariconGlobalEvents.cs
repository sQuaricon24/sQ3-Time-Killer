using System;
using UnityEngine;

namespace Squaricon.SQ3
{
    public static class SquariconGlobalEvents
    {
        //Gameplay events
        public static Action<TokenController> OnDragEnded = null;
        public static Action<TokenController, DragAxis> OnTokenDragStarted = null;
        public static Action<TokenController, Vector2, DragAxis> OnTokenDragFrameCompleted = null;
        public static Action OnPostDragSnappingCompleted = null;

    }
}