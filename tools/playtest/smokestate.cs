using UnityEngine;
using System.Linq;
public class Script {
  // One-line status for the floor smoke test: floor, biome, boss, portal and player state.
  public static string Main() {
    var hud = Object.FindFirstObjectByType<Oathdeep.UI.HudView>();
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    var idx = runner ? (int)typeof(Oathdeep.Core.GameRunner).GetField("floorIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(runner) : -1;
    var boss = Object.FindFirstObjectByType<Oathdeep.Enemies.PatternBossAI>() as Oathdeep.Enemies.EnemyBase ?? Object.FindFirstObjectByType<Oathdeep.Enemies.BoneWardenAI>();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    var portal = Object.FindFirstObjectByType<Oathdeep.Dungeon.ExitPortal>();
    return "floor=" + (idx + 1) + " boss=" + (boss ? boss.name : "-") + " portal=" + (portal != null) + " playerDead=" + (p ? p.IsDead.ToString() : "nop") + " t=" + Time.timeScale;
  }
}
