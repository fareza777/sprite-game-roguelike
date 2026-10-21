using UnityEngine;
public class Script {
  public static string Main() {
    var cine = Object.FindFirstObjectByType<Oathdeep.UI.CinematicView>();
    var save = Oathdeep.Core.SaveSystem.Current;
    return "cinematic=" + (cine != null && cine.gameObject.activeInHierarchy) + " victories=" + save.victories + " codex=" + save.codex.Count + " best=" + save.bestFloor;
  }
}
