using UnityEngine;
using Oathdeep.Player;
public class Script {
  // Grants every talent and presses roll and attack; reports what the hero has and how many fire patches burn.
  public static string Main() {
    var p = Object.FindFirstObjectByType<PlayerController>();
    if (p == null) return "no hero";
    var t = p.GetComponent<PlayerTalents>();
    foreach (Talent k in System.Enum.GetValues(typeof(Talent))) t.Add(k);
    // The input buffer expires between editor ticks, so start the roll directly.
    typeof(PlayerController).GetMethod("StartDodge", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(p, null);
    return "talents=" + t.Owned.Count + " fires=" + Object.FindObjectsByType<FireTrail>(FindObjectsSortMode.None).Length;
  }
}
