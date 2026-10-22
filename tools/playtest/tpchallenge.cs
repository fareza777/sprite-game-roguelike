using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
public class Script {
  // Teleports the hero into the first uncleared room whose challenge is named in %TEMP%/undersun_challenge.txt
  // (Waves, Survive, Nests, Gauntlet) and lists every room's challenge.
  public static string Main() {
    var want = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_challenge.txt")).Trim();
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    if (b == null || p == null) return "no dungeon/player";
    var counts = string.Join(" ", b.Rooms.Where(r => r.Type == RoomType.Combat).GroupBy(r => r.Challenge).Select(g => g.Key + "=" + g.Count()));
    var target = b.Rooms.FirstOrDefault(r => r.Type == RoomType.Combat && !r.Cleared && r.Challenge.ToString() == want);
    if (target == null) return "none of " + want + "; rooms: " + counts;
    var c = target.WorldBounds.center;
    p.GetComponent<Rigidbody2D>().position = c;
    p.transform.position = c;
    return "into " + want + " room; rooms: " + counts;
  }
}
