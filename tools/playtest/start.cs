using UnityEngine;
using UnityEditor;
public class Script {
  public static string Main() {
    if (!EditorApplication.isPlaying) return "not playing";
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    if (runner == null) return "no runner";
    runner.StartRun();
    return "run started; objects: " + Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length;
  }
}
