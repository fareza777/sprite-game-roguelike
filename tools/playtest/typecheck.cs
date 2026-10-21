public class Script {
  public static string Main() {
    string s = "";
    foreach (var n in new[] { "Oathdeep.UI.FrontEnd", "Oathdeep.UI.MainMenuView", "Oathdeep.UI.ControlLayout", "Oathdeep.Core.SaveSystem", "Oathdeep.UI.OnboardingCoach" })
      s += n.Substring(n.LastIndexOf('.') + 1) + "=" + (System.Type.GetType(n + ", Oathdeep.Runtime") != null) + " ";
    return s;
  }
}
