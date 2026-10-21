using UnityEditor;
using UnityEditor.SceneManagement;
public class Script {
  public static string Main() {
    if (EditorApplication.isPlaying) return "already playing";
    EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");
    EditorApplication.EnterPlaymode();
    return "entering play mode";
  }
}
