using UnityEngine;
public class Script {
  public static string Main() {
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    typeof(Oathdeep.Core.GameRunner).GetMethod("ToMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(runner, null);
    var page = Object.FindFirstObjectByType<Oathdeep.UI.CodexPage>(FindObjectsInactive.Include);
    page.Show();
    var sel = Oathdeep.Data.Bestiary.Get("cinder_imp");
    typeof(Oathdeep.UI.CodexPage).GetField("selected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(page, sel);
    typeof(Oathdeep.UI.CodexPage).GetMethod("Rebuild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(page, null);
    return "codex unlocked=" + Oathdeep.Core.SaveSystem.Current.codex.Count;
  }
}
