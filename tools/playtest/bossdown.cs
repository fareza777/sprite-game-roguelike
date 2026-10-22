using UnityEngine;
using Oathdeep.Combat;
public class Script {
  // Closes run screens, resumes, then kills every enemy including the boss (call after tpboss + a few pumps).
  public static string Main() {
    var fe = Object.FindFirstObjectByType<Oathdeep.UI.FrontEnd>();
    fe.HideRunScreens();
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    runner.Resume();
    int n = 0;
    // Shields first, as a player would: break every anchor (totems, root cores, dusk anchors, nests).
    foreach (var a in Object.FindObjectsByType<Oathdeep.Enemies.BossAnchor>(FindObjectsSortMode.None)) {
      var ah = a.GetComponent<Health>();
      if (ah != null && !ah.IsDead) ah.TryTakeDamage(new DamageInfo(99999, false, a.transform.position, 0f, 0f, Team.Player));
    }
    foreach (var e in Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None)) {
      var h = e.GetComponent<Health>();
      if (h.IsDead) continue;
      if (h.DamageTakenMultiplier <= 0f) h.DamageTakenMultiplier = 1f;   // the shield drops once the anchors break
      if (h.TryTakeDamage(new DamageInfo(99999, false, e.transform.position, 0f, 0f, Team.Player))) n++;
    }
    return "killed " + n + " timeScale=" + Time.timeScale;
  }
}
