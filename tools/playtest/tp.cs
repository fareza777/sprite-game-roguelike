using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
public class Script {
  public static string Main() {
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    if (b == null || p == null) return "no dungeon/player";
    var rooms = b.Geometry.Rooms;
    var target = rooms.Where(r => r.Node.Type != RoomType.Start && r.Node.Type != RoomType.Treasure && r.Node.Type != RoomType.Boss)
                      .OrderBy(r => Vector2.Distance(r.Centre, rooms[0].Centre)).First();
    p.GetComponent<Rigidbody2D>().position = new Vector2(target.Centre.x + 0.5f, target.Centre.y + 0.5f);
    p.transform.position = new Vector3(target.Centre.x + 0.5f, target.Centre.y + 0.5f, 0);
    return "teleported to " + target.Shape + " " + target.Node.Type + " at " + target.Centre + " rooms=" + rooms.Count + " types=" + string.Join(",", rooms.Select(r => r.Node.Type.ToString()[0]));
  }
}
