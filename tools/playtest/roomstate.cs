using UnityEngine;
using System.Linq;
using System.Reflection;
using Oathdeep.Dungeon;
public class Script {
  public static string Main() {
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    var f = BindingFlags.NonPublic | BindingFlags.Instance;
    string s = "";
    foreach (var rc in b.Rooms) {
      bool fighting = (bool)typeof(RoomController).GetField("fighting", f).GetValue(rc);
      bool visited = (bool)typeof(RoomController).GetField("visited", f).GetValue(rc);
      if (!visited) continue;
      var root = (Transform)typeof(RoomController).GetField("enemyRoot", f).GetValue(rc);
      var kids = root.Cast<Transform>().Select(t => t.name + ":" + (t.GetComponent<Oathdeep.Combat.Health>()?.IsDead.ToString() ?? "noHealth") + "@" + ((Vector2)t.position).ToString("F1"));
      s += $"[{rc.name} cleared={rc.Cleared} fighting={fighting} children={root.childCount} {string.Join(" ", kids)}] ";
    }
    var all = Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    s += " allEnemies=" + string.Join(" ", all.Select(e => e.name + "@" + ((Vector2)e.transform.position).ToString("F1") + " parent=" + (e.transform.parent ? e.transform.parent.name : "none") + " dead=" + e.GetComponent<Oathdeep.Combat.Health>().IsDead));
    return "player=" + ((Vector2)p.transform.position).ToString("F1") + " " + s;
  }
}
