using UnityEditor;
public class Script { public static string Main() =>
  "target=" + EditorUserBuildSettings.activeBuildTarget + " compiling=" + EditorApplication.isCompiling + " updating=" + EditorApplication.isUpdating; }
