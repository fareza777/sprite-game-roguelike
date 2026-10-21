using UnityEngine;
using System.Linq;
public class Script {
  public static string Main() {
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    string s = "player=" + (p ? p.transform.position.ToString() + " active=" + p.gameObject.activeInHierarchy + " sr=" + p.GetComponentInChildren<SpriteRenderer>().enabled + " sprite=" + p.GetComponentInChildren<Animator>().GetComponent<SpriteRenderer>().sprite : "none");
    s += " cam=" + Camera.main.transform.position;
    foreach (var e in Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None).Take(10)) {
      var sr = e.GetComponentInChildren<Animator>() ? e.GetComponentInChildren<Animator>().GetComponent<SpriteRenderer>() : null;
      s += " | " + e.name + "@" + (Vector2)e.transform.position + " spr=" + (sr ? (sr.sprite ? sr.sprite.name : "NULL") + " en=" + sr.enabled + " col=" + sr.color : "noSR");
    }
    return s;
  }
}
