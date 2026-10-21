public class Script {
  public static string Main() {
    string s = "";
    foreach (var n in new[] { "Oathdeep.UI.CodexPage", "Oathdeep.Enemies.PatternBossAI", "Oathdeep.Data.Bestiary" })
      s += n.Substring(n.LastIndexOf('.') + 1) + "=" + (System.Type.GetType(n + ", Oathdeep.Runtime") != null) + " ";
    return s + " compiling=" + UnityEditor.EditorApplication.isCompiling;
  }
}
