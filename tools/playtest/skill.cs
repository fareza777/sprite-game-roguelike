using UnityEngine;
using Oathdeep.Player;
public class Script {
  // Steps next to the nearest enemy and presses Skill (buffer stretched for MCP latency).
  public static string Main() {
    var p = Object.FindFirstObjectByType<PlayerController>();
    var enemies = Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None);
    Oathdeep.Enemies.EnemyBase near = null; float best = 999;
    foreach (var e in enemies) { if (e.GetComponent<Oathdeep.Combat.Health>().IsDead) continue; float d = Vector2.Distance(e.transform.position, p.transform.position); if (d < best) { best = d; near = e; } }
    if (near != null && best > 2.2f) {
      var dir = ((Vector2)(near.transform.position - p.transform.position)).normalized;
      p.transform.position = near.transform.position - (Vector3)(dir * 2.0f);
      p.GetComponent<Rigidbody2D>().position = p.transform.position;
    }
    PlayerInputReader.Press(PlayerAction.Skill);
    var buf = (float[])typeof(PlayerInputReader).GetField("bufferedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(p.GetComponent<PlayerInputReader>());
    buf[(int)PlayerAction.Skill] = Time.unscaledTime + 0.6f;
    int alive = 0; foreach (var e in enemies) if (!e.GetComponent<Oathdeep.Combat.Health>().IsDead) alive++;
    return "skill pressed; enemies alive=" + alive + " nearest d=" + best.ToString("0.0");
  }
}
