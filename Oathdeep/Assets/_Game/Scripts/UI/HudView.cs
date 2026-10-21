using System.Collections;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>In-run HUD: health, gold, floor, boss bar, toasts, touch controls and minimap.</summary>
    public sealed class HudView : MonoBehaviour
    {
        UISkin skin;
        Image hpFill;
        Image hpLag;
        TextMeshProUGUI hpText;
        TextMeshProUGUI goldText;
        TextMeshProUGUI floorText;
        RectTransform bossRoot;
        Image bossFill;
        TextMeshProUGUI bossName;
        TextMeshProUGUI toast;
        CanvasGroup toastGroup;
        Coroutine toastRoutine;
        float hpTarget = 1f;
        Sprite skillIcon;

        public MinimapView Minimap { get; private set; }

        public void Build(UISkin uiSkin, RectTransform safeArea, PlayerController player, System.Action onPause, System.Action onBag, Sprite heroSkillIcon = null)
        {
            skin = uiSkin;
            skillIcon = heroSkillIcon != null ? heroSkillIcon : skin.iconSkill;

            // --- Top-left: health ---
            var hpFrame = UIFactory.Rect("HP", safeArea, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(150, 18));
            UIFactory.Image(hpFrame, skin.barFrame, null, true);
            var lagRect = UIFactory.Stretch("Lag", hpFrame); lagRect.offsetMin = new Vector2(3, 3); lagRect.offsetMax = new Vector2(-3, -3);
            hpLag = UIFactory.Image(lagRect, skin.white, skin.dangerColor);
            hpLag.type = Image.Type.Filled; hpLag.fillMethod = Image.FillMethod.Horizontal; hpLag.preserveAspect = false;
            var fillRect = UIFactory.Stretch("Fill", hpFrame); fillRect.offsetMin = new Vector2(3, 3); fillRect.offsetMax = new Vector2(-3, -3);
            hpFill = UIFactory.Image(fillRect, skin.barFillHealth != null ? skin.barFillHealth : skin.white, skin.barFillHealth != null ? Color.white : new Color32(0x75, 0xA7, 0x43, 0xFF));
            hpFill.type = Image.Type.Filled; hpFill.fillMethod = Image.FillMethod.Horizontal; hpFill.preserveAspect = false;
            var hpTextRect = UIFactory.Stretch("Value", hpFrame);
            hpText = UIFactory.Text(hpTextRect, skin, "100/100", 12, TextAlignmentOptions.Center);

            // --- Top-right: gold + pause ---
            var pause = UIFactory.Rect("Pause", safeArea, Vector2.one, Vector2.one, Vector2.one, new Vector2(-8, -8), new Vector2(30, 30));
            var pauseImg = UIFactory.Image(pause, skin.iconPause, null, false, true);
            pause.gameObject.AddComponent<Button>().onClick.AddListener(() => onPause?.Invoke());
            pauseImg.preserveAspect = true;

            var goldIcon = UIFactory.Rect("GoldIcon", safeArea, Vector2.one, Vector2.one, Vector2.one, new Vector2(-46, -12), new Vector2(16, 16));
            UIFactory.Image(goldIcon, skin.iconGold);
            var goldRect = UIFactory.Rect("Gold", safeArea, Vector2.one, Vector2.one, Vector2.one, new Vector2(-66, -10), new Vector2(70, 20));
            goldText = UIFactory.Text(goldRect, skin, "0", 14, TextAlignmentOptions.Right, skin.accentColor);

            var floorRect = UIFactory.Rect("Floor", safeArea, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -32), new Vector2(200, 16));
            floorText = UIFactory.Text(floorRect, skin, "B1  Sunken Crypt", 10, TextAlignmentOptions.Left, new Color(1, 1, 1, 0.7f));

            // --- Minimap under the gold counter ---
            var mapRect = UIFactory.Rect("Minimap", safeArea, Vector2.one, Vector2.one, Vector2.one, new Vector2(-8, -44), new Vector2(84, 84));
            Minimap = mapRect.gameObject.AddComponent<MinimapView>();
            Minimap.Build(skin);

            // --- Bag under the minimap ---
            var bag = UIFactory.Rect("Bag", safeArea, Vector2.one, Vector2.one, Vector2.one, new Vector2(-8, -134), new Vector2(34, 34));
            UIWidgets.IconButton(bag, skin, skin.iconBag, () => onBag?.Invoke());

            // --- Boss bar (hidden until a boss appears) ---
            bossRoot = UIFactory.Rect("Boss", safeArea, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -136), new Vector2(260, 30));
            var bossFrame = UIFactory.Rect("Frame", bossRoot, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 14));
            UIFactory.Image(bossFrame, skin.barFrame, null, true);
            var bossFillRect = UIFactory.Stretch("Fill", bossFrame); bossFillRect.offsetMin = new Vector2(3, 3); bossFillRect.offsetMax = new Vector2(-3, -3);
            bossFill = UIFactory.Image(bossFillRect, skin.barFillBoss != null ? skin.barFillBoss : skin.white, skin.barFillBoss != null ? Color.white : skin.dangerColor);
            bossFill.type = Image.Type.Filled; bossFill.fillMethod = Image.FillMethod.Horizontal; bossFill.preserveAspect = false;
            var bossNameRect = UIFactory.Rect("Name", bossRoot, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 14));
            bossName = UIFactory.Text(bossNameRect, skin, "", 12, TextAlignmentOptions.Center, skin.dangerColor);
            bossRoot.gameObject.SetActive(false);

            // --- Toast ---
            var toastRect = UIFactory.Rect("Toast", safeArea, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320, 24));
            toast = UIFactory.Text(toastRect, skin, "", 14, TextAlignmentOptions.Center, skin.accentColor);
            toastGroup = toastRect.gameObject.AddComponent<CanvasGroup>();
            toastGroup.alpha = 0f;

            BuildControls(safeArea, player);
            Subscribe(player);
        }

        RectTransform controlsRoot;
        RectTransform safeAreaRoot;
        PlayerController controlledPlayer;

        /// <summary>A touch control by name ("Attack", "Dodge", "Skill", "MoveZone") for tutorials to point at.</summary>
        public RectTransform ControlRect(string name) => controlsRoot != null ? controlsRoot.Find(name) as RectTransform : null;

        /// <summary>Re-lays out the touch controls (after the joystick / handedness settings change).</summary>
        public void RebuildControls()
        {
            if (controlsRoot != null) Destroy(controlsRoot.gameObject);
            BuildControls(safeAreaRoot, controlledPlayer);
        }

        void BuildControls(RectTransform safeArea, PlayerController player)
        {
            safeAreaRoot = safeArea;
            controlledPlayer = player;
            var layout = ControlLayout.Compute(GameSettings.LeftHanded);
            controlsRoot = UIFactory.Stretch("Controls", safeArea);
            controlsRoot.SetAsFirstSibling();   // under the HUD readouts, above the world

            var zone = UIFactory.Rect("MoveZone", controlsRoot, layout.MoveZoneMin, layout.MoveZoneMax, Vector2.zero, Vector2.zero, Vector2.zero);
            var zoneImg = UIFactory.Image(zone, skin.white, new Color(0, 0, 0, 0), false, true);
            zoneImg.preserveAspect = false;
            var stickBase = UIFactory.Rect("Base", zone, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), ControlLayout.StickRest, new Vector2(112, 112));
            UIFactory.Image(stickBase, skin.joystickBase);
            var knob = UIFactory.Rect("Knob", stickBase, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52, 52));
            UIFactory.Image(knob, skin.joystickKnob);
            var group = stickBase.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var stick = zone.gameObject.AddComponent<VirtualJoystick>();
            stick.Setup(stickBase, knob, group, !GameSettings.FixedJoystick);

            var corner = layout.LeftHanded ? Vector2.zero : new Vector2(1, 0);
            MakeAction(controlsRoot, corner, "Attack", PlayerAction.Attack, skin.iconAttack, layout.Attack, null);
            MakeAction(controlsRoot, corner, "Dodge", PlayerAction.Dodge, skin.iconDodge, layout.Dodge, () => player != null ? player.DodgeCooldownFraction : 0f);
            MakeAction(controlsRoot, corner, "Skill", PlayerAction.Skill, skillIcon, layout.Skill, () => player != null ? player.SkillCooldownFraction : 0f);
        }

        void MakeAction(RectTransform parent, Vector2 corner, string name, PlayerAction action, Sprite icon, ControlLayout.Slot slot, System.Func<float> cooldown)
        {
            float size = slot.Size;
            var root = UIFactory.Rect(name, parent, corner, corner, new Vector2(0.5f, 0.5f), slot.Position, new Vector2(size, size));
            var bg = UIFactory.Image(root, skin.actionRing, null, false, true);
            bg.color = new Color(1, 1, 1, 0.85f);
            var iconRect = UIFactory.Rect("Icon", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.55f, size * 0.55f));
            UIFactory.Image(iconRect, icon);
            Image fill = null;
            if (cooldown != null)
            {
                var fillRect = UIFactory.Stretch("Cooldown", root);
                fill = UIFactory.Image(fillRect, skin.actionRing, new Color(0, 0, 0, 0.6f));
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Radial360;
                fill.fillOrigin = (int)Image.Origin360.Top;
                fill.fillClockwise = false;
            }
            root.gameObject.AddComponent<ActionButton>().Setup(action, fill, root, cooldown);
        }

        void Subscribe(PlayerController player)
        {
            var health = player.GetComponent<Health>();
            health.Changed += (cur, max) =>
            {
                hpTarget = max > 0 ? (float)cur / max : 0f;
                hpFill.fillAmount = hpTarget;
                hpText.text = $"{cur}/{max}";
            };
            GameEvents.GoldChanged += g => goldText.text = g.ToString();
            GameEvents.Toast += ShowToast;
            GameEvents.BossEncountered += n => { bossRoot.gameObject.SetActive(true); bossName.text = n; };
            GameEvents.BossHealthChanged += (c, m) => bossFill.fillAmount = m > 0 ? c / m : 0f;
            GameEvents.BossDefeated += () => bossRoot.gameObject.SetActive(false);
        }

        public void SetFloor(int number, string biome) => floorText.text = $"B{number}  {biome}";

        void Update()
        {
            if (hpLag != null) hpLag.fillAmount = Mathf.MoveTowards(hpLag.fillAmount, hpTarget, Time.unscaledDeltaTime * 0.6f);
        }

        void ShowToast(string message)
        {
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine(message));
        }

        IEnumerator ToastRoutine(string message)
        {
            toast.text = message;
            for (float t = 0; t < 0.15f; t += Time.unscaledDeltaTime) { toastGroup.alpha = t / 0.15f; yield return null; }
            toastGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(1.6f);
            for (float t = 0; t < 0.4f; t += Time.unscaledDeltaTime) { toastGroup.alpha = 1f - t / 0.4f; yield return null; }
            toastGroup.alpha = 0f;
        }
    }
}
