using UnityEngine;
public class Script {
  public static string Main() {
    var fe = Object.FindFirstObjectByType<Oathdeep.UI.FrontEnd>();
    typeof(Oathdeep.UI.FrontEnd).GetMethod("OpenSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(fe, null);
    return "settings opened";
  }
}
