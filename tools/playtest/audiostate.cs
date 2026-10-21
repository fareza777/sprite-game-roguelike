using UnityEngine;
public class Script { public static string Main() { var go = GameObject.Find("Audio"); string s = ""; foreach (var a in go.GetComponents<AudioSource>()) if (a.isPlaying) s += (a.clip ? a.clip.name : "oneshot") + "@" + a.volume.ToString("0.00") + " "; return s; } }
