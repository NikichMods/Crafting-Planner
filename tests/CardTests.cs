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
            Require((float)baseline.Invoke(null, new object[] {"MenuSpendX"}) == 24 && (float)baseline.Invoke(null, new object[] {"MenuSpendY"}) == -1 && (float)baseline.Invoke(null, new object[] {"QuantityArrowGap"}) == 13, "Cost-block resets must retain returned settings");
            Require((float)baseline.Invoke(null, new object[] {"MenuY"}) == 1 && (float)baseline.Invoke(null, new object[] {"MenuMinusX"}) == 8 && (float)baseline.Invoke(null, new object[] {"MenuPlusX"}) == -9, "Counter resets must retain the first returned calibration");
            object style = Activator.CreateInstance(styleType, true);
            BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            string tuningRecord = (string)styleType.GetMethod("Record", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(style, null);
            Require(tuningRecord.Contains(" menu_spend_x=24") && tuningRecord.Contains(" menu_spend_y=-1") && tuningRecord.Contains(" quantity_arrow_gap=13"), "Calibration export must retain cost offsets and measured arrow spacing");
            Require(tuningRecord.Contains(" ingredients_x=29") && tuningRecord.Contains(" cost_ingredients_x=46"), "Export must preserve both independently recovered material profiles");
            MethodInfo ingredientX = styleType.GetMethod("IngredientX", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo ingredientY = styleType.GetMethod("IngredientY", BindingFlags.Instance | BindingFlags.NonPublic);
            styleType.GetField("CostIngredientsX", fieldFlags).SetValue(style, 53f); styleType.GetField("CostIngredientsY", fieldFlags).SetValue(style, -8f);
            Require((float)ingredientX.Invoke(style, new object[] {false}) == 29 && (float)ingredientY.Invoke(style, new object[] {false}) == 0, "Repair-cost tuning must not move ordinary builder materials");
            styleType.GetField("IngredientsX", fieldFlags).SetValue(style, 11f); styleType.GetField("IngredientsY", fieldFlags).SetValue(style, 5f);
            Require((float)ingredientX.Invoke(style, new object[] {true}) == 53 && (float)ingredientY.Invoke(style, new object[] {true}) == -8, "Ordinary builder tuning must not move cost-row materials");
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
            if (Array.IndexOf(args, "--state") >= 0) { CheckPinnedState(candidate); CheckPlacement(candidate); CheckWorldCompletion(candidate); }
            if (Array.IndexOf(args, "--native-rows") >= 0) CheckAbsentRows(candidate);
            Console.WriteLine("PASS: centred rounded backing, feather/bounds/top-row coverage, quantity slot and 11-language schema. Native visual and input acceptance require the game.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void CheckAbsentRows(Assembly candidate)
    {
        var patch = candidate.GetType("CraftingPlanner.PlannerRowRedrawPatch", true);
        var prefix = patch.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
        prefix.Invoke(null, new object[] {null});
        // Real Unity MonoBehaviour metadata, no constructor or native GameObject allocation.
        object detached = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(prefix.GetParameters()[0].ParameterType);
        prefix.Invoke(null, new[] {detached});
        Console.WriteLine("PASS: actual redraw prefix accepts CLR-null and detached Unity row without GetComponent; live Max Buttons input remains a game check.");
    }
    static void CheckWorldCompletion(Assembly candidate)
    {
        const BindingFlags methods = BindingFlags.Static | BindingFlags.NonPublic;
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        Type planner=candidate.GetType("CraftingPlanner.Planner",true), goalType=candidate.GetType("CraftingPlanner.ProjectGoal",true);
        Type state=candidate.GetType("CraftingPlanner.PlannerGoalState",true), receiptType=candidate.GetType("CraftingPlanner.WorldCompletionAttempt",true);
        IList goals=(IList)planner.GetField("Goals",methods).GetValue(null); goals.Clear();
        MethodInfo find=state.GetMethod("FindWorldCompletion",methods), aggregate=planner.GetMethod("AggregateRequirements",methods);
        Type definitionType=find.GetParameters()[1].ParameterType;
        object definition=Activator.CreateInstance(definitionType); Field(definition,"id","repair"); Field(definition,"change_wgo","repaired");
        Type needsType=definitionType.GetField("needs").FieldType;
        IList needs=(IList)Activator.CreateInstance(needsType);
        object item=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]);
        Field(item,"id","completion_material"); Field(item,"value",4); needs.Add(item); Field(definition,"needs",needs);
        Func<string,bool,object> goal=delegate(string key,bool repeatable) {object result=Activator.CreateInstance(goalType,true); Field(result,"Key",key); Field(result,"Definition",definition); Field(result,"Quantity",1); Field(result,"Repeatable",repeatable); return result;};
        object tracked=goal("2|broken_owner|repair",false), otherOwner=goal("2|other_owner|repair",false), otherRecipe=goal("2|broken_owner|other",false), builder=goal("1|broken_owner|repair",false);
        goals.Add(tracked); goals.Add(otherOwner); goals.Add(otherRecipe); goals.Add(builder);
        Require(ReferenceEquals(find.Invoke(null,new[]{(object)goals,definition,"broken_owner"}),tracked),"World completion must match original owner/definition/context");
        Require(find.Invoke(null,new[]{(object)goals,definition,"repaired_owner"})==null,"Replacement owner must not be substituted into the original key");
        Field(tracked,"Repeatable",true); Require(find.Invoke(null,new[]{(object)goals,definition,"broken_owner"})==null,"World completion excludes repeatable goals"); Field(tracked,"Repeatable",false);
        Field(definition,"change_wgo",null); Require(find.Invoke(null,new[]{(object)goals,definition,"broken_owner"})==null,"Ordinary production is excluded"); Field(definition,"change_wgo","0");
        object objectCraft=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(state.GetMethod("FindPlacement",methods).GetParameters()[1].ParameterType);
        Field(objectCraft,"id","repair"); Field(objectCraft,"needs",needs); Field(objectCraft,"change_wgo","repaired"); Field(objectCraft,"one_time_craft",true);
        Require(find.Invoke(null,new[]{(object)goals,objectCraft,"broken_owner"})==null,"Builder upgrades must remain excluded");
        var observe=receiptType.GetMethod("ObserveCompletion",fields); var complete=receiptType.GetMethod("Complete",fields);
        Func<object,object> receipt=captured=>{object r=Activator.CreateInstance(receiptType,true);Field(r,"Goal",captured);Field(r,"Definition",definition);return r;};
        Func<object,bool,bool> finish=(r,normal)=>(bool)complete.Invoke(r,new object[]{goals,normal,null});
        foreach(int failure in new[]{0,1,2,3,4})
        {
            object r=receipt(tracked);
            if(failure==4) observe.Invoke(r,new[]{Activator.CreateInstance(definitionType),(object)true});
            else if(failure!=0) observe.Invoke(r,new[]{definition,(object)(failure!=1)});
            if(failure==2) observe.Invoke(r,new[]{definition,(object)true});
            Require(!finish(r,failure!=3) && goals.Contains(tracked),"Absent/failed/duplicate/wrong marker or native exception must preserve goal");
        }
        int before=(int)((IDictionary)aggregate.Invoke(null,null))["completion_material"];
        object success=receipt(tracked); observe.Invoke(success,new[]{definition,(object)true});
        Require(finish(success,true) && !goals.Contains(tracked),"Successful clearing/repair must remove the original one-off goal at zero");
        Require(!finish(success,true),"Completion receipt must be consumed once");
        Require(goals.Contains(otherOwner) && goals.Contains(otherRecipe) && goals.Contains(builder),"Unrelated owner/recipe/builder goals changed");
        Require((int)((IDictionary)aggregate.Invoke(null,null))["completion_material"]==before-4,"Completion must update derived exact material requirements");
        object stale=receipt(otherOwner); observe.Invoke(stale,new[]{definition,(object)true}); goals.Remove(otherOwner);
        object replacement=goal("2|other_owner|repair",false); goals.Add(replacement);
        Require(!finish(stale,true) && goals.Contains(replacement),"Removed/re-added goal must not inherit an old completion receipt");
        goals.Clear();
        Console.WriteLine("PASS: original world key, upgrade/production exclusion, exact completion marker, failures, once-only removal, unrelated goals and derived requirements.");
    }
    static void CheckPlacement(Assembly candidate)
    {
        BindingFlags methods = BindingFlags.Static | BindingFlags.NonPublic;
        Type planner = candidate.GetType("CraftingPlanner.Planner", true), state = candidate.GetType("CraftingPlanner.PlannerGoalState", true);
        Type goalType = candidate.GetType("CraftingPlanner.ProjectGoal", true), receiptType = candidate.GetType("CraftingPlanner.PlacementAttempt", true);
        IList goals = (IList)planner.GetField("Goals", methods).GetValue(null); goals.Clear();
        MethodInfo find = state.GetMethod("FindPlacement", methods), aggregate = planner.GetMethod("AggregateRequirements", methods);
        Type definitionType = find.GetParameters()[1].ParameterType;
        object definition = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(definitionType);
        Field(definition, "id", "bench");
        Field(definition, "build_type", Enum.Parse(definitionType.GetField("build_type").FieldType, "Put"));
        Type needsType = definitionType.GetField("needs").FieldType;
        IList needs = (IList)Activator.CreateInstance(needsType);
        object material = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]);
        Field(material, "id", "placement_material"); Field(material, "value", 4); needs.Add(material); Field(definition, "needs", needs);
        Func<string,int,bool,object> goal = delegate(string key, int quantity, bool repeatable) {
            object result = Activator.CreateInstance(goalType, true);
            Field(result, "Key", key); Field(result, "Definition", definition); Field(result, "Quantity", quantity); Field(result, "Repeatable", repeatable); return result;
        };
        Func<object,int> quantityOf = value => (int)goalType.GetField("Quantity", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);
        object tracked = goal("1|builder_a|bench", 3, true), otherOwner = goal("1|builder_b|bench", 5, true), otherCraft = goal("1|builder_a|other", 7, true), world = goal("2|builder_a|bench", 1, false);
        goals.Add(tracked); goals.Add(otherOwner); goals.Add(otherCraft); goals.Add(world);
        Require(ReferenceEquals(find.Invoke(null, new object[] {goals, definition, "builder_a"}), tracked), "Placement must match the original builder/definition key after menus close");
        Require(find.Invoke(null, new object[] {goals, definition, "missing"}) == null, "Unknown builders must not consume any goal");
        Field(definition, "one_time_craft", true); Require(find.Invoke(null, new object[] {goals, definition, "builder_a"}) == null, "One-time upgrades are a separate success path"); Field(definition, "one_time_craft", false);
        Field(definition, "build_type", Enum.Parse(definitionType.GetField("build_type").FieldType, "Remove"));
        Require(find.Invoke(null, new object[] {goals, definition, "builder_a"}) == null, "Removal controls must not fulfil construction goals");
        Field(definition, "build_type", Enum.Parse(definitionType.GetField("build_type").FieldType, "Put"));
        Type inventoryType = receiptType.GetField("Inventory", BindingFlags.Instance | BindingFlags.NonPublic).FieldType;
        object inventory = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(inventoryType);
        object unrelated = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(inventoryType);
        MethodInfo spend = receiptType.GetMethod("ObserveSpend", BindingFlags.Instance | BindingFlags.NonPublic), complete = receiptType.GetMethod("Complete", BindingFlags.Instance | BindingFlags.NonPublic);
        Func<object,object> receipt = capturedGoal => {
            object result = Activator.CreateInstance(receiptType, true); Field(result, "Goal", capturedGoal); Field(result, "Definition", definition); Field(result, "Inventory", inventory); return result;
        };
        Action<object,object,object,bool> observe = (value, inv, items, success) => spend.Invoke(value, new object[] {inv,items,success});
        Func<object,bool,bool,bool> finish = (value, normal, built) => (bool)complete.Invoke(value, new object[] {goals,normal,built,null});
        int before = (int)((IDictionary)aggregate.Invoke(null,null))["placement_material"];
        object first = receipt(tracked);
        observe(first, unrelated, needs, true); observe(first, inventory, Activator.CreateInstance(needsType), true); observe(first, inventory, needs, true);
        Require(finish(first,true,true) && quantityOf(tracked) == 2, "Only the exact operation's native spend may fulfil one unit");
        Require(!finish(first,true,true) && quantityOf(tracked) == 2, "A receipt must apply at most once");
        Require((int)((IDictionary)aggregate.Invoke(null,null))["placement_material"] == before - 4, "Derived requirements must fall by one project's exact needs");
        Require(quantityOf(otherOwner) == 5 && quantityOf(otherCraft) == 7 && quantityOf(world) == 1, "Other owner/recipe/world goals must be unchanged");
        foreach (int failure in new[] {0,1,2,3,4,5})
        {
            object value = receipt(tracked);
            if (failure != 0) observe(value, inventory, needs, failure != 1);
            if (failure == 2) observe(value, inventory, needs, true);
            Require(!finish(value, failure != 3, failure != 4 && failure != 5), "No-spend/failed-spend/duplicate-spend/exception/cancel/rejection must not reduce the plan");
            Require(quantityOf(tracked) == 2, "A failed attempt changed quantity");
        }
        object stale = receipt(tracked); observe(stale, inventory, needs, true); goals.Remove(tracked);
        object replacement = goal("1|builder_a|bench",2,true); goals.Add(replacement);
        Require(!finish(stale,true,true) && quantityOf(replacement) == 2, "Removed/re-added goals must not inherit an old captured placement");
        for (int i = 0; i < 2; i++) { object value=receipt(replacement); observe(value,inventory,needs,true); Require(finish(value,true,true), "Repeated placements must each consume one unit"); }
        Require(!goals.Contains(replacement), "Last successful placement must remove the goal at zero");
        goals.Clear();
        Console.WriteLine("PASS: placement key isolation, matching native spend, once-only receipts, failure preservation, exact requirements and zero removal. Native placement/patch acceptance still requires the game.");
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
        MethodInfo canIncrease = state.GetMethod("CanIncrease", BindingFlags.Static | BindingFlags.NonPublic);
        Require((bool)canIncrease.Invoke(null, new object[] {0, true}) && (bool)canIncrease.Invoke(null, new object[] {19, true}), "Repeatable right arrow must be available below20");
        Require(!(bool)canIncrease.Invoke(null, new object[] {20, true}), "Repeatable right arrow must disappear at20 in both views");
        Require((bool)canIncrease.Invoke(null, new object[] {0, false}) && !(bool)canIncrease.Invoke(null, new object[] {1, false}), "One-off right arrow must retain its0/1 boundary");
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
        Require((bool)canIncrease.Invoke(null, new object[] {19, true}), "Decrementing20->19 must make the right arrow available again");
        Require((bool)adjust.Invoke(null, new object[] {goals, goal, 1, null}), "19 must return to20 through the HUD path");
        for (int i = 0; i < 20; i++) adjust.Invoke(null, new object[] {goals, goal, -1, null});
        Require(goals.Count == 0, "A capped goal must still remove at zero");
        Console.WriteLine("PASS: stored-goal identity, aggregation, zero removal, stale callback, one-off bound and shared menu/HUD cap20.");
    }
}
