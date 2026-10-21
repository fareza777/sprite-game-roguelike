using UnityEngine;
using UnityEditor;
public class Script {
  public static string Main() {
    var cam = Camera.main;
    int w = 540, h = 960;
    var canvasGo = GameObject.Find("UI");
    var canvas = canvasGo ? canvasGo.GetComponent<Canvas>() : null;
    var rt = new RenderTexture(w, h, 24);
    var prev = cam.targetTexture;
    cam.targetTexture = rt;
    if (canvas) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 1f; Canvas.ForceUpdateCanvases(); }
    cam.Render();
    if (canvas) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    cam.targetTexture = prev;
    RenderTexture.active = rt;
    var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
    tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
    RenderTexture.active = null;
    var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "oathdeep_ui.png");
    System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
    Object.DestroyImmediate(tex); rt.Release();
    return path;
  }
}
