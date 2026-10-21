public class Script {
  public static string Main() {
    var m = typeof(Oathdeep.EditorTools.OathdeepSetup).GetMethod("BuildAndroidNow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    m.Invoke(null, null);
    return System.IO.File.ReadAllText("Builds/Android/build-report.txt");
  }
}
