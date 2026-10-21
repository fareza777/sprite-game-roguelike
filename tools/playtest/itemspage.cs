using UnityEngine;
public class Script {
  // Back to the main menu and open the Item Compendium with the first found item selected.
  public static string Main() {
    var runner = Object.FindFirstObjectByType<Oathdeep.Core.GameRunner>();
    typeof(Oathdeep.Core.GameRunner).GetMethod("ToMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(runner, null);
    var page = Object.FindFirstObjectByType<Oathdeep.UI.ItemCompendiumPage>(FindObjectsInactive.Include);
    page.Show();
    var found = Oathdeep.Core.SaveSystem.Current.items;
    var pick = Oathdeep.Loot.ItemCatalog.Get(found.Count > 3 ? found[3] : "rusted_blade");
    typeof(Oathdeep.UI.ItemCompendiumPage).GetField("selected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(page, pick);
    typeof(Oathdeep.UI.ItemCompendiumPage).GetMethod("Rebuild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(page, null);
    return "found " + found.Count + ", showing " + pick.Name;
  }
}
