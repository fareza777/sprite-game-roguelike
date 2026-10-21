using UnityEngine;
public class Script {
  public static string Main() {
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    var rb = p.GetComponent<Rigidbody2D>();
    return "pos=" + p.transform.position + " rb=" + rb.position + " sim=" + rb.simulated + " t=" + Time.timeScale + " cam=" + Camera.main.transform.position;
  }
}
