using UnityEngine;
using Oathdeep.Combat;
public class Script {
  public static string Main() {
    int n = 0;
    foreach (var e in Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None)) {
      var h = e.GetComponent<Health>();
      if (h.IsDead || e.GetComponent<Oathdeep.Enemies.BoneWardenAI>() != null) continue;
      if (h.TryTakeDamage(new DamageInfo(9999, false, e.transform.position, 0f, 0f, Team.Player))) n++;
    }
    return "killed " + n;
  }
}
