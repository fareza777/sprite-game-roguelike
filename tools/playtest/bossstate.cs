using UnityEngine;
using System.Linq;
public class Script {
  public static string Main() {
    var rooms = Object.FindObjectsByType<Oathdeep.Dungeon.RoomController>(FindObjectsSortMode.None);
    var boss = rooms.FirstOrDefault(r => r.Type == Oathdeep.Dungeon.RoomType.Boss);
    var enemies = Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None);
    return "bossRoom cleared=" + (boss ? boss.Cleared.ToString() : "?") + " enemies=" + enemies.Length + " alive=" + enemies.Count(e => !e.GetComponent<Oathdeep.Combat.Health>().IsDead) + " t=" + Time.timeScale + " phase?";
  }
}
