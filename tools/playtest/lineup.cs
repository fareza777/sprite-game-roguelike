using UnityEngine;
using UnityEditor;
public class Script {
  // Spawns the enemy prefabs named in %TEMP%/undersun_lineup.txt (comma separated) in rows around the hero,
  // with their behaviours off so they stand in their idle animation for a screenshot.
  public static string Main() {
    var hero = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    if (hero == null) return "no hero";
    var names = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_lineup.txt")).Trim().Split(',');
    var origin = (Vector2)hero.transform.position;
    int cols = 3;
    for (int i = 0; i < names.Length; i++) {
      var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Enemies/" + names[i].Trim() + ".prefab");
      if (prefab == null) return "missing " + names[i];
      var go = Object.Instantiate(prefab, origin + new Vector2((i % cols - 1) * 2.2f, 1.5f - (i / cols) * 2.4f), Quaternion.identity);
      foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
      foreach (var rb in go.GetComponentsInChildren<Rigidbody2D>()) rb.simulated = false;
    }
    return "spawned " + names.Length;
  }
}
