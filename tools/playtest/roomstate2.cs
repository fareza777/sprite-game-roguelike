using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
public class Script {
  // Which combat rooms are cleared, by challenge; and whether a dialog (spoils) is open.
  public static string Main() {
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    if (b == null) return "no dungeon";
    var rooms = string.Join(" ", b.Rooms.Where(r => r.Type == RoomType.Combat).Select(r => r.Challenge + (r.Cleared ? "+" : "-")));
    return rooms + " timeScale=" + Time.timeScale;
  }
}
