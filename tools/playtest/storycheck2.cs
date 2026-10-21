using UnityEngine;
public class Script { public static string Main() {
  var r = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
  var st = (Oathdeep.Core.StoryAssets)typeof(Oathdeep.Core.GameRunner).GetField("story", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(r);
  var txt = st.script.text;
  var d = Oathdeep.Core.StoryData.Parse(txt);
  return "len=" + txt.Length + " first=" + (int)txt[0] + " parsedNow=" + d.prologue.Length + " floors=" + d.floors.Length;
} }
