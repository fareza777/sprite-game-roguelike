using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
public class Script {
  // Teleports the hero next to the encounter in the first room of the type named in %TEMP%/undersun_room.txt and uses it.
  public static string Main() {
    var type = (RoomType)System.Enum.Parse(typeof(RoomType), System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_room.txt")).Trim());
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    var room = b.Geometry.Rooms.FirstOrDefault(r => r.Node.Type == type);
    if (room == null) return "no " + type + " room; types=" + string.Join(",", b.Geometry.Rooms.Select(r => r.Node.Type));
    Vector2 to = new Vector2(room.Centre.x + 0.5f, room.Centre.y + 0.5f);
    var enc = Object.FindObjectsByType<RoomEncounter>(FindObjectsSortMode.None).OrderBy(e => Vector2.Distance(e.transform.position, to)).FirstOrDefault();
    if (enc != null) to = (Vector2)enc.transform.position + new Vector2(-1.2f, -0.3f);
    p.GetComponent<Rigidbody2D>().position = to; p.transform.position = to;
    if (enc != null) enc.GetComponent<Interactable>().Use();
    return "at " + type + " " + room.Shape + (enc != null ? " encounter=" + enc.Kind : "");
  }
}
