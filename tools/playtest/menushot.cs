using UnityEngine;
public class Script {
  // Dismisses splash/cinematic and shows the main menu (or the Quests page if %TEMP%/undersun_page.txt says "quests").
  public static string Main() {
    var cine = Object.FindFirstObjectByType<Oathdeep.UI.CinematicView>();
    if (cine != null) typeof(Oathdeep.UI.CinematicView).GetMethod("Finish", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(cine, null);
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    typeof(Oathdeep.Core.GameRunner).GetMethod("ToMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(runner, null);
    var f = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_page.txt");
    if (System.IO.File.Exists(f) && System.IO.File.ReadAllText(f).Trim() == "quests") {
      var q = Object.FindFirstObjectByType<Oathdeep.UI.QuestPage>(FindObjectsInactive.Include);
      typeof(Oathdeep.UI.QuestPage).GetField("sideTab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(q, true);
      q.Show();
      return "quests";
    }
    return "menu";
  }
}
