using UnityEngine;
using Oathdeep.Loot;
using Oathdeep.Player;
public class Script {
  // Gives the hero 10 rolled items for floor 4 (some lucky) and opens the inventory page.
  public static string Main() { try { return Run(); } catch (System.Exception e) { return e.ToString(); } }
  static string Run() {
    var p = Object.FindFirstObjectByType<PlayerController>();
    var inv = p.GetComponent<PlayerInventory>();
    var rng = new System.Random(7);
    string s = "";
    for (int i = 0; i < 10; i++) {
      var item = ItemGenerator.Roll(rng, 4, i * 0.4f);
      s += inv.Take(item) + ":" + item.Name + "(" + item.Rarity + ") ";
    }
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    typeof(Oathdeep.Core.GameRunner).GetMethod("OpenInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(runner, null);
    var page = Object.FindFirstObjectByType<Oathdeep.UI.InventoryPage>();
    typeof(Oathdeep.UI.InventoryPage).GetMethod("Select", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(page, new object[] { inv.Bag[0] });
    return s;
  }
}
