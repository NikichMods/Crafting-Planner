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
            Type layout = candidate.GetType("CraftingPlanner.PlannerLayout", true);
            MethodInfo fade = layout.GetMethod("BackingAlpha", BindingFlags.Static | BindingFlags.NonPublic);
            Func<float, float, float> value = delegate(float x, float y) { return (float)fade.Invoke(null, new object[] {x, y, 240f, 200f, 24f, 13f}); };
            Require(value(120, 100) == 1, "Body centre must retain the configured density");
            Require(value(120, 15) > .95f, "Top goal must have backing, not only the middle of the list");
            Require(value(-14, 100) == 0 && value(254, 100) == 0 && value(120, -14) == 0 && value(120, 214) == 0, "All exterior edges must vanish");
            Require(value(0, 0) < value(120, 0), "Corners must be rounded");
            Require(value(0, 100) > 0 && value(0, 100) < 1, "Edges must be feathered");
            for (int y = -20; y <= 220; y += 10) for (int x = -20; x <= 260; x += 10)
            {
                float current = value(x, y);
                Require(current >= 0 && current <= 1, "Invalid opacity");
                Require(Math.Abs(current - value(240 - x, y)) < .00001f && Math.Abs(current - value(x, 200 - y)) < .00001f, "Backing is off-centre");
                if (x < 120) Require(current <= value(x + 10, y), "Backing weakens toward centre");
            }
            MethodInfo quantityX = layout.GetMethod("QuantityX", BindingFlags.Static | BindingFlags.NonPublic);
            Require((int)quantityX.Invoke(null, new object[] {100}) == 120, "Quantity must reserve its left arrow slot");
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
            IDictionary editTexts = (IDictionary)localization.GetField("EditTexts", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Require(editTexts.Count == languages.Length, "Editing language coverage changed");
            foreach (string language in languages)
            {
                string[] labels = (string[])editTexts[language];
                Require(labels != null && labels.Length == 3, "Missing editing labels: " + language);
                foreach (string label in labels) Require(!string.IsNullOrEmpty(label), "Empty editing label");
            }
            IDictionary transferTexts = (IDictionary)localization.GetField("TransferTexts", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Require(transferTexts.Count == languages.Length, "Transfer feedback language coverage changed");
            foreach (string language in languages)
            {
                string[] labels = (string[])transferTexts[language];
                Require(labels != null && labels.Length == 5, "Missing transfer feedback: " + language);
                foreach (string label in labels) Require(!string.IsNullOrEmpty(label), "Empty transfer feedback");
            }
            MethodInfo normalize = localization.GetMethod("Normalize", BindingFlags.Static | BindingFlags.NonPublic);
            Require((string)normalize.Invoke(null, new object[] {"zh_CN"}) == "zh-cn", "Native Chinese code not normalized");
            Require((string)normalize.Invoke(null, new object[] {"pt_BR"}) == "pt-br", "Native Portuguese code not normalized");
            if (Array.IndexOf(args, "--state") >= 0) CheckPinnedState(candidate);
            Console.WriteLine("PASS: centred rounded backing, feather/bounds/top-row coverage, quantity slot and 11-language schema. Native visual and input acceptance require the game.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void Field(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(target, value);
    }
    static void CheckPinnedState(Assembly candidate)
    {
        Type planner = candidate.GetType("CraftingPlanner.Planner", true);
        Type goalType = candidate.GetType("CraftingPlanner.ProjectGoal", true);
        IList goals = (IList)planner.GetField("Goals", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        goals.Clear();
        Type definitionType = goalType.GetField("Definition", BindingFlags.Instance | BindingFlags.NonPublic).FieldType;
        object definition = Activator.CreateInstance(definitionType);
        Field(definition, "id", "test_project");
        var needsField = definitionType.GetField("needs");
        IList needs = (IList)Activator.CreateInstance(needsField.FieldType);
        needs.Add(Activator.CreateInstance(needsField.FieldType.GetGenericArguments()[0], new object[] { "test_material", 4 }));
        needsField.SetValue(definition, needs);
        object goal = Activator.CreateInstance(goalType, true);
        Field(goal, "Key", "1|original_builder|test_project"); Field(goal, "Definition", definition);
        Field(goal, "Quantity", 2); Field(goal, "Repeatable", true); goals.Add(goal);
        MethodInfo adjust = candidate.GetType("CraftingPlanner.PlannerGoalState", true).GetMethod("AdjustPinned", BindingFlags.Static | BindingFlags.NonPublic);
        Require((bool)adjust.Invoke(null, new object[] {goals, goal, 1, null}), "HUD adjustment rejected");
        Require(goals.Count == 1 && ReferenceEquals(goals[0], goal), "Editing outside its menu duplicated the original goal");
        Require((int)goalType.GetField("Quantity", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(goal) == 3, "Wrong edited quantity");
        IDictionary aggregate = (IDictionary)planner.GetMethod("AggregateRequirements", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        Require((int)aggregate["test_material"] == 12, "Aggregate did not follow HUD editing");
        for (int i = 0; i < 3; i++) adjust.Invoke(null, new object[] {goals, goal, -1, null});
        Require(goals.Count == 0, "Zero goal was not removed");
        Require(!(bool)adjust.Invoke(null, new object[] {goals, goal, 1, null}), "Stale HUD callback recreated a removed goal");
        Field(goal, "Quantity", 1); Field(goal, "Repeatable", false); goals.Add(goal);
        adjust.Invoke(null, new object[] {goals, goal, 1, null});
        Require((int)goalType.GetField("Quantity", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(goal) == 1, "One-off goal multiplied");
        goals.Clear();
        Console.WriteLine("PASS: stored-goal identity outside its original context, aggregation, zero removal, stale callback and one-off bound.");
    }
}
