using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>
    /// Which world rect the camera is confined to. A room sets it on entry and releases it on exit,
    /// so corridors between rooms are followed freely instead of leaving the player off-screen.
    /// </summary>
    public sealed class CameraBounds
    {
        Rect rect;

        public bool Active { get; private set; }

        public void Set(Rect worldRect)
        {
            rect = worldRect;
            Active = true;
        }

        /// <summary>Releases only if `worldRect` is the one in force (exit events can arrive after the next room's enter).</summary>
        public void Release(Rect worldRect)
        {
            if (Active && rect == worldRect) Active = false;
        }

        public Vector2 Clamp(Vector2 p, float halfW, float halfH)
        {
            if (!Active) return p;
            p.x = rect.width <= halfW * 2f ? rect.center.x : Mathf.Clamp(p.x, rect.xMin + halfW, rect.xMax - halfW);
            p.y = rect.height <= halfH * 2f ? rect.center.y : Mathf.Clamp(p.y, rect.yMin + halfH, rect.yMax - halfH);
            return p;
        }
    }
}
