using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
using Oathdeep.Player;
public class Script {
  // Steers the knight toward the first door of the room it stands in and reports progress.
  public static string Main() {
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var p = Object.FindFirstObjectByType<PlayerController>();
    if (b == null || p == null) return "no run";
    Vector2 pos = p.transform.position;
    var room = b.Geometry.Rooms.OrderBy(r => Vector2.Distance(pos, r.Centre)).First();
    var span = room.Doors[0];
    var r = span.Cells;
    Vector2 door = new Vector2(r.x + r.width * 0.5f, r.y + r.height * 0.5f);
    Vector2 target = door + (Vector2)span.Direction * 3f;   // a few cells past the doorway
    Vector2 dir = (target - pos);
    PlayerInputReader.SetTouchMove(dir.magnitude > 0.3f ? dir.normalized : Vector2.zero);
    var hits = Physics2D.OverlapBoxAll(door, new Vector2(r.width, r.height), 0f).Select(c => c.name + (c.enabled ? "" : "(off)") + (c.isTrigger ? "(T)" : ""));
    return $"room={room.Node.Type} pos={pos} door={door} span={r} dir={span.Direction} target={target} dist={dir.magnitude:0.0} doorColliders=[{string.Join(",", hits)}]";
  }
}
