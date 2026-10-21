using UnityEngine;
using System.Linq;
public class Script {
  public static string Main() {
    int id = PlayerPrefs.GetInt("pt.stranded");
    var e = Object.FindObjectsByType<Oathdeep.Enemies.EnemyBase>(FindObjectsSortMode.None).FirstOrDefault(x => x.GetInstanceID() == id);
    return e == null ? "gone" : e.name + " now at " + (Vector2)e.transform.position;
  }
}
