using UnityEngine;
public class Script { public static string Main() {
  var r = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
  var st = (Oathdeep.Core.StoryAssets)typeof(Oathdeep.Core.GameRunner).GetField("story", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(r);
  if (st == null) return "story NULL";
  return "script=" + (st.script != null) + " prologue=" + st.Data.prologue.Length + " panels=" + st.panels.Length + " voices=" + st.voices.Length + " pro1Panel=" + (st.PanelFor("pro_1") != null) + " pro1Voice=" + (st.VoiceFor("pro_1") != null);
} }
