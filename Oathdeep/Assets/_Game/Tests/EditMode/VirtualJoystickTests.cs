using NUnit.Framework;
using System.Reflection;
using Oathdeep.Player;
using Oathdeep.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Oathdeep.Tests
{
    public class VirtualJoystickTests
    {
        GameObject root;
        RectTransform zone, stickBase, knob;
        CanvasGroup visual;
        VirtualJoystick stick;
        PlayerInputReader input;
        static readonly Vector2 Rest = new Vector2(92, 132);

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("JoystickTest", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = new Vector2(360, 640);
            zone = Rect("Zone", rect, new Vector2(180, 288), Vector2.zero);
            zone.pivot = Vector2.zero;
            stickBase = Rect("Base", zone, Vector2.one * 112, Rest);
            knob = Rect("Knob", stickBase, Vector2.one * 44, Vector2.zero);
            knob.anchorMin = knob.anchorMax = Vector2.one * 0.5f;
            visual = stickBase.gameObject.AddComponent<CanvasGroup>();
            input = root.AddComponent<PlayerInputReader>();
            stick = zone.gameObject.AddComponent<VirtualJoystick>();
            // EditMode does not invoke MonoBehaviour.Awake for ordinary components.
            Callback(input, "Awake");
            Callback(stick, "Awake");
            stick.Setup(stickBase, knob, visual, true);
        }

        [TearDown]
        public void TearDown()
        {
            Callback(stick, "OnDisable");
            Callback(input, "OnDestroy");
            Object.DestroyImmediate(root);
        }

        // Invoke lifecycle callbacks directly: SendMessage asserts for non-executing EditMode behaviours.
        static void Callback(object target, string name, params object[] args) =>
            target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, args);

        static RectTransform Rect(string name, RectTransform parent, Vector2 size, Vector2 at)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.sizeDelta = size;
            rect.anchoredPosition = at;
            return rect;
        }

        PointerEventData Pointer(int id, Vector2 at) => new PointerEventData(null)
        {
            pointerId = id,
            position = RectTransformUtility.WorldToScreenPoint(null, zone.TransformPoint(at)),
            button = PointerEventData.InputButton.Left,
        };

        Vector2 Movement()
        {
            Callback(input, "Update");
            return input.Move;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Idle_IsCompletelyHidden_InBothModes(bool floating)
        {
            stick.Setup(stickBase, knob, visual, floating);
            Assert.AreEqual(0f, visual.alpha);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Touch_ShowsStick_ReleaseHidesAndStopsIt(bool floating)
        {
            stick.Setup(stickBase, knob, visual, floating);
            stick.OnPointerDown(Pointer(1, Rest));
            Assert.Greater(visual.alpha, 0.8f);
            stick.OnDrag(Pointer(1, Rest + Vector2.right * 20));
            Assert.Greater(Movement().x, 0f);
            stick.OnPointerUp(Pointer(1, Rest));
            Assert.AreEqual(0f, visual.alpha);
            Assert.AreEqual(Vector2.zero, knob.anchoredPosition);
            Assert.AreEqual(Vector2.zero, Movement());
        }

        [TestCase(8f, 8f)]
        [TestCase(170f, 270f)]
        public void FloatingTouch_StartsUnderFinger_WithoutMovingTheHero(float x, float y)
        {
            var point = new Vector2(x, y);
            stick.OnPointerDown(Pointer(1, point));
            Assert.Less(Vector2.Distance(point, stickBase.anchoredPosition), 0.01f);
            Assert.AreEqual(Vector2.zero, Movement());
        }

        [Test]
        public void FloatingStick_FollowsLongDrags_EvenOutsideTheStartingZone()
        {
            stick.OnPointerDown(Pointer(1, Rest));
            stick.OnDrag(Pointer(1, new Vector2(250, 310)));
            Assert.Greater(stickBase.anchoredPosition.x, zone.rect.width);
            Assert.AreEqual(1f, Movement().magnitude, 0.001f);
        }

        [Test]
        public void FixedStick_KeepsItsOriginDuringEveryTouch()
        {
            stick.Setup(stickBase, knob, visual, false);
            stick.OnPointerDown(Pointer(1, new Vector2(150, 220)));
            stick.OnDrag(Pointer(1, new Vector2(170, 250)));
            Assert.AreEqual(Rest, stickBase.anchoredPosition);
            stick.OnPointerUp(Pointer(1, Rest));
            stick.OnPointerDown(Pointer(2, new Vector2(80, 80)));
            Assert.AreEqual(Rest, stickBase.anchoredPosition);
        }

        [Test]
        public void FixedStick_HonoursTheSavedCustomPosition()
        {
            stick.Setup(stickBase, knob, visual, false);
            var fraction = new Vector2(0.6f, 0.7f);
            stick.UseRestFraction(fraction);
            var expected = Vector2.Scale(zone.rect.size, fraction);
            stick.OnPointerDown(Pointer(1, new Vector2(20, 20)));
            stick.OnDrag(Pointer(1, new Vector2(170, 250)));
            stick.OnPointerUp(Pointer(1, Rest));
            Assert.Less(Vector2.Distance(expected, stickBase.anchoredPosition), 0.001f);
        }

        [TestCase(-1)]
        [TestCase(1)]
        public void AnotherPointer_CannotStealOrReleaseTheStick(int owner)
        {
            stick.OnPointerDown(Pointer(owner, Rest));
            stick.OnDrag(Pointer(owner, Rest + Vector2.right * 20));
            var position = stickBase.anchoredPosition;
            var offset = knob.anchoredPosition;
            stick.OnPointerDown(Pointer(77, new Vector2(150, 250)));
            stick.OnDrag(Pointer(77, new Vector2(160, 260)));
            stick.OnPointerUp(Pointer(77, Rest));
            Assert.AreEqual(position, stickBase.anchoredPosition);
            Assert.AreEqual(offset, knob.anchoredPosition);
            Assert.Greater(visual.alpha, 0.8f);
            stick.OnPointerUp(Pointer(owner, Rest));
            Assert.AreEqual(0f, visual.alpha);
        }

        [Test]
        public void Knob_StaysFullyInsideTheBase()
        {
            stick.Setup(stickBase, knob, visual, false);
            stick.OnPointerDown(Pointer(1, Rest));
            stick.OnDrag(Pointer(1, Rest + Vector2.right * 200));
            Assert.LessOrEqual(knob.anchoredPosition.magnitude + knob.rect.width / 2f, stickBase.rect.width / 2f + 0.001f);
        }

        [Test]
        public void DisableAndReenable_ResetsVisualsAndMovement()
        {
            stick.OnPointerDown(Pointer(1, Rest));
            stick.OnDrag(Pointer(1, Rest + Vector2.right * 20));
            Callback(stick, "OnDisable");
            stick.enabled = false;
            Assert.AreEqual(0f, visual.alpha);
            Assert.AreEqual(Vector2.zero, knob.anchoredPosition);
            Assert.AreEqual(Vector2.zero, Movement());
            stick.enabled = true;
            stick.OnPointerDown(Pointer(2, new Vector2(120, 200)));
            Assert.Greater(visual.alpha, 0.8f);
        }

        [TestCase("OnApplicationFocus", false)]
        [TestCase("OnApplicationPause", true)]
        public void InterruptedGesture_HidesAndReleasesTheStick(string callback, bool value)
        {
            stick.OnPointerDown(Pointer(1, Rest));
            stick.OnDrag(Pointer(1, Rest + Vector2.right * 20));
            Callback(stick, callback, value);
            Assert.AreEqual(0f, visual.alpha);
            Assert.AreEqual(Vector2.zero, Movement());
        }

        [Test]
        public void SmallThumbJitter_StaysInsideTheDeadZone()
        {
            stick.OnPointerDown(Pointer(1, Rest));
            stick.OnDrag(Pointer(1, Rest + Vector2.right));
            Assert.AreEqual(Vector2.zero, Movement());
        }
    }
}
