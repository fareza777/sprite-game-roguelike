using UnityEngine;
public class Script {
  public static string Main() {
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    string s = "";
    foreach (var sr in p.GetComponentsInChildren<SpriteRenderer>(true))
      s += sr.name + " en=" + sr.enabled + " act=" + sr.gameObject.activeInHierarchy + " col=" + sr.color + " mat=" + (sr.sharedMaterial ? sr.sharedMaterial.name : "null") + " order=" + sr.sortingOrder + " layer=" + sr.sortingLayerName + " | ";
    s += " pos=" + p.transform.position + " scale=" + p.transform.localScale + " cam=" + Camera.main.transform.position + " uionly=" + System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_uionly.txt"));
    var boss = Object.FindFirstObjectByType<Oathdeep.Enemies.PatternBossAI>();
    if (boss) s += " boss=" + boss.transform.position;
    return s;
  }
}
