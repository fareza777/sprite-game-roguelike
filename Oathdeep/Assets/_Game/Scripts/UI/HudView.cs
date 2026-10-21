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
        RectTransform interactRoot;
        TextMeshProUGUI interactLabel;
        Oathdeep.Dungeon.Interactable nearby;
        PlayerController trackedPlayer;
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

            // --- Context button (TALK / TRADE / TOUCH...) above the controls, shown near an interactable ---
            interactRoot = UIFactory.Rect("Interact", safeArea, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 236), new Vector2(132, 40));
            UIFactory.Button(interactRoot, skin, "TALK", 15, () => { if (nearby != null) nearby.Use(); });
            interactLabel = interactRoot.GetComponentInChildren<TextMeshProUGUI>();
            interactRoot.gameObject.SetActive(false);
            trackedPlayer = player;

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
            // Wraps onto a second line and shrinks a little rather than cutting long item names off.
            // Stretches with the screen: a 20:9 phone's canvas is only ~322 units wide, narrower than a fixed box.
            var toastRect = UIFactory.Rect("Toast", safeArea, new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 44));
            var toastBack = UIFactory.Image(toastRect, skin.white, new Color(0.03f, 0.04f, 0.08f, 0.6f));
            toastBack.preserveAspect = false;
            var toastText = UIFactory.Stretch("Text", toastRect);
            toastText.offsetMin = new Vector2(8, 3);
            toastText.offsetMax = new Vector2(-8, -3);
            toast = UIFactory.Text(toastText, skin, "", 14, TextAlignmentOptions.Center, skin.accentColor);
            toast.textWrappingMode = TextWrappingModes.Normal;
            toast.enableAutoSizing = true;
            toast.fontSizeMin = 9;
            toast.fontSizeMax = 14;
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

            // Player-placed controls (Settings > Customize layout) override the built-in spots.
            bool custom = GameSettings.CustomControls;
            var placed = GameSettings.ControlPositions;   // stick, attack, dodge, skill
            var (zoneMin, zoneMax) = custom ? ControlPlacement.MoveZoneFor(placed[0]) : (layout.MoveZoneMin, layout.MoveZoneMax);
            var zone = UIFactory.Rect("MoveZone", controlsRoot, zoneMin, zoneMax, Vector2.zero, Vector2.zero, Vector2.zero);
            var zoneImg = UIFactory.Image(zone, skin.white, new Color(0, 0, 0, 0), false, true);
            zoneImg.preserveAspect = false;
            var stickBase = UIFactory.Rect("Base", zone, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), ControlLayout.StickRest, Vector2.one * ControlLayout.StickSize);
            UIFactory.Image(stickBase, skin.joystickBase);
            var knob = UIFactory.Rect("Knob", stickBase, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52, 52));
            UIFactory.Image(knob, skin.joystickKnob);
            var group = stickBase.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var stick = zone.gameObject.AddComponent<VirtualJoystick>();
            stick.Setup(stickBase, knob, group, !GameSettings.FixedJoystick);
            if (custom) stick.UseRestFraction(ControlPlacement.RestInZone(placed[0], zoneMin, zoneMax));

            MakeAction(controlsRoot, "Attack", PlayerAction.Attack, skin.iconAttack, layout.Attack, custom ? placed[1] : (Vector2?)null, null);
            MakeAction(controlsRoot, "Dodge", PlayerAction.Dodge, skin.iconDodge, layout.Dodge, custom ? placed[2] : (Vector2?)null,
                () => player != null ? player.DodgeCooldownFraction : 0f);
            MakeAction(controlsRoot, "Skill", PlayerAction.Skill, skillIcon, layout.Skill, custom ? placed[3] : (Vector2?)null,
                () => player != null ? player.SkillCooldownFraction : 0f);
        }

        /// <param name="placedAt">A player-chosen centre (fraction of the safe area), or null for the slot's built-in spot.</param>
        void MakeAction(RectTransform parent, string name, PlayerAction action, Sprite icon, ControlLayout.Slot slot, Vector2? placedAt,
            System.Func<float> cooldown)
        {
            float size = slot.Size;
            var anchor = placedAt ?? new Vector2(slot.AnchorX, 0f);
            var offset = placedAt.HasValue ? Vector2.zero : slot.Position;
            var root = UIFactory.Rect(name, parent, anchor, anchor, new Vector2(0.5f, 0.5f), offset, new Vector2(size, size));
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
            toast.ForceMeshUpdate();
            for (float t = 0; t < 0.15f; t += Time.unscaledDeltaTime) { toastGroup.alpha = t / 0.15f; yield return null; }
            toastGroup.alpha = 1f;
            // Longer messages stay up longer so they can be read (rich-text tags do not count).
            yield return new WaitForSecondsRealtime(Mathf.Clamp(1.2f + toast.GetParsedText().Length * 0.03f, 1.6f, 3.2f));
            for (float t = 0; t < 0.4f; t += Time.unscaledDeltaTime) { toastGroup.alpha = 1f - t / 0.4f; yield return null; }
            toastGroup.alpha = 0f;
        }

        /// <summary>Shows the context button while the hero stands by something usable. E also works on keyboards.</summary>
        void LateUpdate()
        {
            if (interactRoot == null) return;
            nearby = trackedPlayer != null && !trackedPlayer.IsDead ? Oathdeep.Dungeon.Interactable.Nearest(trackedPlayer.transform.position) : null;
            bool show = nearby != null && Time.timeScale > 0f;
            if (interactRoot.gameObject.activeSelf != show) interactRoot.gameObject.SetActive(show);
            if (!show) return;
            if (interactLabel.text != nearby.Prompt) interactLabel.text = nearby.Prompt;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame) nearby.Use();
        }
    }
}
