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
            MethodInfo arrowOffset = layout.GetMethod("QuantityArrowOffset", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo halfSlot = layout.GetMethod("QuantityHalfSlot", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (float digits in new[] {6f, 12f, 18f, 30f}) foreach (float arrow in new[] {4f, 9f, 20f}) foreach (float gap in new[] {0f, 13f, 40f})
            {
                float offset = (float)arrowOffset.Invoke(null, new object[] {digits, arrow, gap});
                Require(Math.Abs(offset - arrow / 2 - digits / 2 - gap) < .001f, "Arrow gap must start at the visible digit edge for one/two-digit counters");
                int reserve = (int)halfSlot.Invoke(null, new object[] {gap});
                Require(reserve >= offset + 10, "HUD names must reserve the arrow hitbox as well as the glyph");
            }
            MethodInfo centre = layout.GetMethod("QuantityCenter", BindingFlags.Static | BindingFlags.NonPublic);
            int centreDefault = (int)centre.Invoke(null, new object[] {240, 4, 0});
            int centreInset = (int)centre.Invoke(null, new object[] {240, 4, 12});
            Require(centreDefault == 201 && centreInset == 189, "Column movement must include the project quantity centre");
            MethodInfo numericFont = layout.GetMethod("NumericCraftFont", BindingFlags.Static | BindingFlags.NonPublic);
            Require((bool)numericFont.Invoke(null, new object[] {true, false, true, false}), "Quantity-only font must leave material numbers unchanged");
            Require(!(bool)numericFont.Invoke(null, new object[] {true, false, true, true}), "Quantity-only switch must affect quantity/arrows");
            Require(!(bool)numericFont.Invoke(null, new object[] {true, true, true, true}), "Combined font switches must not cancel each other");
            Require((bool)numericFont.Invoke(null, new object[] {false, true, false, false}), "Alternative numeric font must follow the selected body profile");
            MethodInfo alignedStarts = layout.GetMethod("RightAlignedStarts", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (int[] widths in new[] {new[] {48,65,72,140}, new[] {180,86,104,99,244}, new[] {86,104,99,244}, new[] {72,140}, new[] {140}})
            {
                int[] starts = (int[])alignedStarts.Invoke(null, new object[] {widths, -7, 10});
                Require(starts[widths.Length - 1] + widths[widths.Length - 1] == -7, "Localized row must retain its right edge");
                for (int i = 1; i < widths.Length; i++) Require(starts[i] - starts[i - 1] - widths[i - 1] == 10, "Action blocks must retain equal gaps across languages and omitted actions");
            }
            MethodInfo rowStarts = layout.GetMethod("ResultRowStarts", BindingFlags.Static | BindingFlags.NonPublic);
            Require(((int[])rowStarts.Invoke(null, new object[] {new int[0], 324, 0})).Length == 0, "A notice without items must stay on its first line");
            int[] wrapped = (int[])rowStarts.Invoke(null, new object[] {new[] {60,100,100,100}, 324, 0});
            Require(wrapped.Length == 2 && wrapped[0] == 0 && wrapped[1] == 3, "Results must wrap at the real measured width");
            int[] compact = (int[])rowStarts.Invoke(null, new object[] {new[] {40,35,44,53}, 180, -4});
            Require(compact.Length == 1, "Compact results must retain arbitrary count widths");
            Type invalidation = candidate.GetType("CraftingPlanner.PlannerInventoryRefresh", true);
            MethodInfo mark = invalidation.GetMethod("Mark", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo consume = invalidation.GetMethod("Consume", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo reset = invalidation.GetMethod("Reset", BindingFlags.Static | BindingFlags.NonPublic);
            reset.Invoke(null, null); Require(!(bool)consume.Invoke(null, null), "Idle frames must not request inventory work");
            mark.Invoke(null, null); mark.Invoke(null, null); mark.Invoke(null, null);
            Require((bool)consume.Invoke(null, null) && !(bool)consume.Invoke(null, null), "A transfer batch must request only one refresh");
            mark.Invoke(null, null); reset.Invoke(null, null); Require(!(bool)consume.Invoke(null, null), "Unload must clear pending transfer work");
            Type styleType = candidate.GetType("CraftingPlanner.PlannerStyle", true);
            MethodInfo baseline = styleType.GetMethod("BaselineValue", BindingFlags.Static | BindingFlags.NonPublic);
            Require((float)baseline.Invoke(null, new object[] {"MenuX"}) == 17 && (float)baseline.Invoke(null, new object[] {"FeedbackY"}) == -5, "Individual resets must use the returned candidate baseline");
            Require(Math.Abs((float)baseline.Invoke(null, new object[] {"Opacity"}) - 39) < .001f && Math.Abs((float)baseline.Invoke(null, new object[] {"FooterShadowOpacity"}) - 80) < .001f, "Percent controls must reset in display units");
            Require((float)baseline.Invoke(null, new object[] {"MenuCaptionFont"}) == 16 && (float)baseline.Invoke(null, new object[] {"ChestActionGap"}) == 10 && (float)baseline.Invoke(null, new object[] {"MenuActionGap"}) == 10, "Caption and action-row resets must use their own baselines");
            Require((float)baseline.Invoke(null, new object[] {"MenuSpendX"}) == 0 && (float)baseline.Invoke(null, new object[] {"MenuSpendY"}) == 0 && (float)baseline.Invoke(null, new object[] {"QuantityArrowGap"}) == 13, "Cost-block and actual arrow-gap resets must use new defaults");
            Require((float)baseline.Invoke(null, new object[] {"MenuMinusX"}) == 0 && (float)baseline.Invoke(null, new object[] {"MenuPlusX"}) == 0, "Old fixed-arrow offsets must not bias measured gaps");
            object style = Activator.CreateInstance(styleType, true);
            BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            string tuningRecord = (string)styleType.GetMethod("Record", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(style, null);
            Require(tuningRecord.Contains(" menu_spend_x=0") && tuningRecord.Contains(" menu_spend_y=0") && tuningRecord.Contains(" quantity_arrow_gap=13"), "Calibration export must retain cost offsets and measured arrow spacing");
            styleType.GetField("Right", fieldFlags).SetValue(style, 12.955f);
            styleType.GetField("IconX", fieldFlags).SetValue(style, -2.5f);
            styleType.GetField("Scale", fieldFlags).SetValue(style, .647f);
            styleType.GetField("FeedbackGap", fieldFlags).SetValue(style, -4.2f);
            MethodInfo clampStyle = styleType.GetMethod("Clamp", fieldFlags), recordStyle = styleType.GetMethod("Record", fieldFlags);
            clampStyle.Invoke(style, null);
            Require((float)styleType.GetField("Right", fieldFlags).GetValue(style) == 13 && (float)styleType.GetField("IconX", fieldFlags).GetValue(style) == -3, "Dimensions must snap consistently to whole UI units");
            Require(Math.Abs((float)styleType.GetField("Scale", fieldFlags).GetValue(style) - .65f) < .00001f, "Scale must use explicit hundredth steps");
            Require((float)styleType.GetField("FeedbackGap", fieldFlags).GetValue(style) == -4, "Negative item spacing must remain configurable in whole units");
            string snapped = (string)recordStyle.Invoke(style, null); clampStyle.Invoke(style, null);
            Require(snapped == (string)recordStyle.Invoke(style, null), "Repeated layout clamp must not drift");
            MethodInfo textRow = layout.GetMethod("TextRowHeight", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo minimumWidth = layout.GetMethod("MinimumWidth", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (float iconSize in new[] {8f, 29f, 80f})
            {
                styleType.GetField("Icon", fieldFlags).SetValue(style, iconSize); clampStyle.Invoke(style, null);
                Require((int)textRow.Invoke(null, new object[] {18,24,0}) == 28 && (int)minimumWidth.Invoke(null, new object[] {4,70,5}) == 266, "Visual icon size must not enlarge text rows or columns");
            }
            MethodInfo clampSelection = layout.GetMethod("ClampSelection", BindingFlags.Static | BindingFlags.NonPublic);
            int afterRemoval = (int)clampSelection.Invoke(null, new object[] {3, 3});
            Require(afterRemoval == 2 && (afterRemoval + 1) % 3 == 0, "Focus must normalize before navigating after last-row removal");
            Require((int)clampSelection.Invoke(null, new object[] {0, 0}) == 0, "Empty focus must be bounded");
            Type localization = candidate.GetType("CraftingPlanner.PlannerLocalization", true);
            IDictionary texts = (IDictionary)localization.GetField("Texts", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            string[] languages = {"en","de","fr","pt-br","es","ru","it","pl","ja","zh-cn","ko"};
            Require(texts.Count == languages.Length, "Host language coverage changed");
            Require(((string[])texts["en"])[0] == "Building Planner", "English HUD title must match the adopted product name");
            foreach (string language in languages)
            {
                string[] labels = (string[])texts[language];
                Require(labels != null && labels.Length == 8, "Missing card labels: " + language);
                foreach (string label in labels) Require(!string.IsNullOrEmpty(label), "Empty localized label");
            }
            IDictionary editTexts = (IDictionary)localization.GetField("EditTexts", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Require(editTexts.Count == languages.Length, "Editing language coverage changed");
            Require(((string[])editTexts["en"])[2] == "Add to planner", "Supported menu must use the adopted caption");
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
        // Project arithmetic fixture: native Item constructors load Unity resources/bag definitions.
        object ingredient = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsField.FieldType.GetGenericArguments()[0]);
        Field(ingredient, "id", "test_material"); Field(ingredient, "value", 4); needs.Add(ingredient);
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
        Field(goal, "Quantity", 19); Field(goal, "Repeatable", true); goals.Add(goal);
        Type state = candidate.GetType("CraftingPlanner.PlannerGoalState", true);
        MethodInfo menuAdjust = state.GetMethod("Adjust", BindingFlags.Static | BindingFlags.NonPublic);
        Require((bool)menuAdjust.Invoke(null, new object[] {goals, definition, "1|original_builder|test_project", true, 1, null}), "19 must increase to20 through the menu path");
        for (int i = 0; i < 25; i++)
        {
            Require(!(bool)adjust.Invoke(null, new object[] {goals, goal, 1, null}), "HUD repeat input must stop at20");
            Require(!(bool)menuAdjust.Invoke(null, new object[] {goals, definition, "1|original_builder|test_project", true, int.MaxValue, null}), "Menu input must not bypass20");
        }
        Require((int)goalType.GetField("Quantity", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(goal) == 20 && goals.Count == 1, "Cap must neither overflow nor duplicate the goal");
        aggregate = (IDictionary)planner.GetMethod("AggregateRequirements", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        Require((int)aggregate["test_material"] == 80, "No-op increases at20 must retain exact aggregate materials");
        Require((bool)adjust.Invoke(null, new object[] {goals, goal, -1, null}), "20 must still decrement");
        Require((bool)adjust.Invoke(null, new object[] {goals, goal, 1, null}), "19 must return to20 through the HUD path");
        for (int i = 0; i < 20; i++) adjust.Invoke(null, new object[] {goals, goal, -1, null});
        Require(goals.Count == 0, "A capped goal must still remove at zero");
        Console.WriteLine("PASS: stored-goal identity, aggregation, zero removal, stale callback, one-off bound and shared menu/HUD cap20.");
    }
}
