using System.Collections.Generic;

namespace Oathdeep.UI
{
    /// <summary>Pure rule for "is the finger that grabbed a control still down?", used by the stick's watchdog.</summary>
    public static class PointerHold
    {
        /// <param name="touchId">The grabbing touch, or -1 when the event carried no touch id.</param>
        /// <param name="touches">Every touch the screen knows about: (id, pressed).</param>
        /// <param name="mouseDown">The mouse's left button, or null when there is no mouse (phones).</param>
        public static bool StillDown(int touchId, IEnumerable<(int id, bool pressed)> touches, bool? mouseDown)
        {
            if (touchId >= 0)
            {
                foreach (var t in touches)
                    if (t.id == touchId && t.pressed) return true;
                return false;
            }
            if (mouseDown == true) return true;
            // No touch id: held only while some finger is actually down. Never "held" by default.
            foreach (var t in touches)
                if (t.pressed) return true;
            return false;
        }
    }
}
