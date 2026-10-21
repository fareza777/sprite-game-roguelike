using UnityEngine;
using System.Linq;
using System.Reflection;
using Oathdeep.Dungeon;
using Oathdeep.Player;
public class Script {
  // Steers toward the centre of the room connected through the first door of the nearest room,
  // attacking any enemy in reach. Reports room state and door colliders.
  public static string Main() {
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var p = Object.FindFirstObjectByType<PlayerController>();
    Vector2 pos = p.transform.position;
    var rooms = b.Geometry.Rooms;
    int stage = PlayerPrefs.GetInt("pt.stage", 0);
    var target = rooms[Mathf.Min(stage, rooms.Count - 1)];
    var enemies = Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None).Where(e => !e.GetComponent<Oathdeep.Combat.Health>().IsDead).ToList();
    Vector2 goal = target.Centre + Vector2.one * 0.5f;
    string mode = "walk";
    if (enemies.Count > 0) {
      var near = enemies.OrderBy(e => Vector2.Distance(e.transform.position, pos)).First();
      goal = near.transform.position; mode = "fight " + near.name;
      if (Vector2.Distance(goal, pos) < 1.4f) {
        PlayerInputReader.Press(PlayerAction.Attack);
        var buf = (float[])typeof(PlayerInputReader).GetField("bufferedUntil", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(p.GetComponent<PlayerInputReader>());
        buf[0] = Time.unscaledTime + 0.6f;
      }
    }
    Vector2 dir = goal - pos;
    PlayerInputReader.SetTouchMove(dir.magnitude > 0.6f ? dir.normalized : Vector2.zero);
    var ctrls = b.Rooms.Select(rc => rc.Type.ToString()[0] + (rc.Cleared ? "+" : "-")).ToArray();
    var doors = Object.FindObjectsByType<Door>(FindObjectsSortMode.None);
    return $"stage={stage} {mode} pos={pos:F1} goal={goal:F1} enemies={enemies.Count} rooms=[{string.Join("", ctrls)}] closedDoors={doors.Count(d => d.IsClosed)}/{doors.Length}";
  }
}
