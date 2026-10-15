// SPDX-License-Identifier: MPL-2.0
// Executes project-owned policy on the compiled candidate; it does not emulate NGUI.
using System;
using System.IO;
using System.Reflection;
using System.Collections;
class CardTests
{
    static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    static int Main(string[] args)
    {
        try
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs e)
            {
                foreach (string folder in args)
                {
                    if (!Directory.Exists(folder)) continue;
                    string path = Path.Combine(folder, new AssemblyName(e.Name).Name + ".dll");
                    if (File.Exists(path)) return Assembly.LoadFrom(path);
                }
                return null;
            };
            Assembly candidate = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            Type card = candidate.GetType("CraftingPlanner.PlannerCard", true);
            MethodInfo fade = card.GetMethod("BackingStrength", BindingFlags.Static | BindingFlags.NonPublic);
            Func<float, float, float> value = delegate(float x, float y) { return (float)fade.Invoke(null, new object[] {x, y}); };
            Require(value(0, 0) == 0, "Bottom-left must vanish");
            Require(value(1, 1) == 1, "Top-right must retain backing opacity");
            Require(value(0, 1) > 0 && value(0, 1) < 1 && value(1, 0) > 0 && value(1, 0) < 1, "Other corners must fade");
            for (int y = 0; y <= 10; y++) for (int x = 0; x <= 10; x++)
            {
                float current = value(x / 10f, y / 10f);
                Require(current >= 0 && current <= 1, "Invalid opacity");
                if (x < 10) Require(current <= value((x + 1) / 10f, y / 10f), "Gradient reverses horizontally");
                if (y < 10) Require(current <= value(x / 10f, (y + 1) / 10f), "Gradient reverses vertically");
            }
            Type localization = candidate.GetType("CraftingPlanner.PlannerLocalization", true);
            IDictionary texts = (IDictionary)localization.GetField("Texts", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            string[] languages = {"en","de","fr","pt-br","es","ru","it","pl","ja","zh-cn","ko"};
            Require(texts.Count == languages.Length, "Host language coverage changed");
            foreach (string language in languages)
            {
                string[] labels = (string[])texts[language];
                Require(labels != null && labels.Length == 8, "Missing card labels: " + language);
                foreach (string label in labels) Require(!string.IsNullOrEmpty(label), "Empty localized label");
            }
            MethodInfo normalize = localization.GetMethod("Normalize", BindingFlags.Static | BindingFlags.NonPublic);
            Require((string)normalize.Invoke(null, new object[] {"zh_CN"}) == "zh-cn", "Native Chinese code not normalized");
            Require((string)normalize.Invoke(null, new object[] {"pt_BR"}) == "pt-br", "Native Portuguese code not normalized");
            Console.WriteLine("PASS: candidate gradient direction/bounds and complete 11-language label schema. Visual, font and gamepad acceptance require the game.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
