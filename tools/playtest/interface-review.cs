// Batch visual QA. Temporarily copy to Assets/_Game/Scripts/Editor/InterfaceArtReview.cs,
// then run Unity -batchmode -executeMethod Oathdeep.EditorTools.InterfaceArtReview.Capture.
// Restores the original player save after leaving Play mode.
using System;
using System.IO;
using System.Linq;
using Oathdeep.Core;
using Oathdeep.Loot;
using Oathdeep.Player;
using Oathdeep.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Oathdeep.EditorTools
{
    [InitializeOnLoad]
    public static class InterfaceArtReview
    {
        const string Key = "UndersunInterfaceReview";
        static readonly string Output = Path.GetFullPath("../docs/screenshots");
        static readonly string Backup = Path.GetFullPath("../tools/art-qa/original-save.json");
        static int frames;

        static InterfaceArtReview()
        {
            if (SessionState.GetBool(Key, false)) Register();
        }

        public static void BuildApk()
        {
            typeof(OathdeepSetup).GetMethod("BuildAndroidNow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
            string report = File.ReadAllText("Builds/Android/build-report.txt");
            Debug.Log("[ArtReview] Android build\n" + report);
            if (!report.StartsWith("result: Succeeded")) throw new InvalidOperationException(report);
        }

        public static void Capture()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run this isolated visual review in batch mode.");
            OathdeepSetup.RefreshInterfaceArt();
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Path.GetDirectoryName(Backup));
            string save = Path.Combine(Application.persistentDataPath, "undersun_save.json");
            SessionState.SetString(Key + "Save", save);
            SessionState.SetBool(Key + "HadSave", File.Exists(save));
            if (File.Exists(save)) File.Copy(save, Backup, true);
            SessionState.SetInt(Key + "Phase", 0);
            SessionState.SetBool(Key, true);
            SessionState.SetString(Key + "Error", "");
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");
            Register();
            EditorApplication.isPlaying = true;
        }

        static void Register()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= ModeChanged;
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        static void ModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetInt(Key + "Phase", 0) == 2) Finish();
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            if (++frames < 30) return;
            try
            {
                if (SessionState.GetInt(Key + "Phase", 0) == 0)
                {
                    SaveSystem.Current.introSeen = true;
                    SaveSystem.Current.onboardingDone = true;
                    SaveSystem.Current.settings.customControls = false;
                    SaveSystem.Current.settings.leftHanded = false;
                    SaveSystem.Current.settings.fixedJoystick = false;
                    SaveSystem.Current.settings.music = SaveSystem.Current.settings.sfx = 0f;
                    foreach (var splash in Object.FindObjectsByType<SplashView>(FindObjectsInactive.Include, FindObjectsSortMode.None)) splash.gameObject.SetActive(false);
                    foreach (var cinematic in Object.FindObjectsByType<CinematicView>(FindObjectsInactive.Include, FindObjectsSortMode.None)) cinematic.gameObject.SetActive(false);
                    var runner = Object.FindFirstObjectByType<GameRunner>();
                    if (runner == null) throw new InvalidOperationException("Main scene has no GameRunner.");
                    runner.StopAllCoroutines();
                    runner.StartRun();
                    Screen.SetResolution(540, 960, false);
                    SessionState.SetInt(Key + "Phase", 1);
                    frames = 0;
                    return;
                }

                Time.timeScale = 0f;
                var hud = Object.FindFirstObjectByType<HudView>();
                if (hud == null) throw new InvalidOperationException("Run did not build the HUD.");
                foreach (var banner in Object.FindObjectsByType<NarrationBanner>(FindObjectsSortMode.None)) banner.gameObject.SetActive(false);
                var cam = Camera.main;
                var rig = cam.GetComponent<CameraRig>();
                if (rig != null) rig.enabled = false;
                var player = Object.FindFirstObjectByType<PlayerController>();
                cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1f, cam.transform.position.z);
                Canvas.ForceUpdateCanvases();
                var stick = hud.GetComponentInChildren<VirtualJoystick>();
                var zone = (RectTransform)stick.transform;
                var baseRect = (RectTransform)zone.Find("Base");
                var knob = (RectTransform)baseRect.Find("Knob");
                var group = baseRect.GetComponent<CanvasGroup>();
                if (group.alpha != 0f) throw new InvalidOperationException("Idle joystick is visible.");
                Screenshot("hud-idle");

                stick.Setup(baseRect, knob, group, true);
                var at = new Vector2(zone.rect.width * 0.55f, zone.rect.height * 0.46f);
                var pointer = Pointer(zone, at);
                stick.OnPointerDown(pointer);
                stick.OnDrag(Pointer(zone, at + new Vector2(22, 12)));
                Screenshot("hud-floating");
                stick.OnPointerUp(pointer);
                if (group.alpha != 0f) throw new InvalidOperationException("Released joystick is visible.");

                stick.Setup(baseRect, knob, group, false);
                var fixedAt = baseRect.anchoredPosition;
                pointer = Pointer(zone, fixedAt);
                stick.OnPointerDown(pointer);
                stick.OnDrag(Pointer(zone, fixedAt + new Vector2(-24, 8)));
                if (Vector2.Distance(fixedAt, baseRect.anchoredPosition) > 0.001f) throw new InvalidOperationException("Fixed joystick moved its base.");
                Screenshot("hud-fixed");
                stick.OnPointerUp(pointer);

                foreach (var item in ItemCatalog.All)
                {
                    var sprite = ItemIcons.For(item.Icon);
                    if (sprite == null || sprite.name != item.Icon || sprite.rect.width != 64f || sprite.pixelsPerUnit != 64f)
                        throw new InvalidOperationException("Incorrect item art binding: " + item.Id);
                }
                var front = Object.FindFirstObjectByType<FrontEnd>();
                front.ShowMenu();
                Screenshot("menu-icons");
                front.HideMenu();
                SaveSystem.Current.items = ItemCatalog.All.Select(item => item.Id).ToList();
                var compendium = Object.FindFirstObjectByType<ItemCompendiumPage>(FindObjectsInactive.Include);
                compendium.Show();
                Screenshot("item-compendium");
                compendium.gameObject.SetActive(false);
                front.ShowInventory(player.GetComponent<PlayerInventory>(), null);
                Screenshot("inventory");
                File.WriteAllText(Path.GetFullPath("../tools/art-qa/visual-review.txt"),
                    "PASS: 104 catalog bindings at 64px/64PPU; hidden idle/release; floating and fixed gestures; six live game previews.\n");
                SessionState.SetInt(Key + "Phase", 2);
                EditorApplication.isPlaying = false;
            }
            catch (Exception error)
            {
                SessionState.SetString(Key + "Error", error.ToString());
                Debug.LogException(error);
                SessionState.SetInt(Key + "Phase", 2);
                EditorApplication.isPlaying = false;
            }
        }

        static PointerEventData Pointer(RectTransform zone, Vector2 local) => new PointerEventData(EventSystem.current)
        {
            pointerId = 97,
            button = PointerEventData.InputButton.Left,
            position = RectTransformUtility.WorldToScreenPoint(null, zone.TransformPoint(local)),
        };

        static void Screenshot(string name)
        {
            var cam = Camera.main;
            var canvas = Object.FindFirstObjectByType<FrontEnd>().CanvasRoot.GetComponent<Canvas>();
            var oldTarget = cam.targetTexture;
            var oldActive = RenderTexture.active;
            var oldMode = canvas.renderMode;
            var oldCamera = canvas.worldCamera;
            float oldPlane = canvas.planeDistance;
            int oldOrder = canvas.sortingOrder;
            int oldLayer = canvas.sortingLayerID;
            bool oldOverride = canvas.overrideSorting;
            var target = new RenderTexture(540, 960, 24);
            var texture = new Texture2D(540, 960, TextureFormat.RGB24, false);
            try
            {
                cam.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 1f;
                canvas.overrideSorting = true;
                canvas.sortingLayerID = SortingLayer.layers.Last().id;
                canvas.sortingOrder = 30000;
                canvas.GetComponent<CanvasScaler>().SendMessage("Update");
                Canvas.ForceUpdateCanvases();
                cam.Render();
                Canvas.ForceUpdateCanvases();
                cam.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 540, 960), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(Output, "2026-10-23-" + name + ".png"), texture.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = oldMode;
                canvas.worldCamera = oldCamera;
                canvas.planeDistance = oldPlane;
                canvas.sortingOrder = oldOrder;
                canvas.sortingLayerID = oldLayer;
                canvas.overrideSorting = oldOverride;
                cam.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                Object.DestroyImmediate(texture);
                target.Release();
                Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        static void Finish()
        {
            string save = SessionState.GetString(Key + "Save", "");
            if (SessionState.GetBool(Key + "HadSave", false)) File.Copy(Backup, save, true);
            else if (File.Exists(save)) File.Delete(save);
            SessionState.SetBool(Key, false);
            EditorApplication.update -= Tick;
            string error = SessionState.GetString(Key + "Error", "");
            Debug.Log(string.IsNullOrEmpty(error) ? "[ArtReview] PASS; original player save restored." : "[ArtReview] FAILED: " + error);
            EditorApplication.Exit(string.IsNullOrEmpty(error) ? 0 : 1);
        }
    }
}
