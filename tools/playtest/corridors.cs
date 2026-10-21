using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
public class Script {
  // Casts the knight's collider between every pair of connected room centres and lists solid blockers.
  public static string Main() {
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var rooms = b.Geometry.Rooms;
    var byGrid = rooms.ToDictionary(r => r.Node.GridPosition, r => r);
    string s = ""; int bad = 0, total = 0;
    foreach (var a in rooms)
      foreach (var n in a.Node.Connections) {
        if (!byGrid.TryGetValue(n, out var c) || n.x < a.Node.GridPosition.x || n.y < a.Node.GridPosition.y) continue;
        total++;
        Vector2 from = a.Centre + new Vector2(0.5f, 0.75f), to = c.Centre + new Vector2(0.5f, 0.75f);
        var hits = Physics2D.CircleCastAll(from, 0.3f, (to - from).normalized, Vector2.Distance(from, to))
          .Where(h => !h.collider.isTrigger && h.collider.GetComponentInParent<Oathdeep.Player.PlayerController>() == null
                      && h.collider.GetComponentInParent<Oathdeep.Enemies.EnemyBase>() == null)
          .Select(h => h.collider.name + "@" + h.point.ToString("F1")).ToList();
        if (hits.Count > 0) { bad++; s += $"{a.Node.GridPosition}->{n} blocked by {string.Join(",", hits)} | "; }
      }
    return $"connections={total} blocked={bad} {s}";
  }
}
