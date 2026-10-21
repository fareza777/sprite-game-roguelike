public class Script {
  public static string Main() {
    try { return Oathdeep.EditorTools.OathdeepSetup.BuildEverything(); }
    catch (System.Exception e) { return "FAILED: " + e; }
  }
}
