using UnityEngine;
using Oathdeep.Combat;
using Oathdeep.Enemies;
public class Script {
  // Drops the live boss to the health fraction in %TEMP%/undersun_bosshp.txt (e.g. 0.6 for phase two) and reports its state.
  public static string Main() {
    var core = Object.FindFirstObjectByType<BossCore>();
    if (core == null) return "no boss";
    var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_bosshp.txt");
    if (System.IO.File.Exists(path)) {
      float f = float.Parse(System.IO.File.ReadAllText(path).Trim(), System.Globalization.CultureInfo.InvariantCulture);
      int want = Mathf.RoundToInt(core.Health.Max * f);
      if (core.Health.Current > want) {
        core.Health.DamageTakenMultiplier = 1f;
        core.Health.TryTakeDamage(new DamageInfo(core.Health.Current - want, false, core.transform.position, 0f, 0f, Team.Hazard));
      }
      System.IO.File.Delete(path);
    }
    var zones = Object.FindObjectsByType<HazardZone>(FindObjectsSortMode.None).Length;
    var anchors = Object.FindObjectsByType<BossAnchor>(FindObjectsSortMode.None).Length;
    var decoys = Object.FindObjectsByType<BossDecoy>(FindObjectsSortMode.None).Length;
    var beams = Object.FindObjectsByType<SweepBeam>(FindObjectsSortMode.None).Length;
    return $"{core.DisplayName} hp={core.Health.Current}/{core.Health.Max} phase={core.Phase} halted={core.Halted} staggered={core.Staggered} mult={core.Health.DamageTakenMultiplier:0.##} zones={zones} anchors={anchors} decoys={decoys} beams={beams}";
  }
}
