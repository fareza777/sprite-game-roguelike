using UnityEngine;
public class Script {
  // Jumps the current run to the floor number in %TEMP%/undersun_floor.txt (1..10).
  public static string Main() {
    int n = int.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_floor.txt")).Trim());
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    typeof(Oathdeep.Core.GameRunner).GetMethod("EnterFloor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(runner, new object[] { n - 1 });
    return "now on floor " + n;
  }
}
