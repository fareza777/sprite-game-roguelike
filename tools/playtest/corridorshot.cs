using UnityEngine;
using System.Linq;
using Oathdeep.Dungeon;
public class Script {
  // Puts the knight halfway down the corridor out of the start room and reports where the camera is.
  public static string Main() {
    PlayerInputReader_Stop();
    var b = Object.FindFirstObjectByType<DungeonBuilder>();
    var p = Object.FindFirstObjectByType<Oathdeep.Player.PlayerController>();
    var a = b.Geometry.Rooms[0];
    var span = a.Doors[0];
    Vector2 door = new Vector2(span.Cells.x + span.Cells.width * 0.5f, span.Cells.y + span.Cells.height * 0.5f);
    Vector2 mid = door + (Vector2)span.Direction * 8f;
    p.GetComponent<Rigidbody2D>().position = mid; p.transform.position = mid;
    return "player=" + mid + " cam=" + (Vector2)Camera.main.transform.position;
  }
  static void PlayerInputReader_Stop() => Oathdeep.Player.PlayerInputReader.SetTouchMove(Vector2.zero);
}
