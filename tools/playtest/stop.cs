using UnityEditor;
public class Script { public static string Main() { if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); return "stopping"; } }
