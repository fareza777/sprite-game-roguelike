using UnityEngine;
using Oathdeep.Core;
public class Script {
  // Starts a run as the hero named in %TEMP%/undersun_hero.txt (aldric, wren, morrow, brakka, sable), god mode on.
  public static string Main() {
    var id = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_hero.txt")).Trim();
    SaveSystem.Current.lastHero = id;
    SaveSystem.Current.onboardingDone = true;
    SaveSystem.Current.introSeen = true;
    var cine = Object.FindFirstObjectByType<Oathdeep.UI.CinematicView>();
    if (cine != null) typeof(Oathdeep.UI.CinematicView).GetMethod("Finish", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(cine, null);
    var runner = Object.FindFirstObjectByType<GameRunner>();
    if (runner == null) return "no runner";
    runner.StartRun();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    p.GetComponent<Oathdeep.Combat.Health>().PushInvulnerable();
    return "run as " + p.Hero + " (" + p.name + ")";
  }
}
