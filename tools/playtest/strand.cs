using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
public class Script {
  // Shoves the first living enemy 3 cells past the room's left wall, simulating a bad summon.
  public static string Main() {
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var e = Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None).FirstOrDefault(x => !x.GetComponent<Oathdeep.Combat.Health>().IsDead);
    if (e == null) return "no enemy yet";
    var room = b.Geometry.Rooms.OrderBy(r => Vector2.Distance(e.transform.position, r.Centre)).First();
    var to = new Vector2(room.Interior.xMin - 3f, room.Interior.center.y);
    e.GetComponent<Rigidbody2D>().position = to; e.transform.position = to;
    PlayerPrefs.SetInt("pt.stranded", e.GetInstanceID());
    return "moved " + e.name + " to " + to + " interior=" + room.Interior;
  }
}
