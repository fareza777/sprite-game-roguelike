using UnityEngine;
using Oathdeep.Player;
public class Script {
  public static string Main() {
    var p = Object.FindFirstObjectByType<PlayerController>();
    var enemies = Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None);
    Oathdeep.Enemies.EnemyBase near = null; float best = 999;
    foreach (var e in enemies) { float d = Vector2.Distance(e.transform.position, p.transform.position); if (d < best) { best = d; near = e; } }
    if (near != null && best > 1.2f) {
      // step next to the nearest enemy so the combo connects
      var dir = ((Vector2)(near.transform.position - p.transform.position)).normalized;
      p.transform.position = near.transform.position - (Vector3)(dir * 1.0f);
      p.GetComponent<Rigidbody2D>().position = p.transform.position;
    }
    PlayerInputReader.Press(PlayerAction.Attack);
    var buf = (float[])typeof(PlayerInputReader).GetField("bufferedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(p.GetComponent<PlayerInputReader>());
    buf[0] = Time.unscaledTime + 0.6f; // MCP round-trips are slower than a frame
    var h = near ? near.GetComponent<Oathdeep.Combat.Health>() : null;
    return "enemies=" + enemies.Length + " nearest=" + (near ? near.name + " d=" + best.ToString("0.0") + " hp=" + h.Current + "/" + h.Max : "none");
  }
}
