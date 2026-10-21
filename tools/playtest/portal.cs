using UnityEngine;
public class Script {
  // Walks the hero onto the exit portal if one exists and reports the current floor.
  public static string Main() {
    var portal = Object.FindFirstObjectByType<Oathdeep.Dungeon.ExitPortal>();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    var floorText = Object.FindFirstObjectByType<Oathdeep.UI.HudView>();
    if (portal == null) return "no portal yet";
    p.GetComponent<Rigidbody2D>().position = portal.transform.position; p.transform.position = portal.transform.position;
    return "on portal";
  }
}
