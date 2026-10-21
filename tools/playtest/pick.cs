using UnityEngine;
using UnityEngine.UI;
public class Script {
  // Clicks answer N (from %TEMP%/undersun_pick.txt, default 0) of the open encounter dialog.
  public static string Main() {
    var view = Object.FindFirstObjectByType<Oathdeep.UI.EncounterDialogView>();
    if (view == null) return "no dialog open";
    var f = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_pick.txt");
    int n = System.IO.File.Exists(f) ? int.Parse(System.IO.File.ReadAllText(f).Trim()) : 0;
    var buttons = view.GetComponentsInChildren<Button>();
    if (n >= buttons.Length) return "only " + buttons.Length + " answers";
    var label = buttons[n].GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
    buttons[n].onClick.Invoke();
    return "picked " + label;
  }
}
