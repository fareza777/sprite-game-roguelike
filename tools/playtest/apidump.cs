using System;
using System.Linq;
using System.Reflection;
public class Script {
  // Public API of a few Google Mobile Ads types (precompiled, so read by reflection).
  public static string Main() {
    var names = new[] { "GoogleMobileAds.Api.MobileAds", "GoogleMobileAds.Api.BannerView", "GoogleMobileAds.Api.AdSize", "GoogleMobileAds.Api.InterstitialAd",
      "GoogleMobileAds.Api.RewardedAd", "GoogleMobileAds.Api.AdRequest", "GoogleMobileAds.Api.RequestConfiguration",
      "GoogleMobileAds.Ump.Api.ConsentInformation", "GoogleMobileAds.Ump.Api.ConsentForm", "GoogleMobileAds.Ump.Api.ConsentRequestParameters" };
    var sb = new System.Text.StringBuilder();
    foreach (var n in names) {
      var t = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(n)).FirstOrDefault(x => x != null);
      if (t == null) { sb.AppendLine(n + ": MISSING"); continue; }
      sb.AppendLine("== " + n);
      foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
        if (m is MethodInfo mi && mi.IsSpecialName) continue;
        string s = m is MethodInfo mm ? (mm.IsStatic ? "static " : "") + mm.ReturnType.Name + " " + mm.Name + "(" + string.Join(", ", mm.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")"
          : m is PropertyInfo pi ? "prop " + pi.PropertyType.Name + " " + pi.Name
          : m is EventInfo ei ? "event " + ei.EventHandlerType.Name + " " + ei.Name
          : m is ConstructorInfo ci ? "ctor(" + string.Join(", ", ci.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")"
          : m.MemberType + " " + m.Name;
        sb.AppendLine("  " + s);
      }
    }
    System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gma_api.txt"), sb.ToString());
    return "written";
  }
}
