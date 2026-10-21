using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>Keeps HUD inside the device safe area (notches, rounded corners, gesture bars).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        Rect applied;
        Vector2Int screen;

        void Update()
        {
            var safe = Screen.safeArea;
            if (safe == applied && screen.x == Screen.width && screen.y == Screen.height) return;
            applied = safe;
            screen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;

            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rt.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
