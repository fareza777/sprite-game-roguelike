using UnityEngine;
using Oathdeep.Player;
public class Script {
  public static string Main() {
    var p = Object.FindFirstObjectByType<PlayerController>();
    if (p == null) return "no hero";
    var st = typeof(PlayerController).GetField("state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(p);
    return "state=" + st + " timeScale=" + Time.timeScale + " pos=" + p.transform.position + " fires=" + Object.FindObjectsByType<FireTrail>(FindObjectsSortMode.None).Length;
  }
}
