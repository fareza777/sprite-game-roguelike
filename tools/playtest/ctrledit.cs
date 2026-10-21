using UnityEngine;
using Oathdeep.UI;
public class Script {
  // %TEMP%/undersun_ctrledit.txt: "open" shows the layout editor; "place" drags the controls to a test arrangement
  // (stick bottom-right, attack bottom-left, dodge and skill above them) and saves; "reset" restores the default.
  public static string Main() {
    var mode = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_ctrledit.txt")).Trim();
    if (mode == "reset") { Oathdeep.Core.GameSettings.ResetControlPositions(); return "reset"; }
    var editor = Object.FindFirstObjectByType<ControlLayoutEditor>(FindObjectsInactive.Include);
    if (editor == null) return "no editor";
    editor.Open(null);
    if (mode == "open") return "editor open";
    var board = (RectTransform)editor.transform;
    var size = board.rect.size;
    void Put(string name, float x, float y) { var h = (RectTransform)board.Find(name); h.anchorMin = h.anchorMax = new Vector2(x, y); h.anchoredPosition = Vector2.zero; }
    Put("Stick", 0.72f, 0.18f);
    Put("Attack", 0.22f, 0.14f);
    Put("Dodge", 0.42f, 0.08f);
    Put("Skill", 0.2f, 0.32f);
    typeof(ControlLayoutEditor).GetMethod("Save", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(editor, null);
    var p = Oathdeep.Core.GameSettings.ControlPositions;
    return "saved custom=" + Oathdeep.Core.GameSettings.CustomControls + " stick=" + p[0] + " attack=" + p[1];
  }
}
