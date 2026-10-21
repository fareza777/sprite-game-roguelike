using UnityEngine;
using UnityEngine.InputSystem;

namespace Oathdeep.Player
{
    public enum PlayerAction { Attack, Dodge, Skill }

    /// <summary>
    /// Merges on-screen controls (touch) with keyboard/gamepad. Button presses are buffered briefly
    /// so inputs made during an attack are not eaten.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        const float BufferSeconds = 0.18f;

        static PlayerInputReader instance;
        Vector2 touchMove;
        readonly float[] bufferedUntil = new float[3];

        public Vector2 Move { get; private set; }

        void Awake() => instance = this;
        void OnDestroy() { if (instance == this) instance = null; }

        /// <summary>Called by the virtual joystick.</summary>
        public static void SetTouchMove(Vector2 value) { if (instance != null) instance.touchMove = Vector2.ClampMagnitude(value, 1f); }

        /// <summary>Called by on-screen action buttons.</summary>
        public static void Press(PlayerAction action) { if (instance != null) instance.Buffer(action); }

        void Buffer(PlayerAction action) => bufferedUntil[(int)action] = Time.unscaledTime + BufferSeconds;

        /// <summary>Returns true once per buffered press.</summary>
        public bool Consume(PlayerAction action)
        {
            int i = (int)action;
            if (Time.unscaledTime > bufferedUntil[i]) return false;
            bufferedUntil[i] = 0f;
            return true;
        }

        public bool IsBuffered(PlayerAction action) => Time.unscaledTime <= bufferedUntil[(int)action];

        void Update()
        {
            Vector2 move = touchMove;
            var kb = Keyboard.current;
            if (kb != null)
            {
                Vector2 k = new Vector2(
                    (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1 : 0) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1 : 0),
                    (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1 : 0) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1 : 0));
                if (k != Vector2.zero) move = k.normalized;
                if (kb.jKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) Buffer(PlayerAction.Attack);
                if (kb.kKey.wasPressedThisFrame || kb.leftShiftKey.wasPressedThisFrame) Buffer(PlayerAction.Dodge);
                if (kb.lKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame) Buffer(PlayerAction.Skill);
            }

            var pad = Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f) move = Vector2.ClampMagnitude(stick, 1f);
                if (pad.buttonWest.wasPressedThisFrame) Buffer(PlayerAction.Attack);
                if (pad.buttonSouth.wasPressedThisFrame) Buffer(PlayerAction.Dodge);
                if (pad.buttonNorth.wasPressedThisFrame) Buffer(PlayerAction.Skill);
            }

            Move = move;
        }
    }
}
