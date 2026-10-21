using UnityEngine;
public class Script {
  // Dismisses any cinematic/splash and opens the Heroes page focused on the hero named in %TEMP%/undersun_hero.txt.
  public static string Main() {
    var cine = Object.FindFirstObjectByType<Oathdeep.UI.CinematicView>();
    if (cine != null) typeof(Oathdeep.UI.CinematicView).GetMethod("Finish", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(cine, null);
    var page = Object.FindFirstObjectByType<Oathdeep.UI.HeroesPage>(FindObjectsInactive.Include);
    if (page == null) return "no heroes page";
    page.Show();
    var id = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "undersun_hero.txt")).Trim();
    var heroes = (System.Collections.Generic.IReadOnlyList<Oathdeep.Data.HeroData>)typeof(Oathdeep.UI.HeroesPage).GetField("heroes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(page);
    for (int i = 0; i < heroes.Count; i++) if (Oathdeep.UI.HeroesPage.IdOf(heroes[i]) == id) {
      typeof(Oathdeep.UI.HeroesPage).GetField("focus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(page, i);
      typeof(Oathdeep.UI.HeroesPage).GetMethod("Refresh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(page, null);
    }
    return "heroes page open on " + id;
  }
}
