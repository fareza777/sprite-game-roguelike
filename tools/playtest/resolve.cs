using UnityEditor.PackageManager;
public class Script {
  public static string Main() { Client.Resolve(); UnityEditor.AssetDatabase.Refresh(); return "resolving packages"; }
}
