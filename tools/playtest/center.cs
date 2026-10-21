using UnityEngine;
public class Script {
  // Moves the hero to the camera's centre (useful while the landscape editor view clamps the camera).
  public static string Main() {
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    Vector2 c = Camera.main.transform.position + Vector3.down * 2f;
    p.GetComponent<Rigidbody2D>().position = c; p.transform.position = c;
    return "hero at " + c;
  }
}
