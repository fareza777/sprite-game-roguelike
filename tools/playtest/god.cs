using UnityEngine;
public class Script {
  public static string Main() {
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    p.GetComponent<Oathdeep.Combat.Health>().PushInvulnerable();
    return "god on";
  }
}
