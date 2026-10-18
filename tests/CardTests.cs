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
            MethodInfo nameRight = layout.GetMethod("GoalNameRight", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (float digits in new[] {6f, 12f, 18f, 30f}) foreach (float arrow in new[] {4f, 9f, 20f}) foreach (float gap in new[] {0f, 13f, 40f})
            {
                float offset = (float)arrowOffset.Invoke(null, new object[] {digits, arrow, gap});
                Require(Math.Abs(offset - arrow / 2 - digits / 2 - gap) < .001f, "Arrow gap must start at the visible digit edge for one/two-digit counters");
                int edge = (int)nameRight.Invoke(null, new object[] {200, digits, arrow, gap});
                Require(edge + 4 <= 200 - offset - 10 && edge + 4 <= 200 - offset - arrow / 2, "Goal text must clear both the actual arrow collider and visible ink");
            }
            Require((int)nameRight.Invoke(null, new object[] {200, 12f, 9f, 13f}) > 200 - 48 - 4, "Measured controls must reclaim the excess nominal name reserve");
            Require((int)nameRight.Invoke(null, new object[] {200, 30f, 40f, 13f}) + 4 <= 200 - 15 - 13 - 40, "Wide-language glyphs must retain ink separation");
            MethodInfo words = layout.GetMethod("GoalWords", BindingFlags.Static | BindingFlags.NonPublic);
            Func<string, string> tokens = delegate(string text) {
                var items = new System.Collections.Generic.List<string>();
                foreach (string word in (IEnumerable)words.Invoke(null, new object[] {text})) items.Add(word);
                return string.Join("|", items.ToArray());
            };
            Require(tokens("Дистилляционный куб II") == "Дистилляционный|куб|II", "Russian long words must stay whole");
            Require(tokens(" \tИсследовательский\nстол\r\n") == "Исследовательский|стол", "Whitespace must not become part of word measurements");
            Require(tokens("Einbalsamiertisch II (Basis)") == "Einbalsamiertisch|II|(Basis)", "German words and attached punctuation must stay whole");
            Require(tokens("Палета-холодильник") == "Палета-холодильник", "A hyphenated name must fit without a forced internal break");
            Require(tokens("錬金術作業台") == "錬|金|術|作|業|台" && tokens("冷蔵パレット") == "冷|蔵|パ|レ|ッ|ト", "CJK names must retain native character wrap opportunities");
            Require(tokens("연금술 작업대 II") == "연금술|작업대|II", "Korean whitespace-delimited words must remain whole");
            Require(tokens(null) == "" && tokens("") == "", "Absent names must not request width growth");
            MethodInfo wordWidth = layout.GetMethod("GoalWordWidth", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo quantityCentre = layout.GetMethod("QuantityCenter", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (int initial in new[] {286, 400, 600}) foreach (int word in new[] {0, 30, 180, 280, 420}) foreach (float arrow in new[] {9f, 40f})
            {
                int grown = (int)wordWidth.Invoke(null, new object[] {initial, 12, 20, arrow, 13f, word});
                int centreBefore = (int)quantityCentre.Invoke(null, new object[] {initial, 12, 20});
                int centreAfter = (int)quantityCentre.Invoke(null, new object[] {grown, 12, 20});
                int right = (int)nameRight.Invoke(null, new object[] {centreAfter, 30f, arrow, 13f});
                Require(grown >= initial && centreBefore - initial == centreAfter - grown, "Word growth must preserve the minimum width and right-anchored counter/material centres");
                Require(word == 0 || right - 46 >= word + 2, "The longest measured word must fit including rounding slack and maximum quantity reserve");
                Require(grown == initial || right - 46 == word + 2, "Growth must be minimal rather than an arbitrary fixed expansion");
                Require((int)wordWidth.Invoke(null, new object[] {grown, 12, 20, arrow, 13f, word}) == grown, "Repeated redraw must not accumulate width growth");
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
            if (Array.IndexOf(args, "--state") >= 0) { CheckPinnedState(candidate); CheckPlacement(candidate); CheckWorldCompletion(candidate); CheckBuilderCompletion(candidate); CheckLighting(candidate); CheckInterior(candidate); CheckPersistence(candidate); }
            if (Array.IndexOf(args, "--no-calibration") >= 0) CheckNoCalibration(candidate);
            if (Array.IndexOf(args, "--native-rows") >= 0) CheckAbsentRows(candidate);
            int catalogue = Array.IndexOf(args, "--builder-catalog");
            if (catalogue >= 0) CheckBuilderCatalog(candidate, args[catalogue + 1]);
            int interiors = Array.IndexOf(args, "--interior-catalog");
            if (interiors >= 0) CheckInteriorCatalog(candidate, args[interiors + 1]);
            Console.WriteLine("PASS: centred rounded backing, feather/bounds/top-row coverage, quantity slot and 11-language schema. Native visual and input acceptance require the game.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void CheckNoCalibration(Assembly candidate)
    {
        const BindingFlags methods = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        Type plugin = candidate.GetType("CraftingPlanner.Plugin", true), calibration = candidate.GetType("CraftingPlanner.PlannerCalibration", true);
        Require(plugin.GetMethod("Update", methods) == null && plugin.GetMethod("OnGUI", methods) == null, "Public candidate must not register F2 update/IMGUI callbacks");
        Require(candidate.GetType("CraftingPlanner.CalibrationKeyGuard") == null && candidate.GetType("CraftingPlanner.CalibrationDirectionGuard") == null, "Calibration input patches must not ship");
        Require(!(bool)calibration.GetProperty("IsOpen", methods).GetValue(null, null) && !(bool)calibration.GetProperty("PreviewDone", methods).GetValue(null, null), "Public calibration flags must remain inactive");
        foreach (string name in new[] {"Update", "Draw", "Close"})
        {
            byte[] code = calibration.GetMethod(name, methods).GetMethodBody().GetILAsByteArray();
            Require(code.Length == 1 && code[0] == 0x2a, "Calibration entry must be a compiled no-op: " + name);
        }
        Require(calibration.GetFields(methods).Length == 0, "Temporary editor state must not ship");
        Console.WriteLine("PASS: public candidate has no F2/IMGUI callbacks, calibration input patches or editor state; inactive compatibility calls are no-ops.");
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
    static void CheckInterior(Assembly candidate)
    {
        const BindingFlags methods=BindingFlags.Static|BindingFlags.NonPublic, fields=BindingFlags.Instance|BindingFlags.NonPublic;
        Type planner=candidate.GetType("CraftingPlanner.Planner",true),state=candidate.GetType("CraftingPlanner.PlannerGoalState",true),goalType=candidate.GetType("CraftingPlanner.ProjectGoal",true);
        Type receiptType=candidate.GetType("CraftingPlanner.InteriorCompletionAttempt",true),defType=state.GetMethod("FindPlacement",methods).GetParameters()[1].ParameterType;
        IList goals=(IList)planner.GetField("Goals",methods).GetValue(null);goals.Clear();
        object definition=Activator.CreateInstance(defType);Field(definition,"id","keeper_room_builddesk::test");Field(definition,"build_type",Enum.Parse(defType.GetField("build_type").FieldType,"None"));Field(definition,"is_auto",true);Field(definition,"wait_script_callback",true);Field(definition,"end_script","keeper_table_place:place:table+stand");
        Type needsType=defType.GetField("needs").FieldType;IList needs=(IList)Activator.CreateInstance(needsType);object need=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]);Field(need,"id","interior_material");Field(need,"value",3);needs.Add(need);Field(definition,"needs",needs);
        Func<string,object> goal=key=>{object g=Activator.CreateInstance(goalType,true);Field(g,"Key",key);Field(g,"Definition",definition);Field(g,"Quantity",1);Field(g,"Repeatable",false);return g;};
        object tracked=goal("1|keeper_room_builddesk|keeper_room_builddesk::test"),other=goal("1|keeper_room_builddesk|other");goals.Add(tracked);goals.Add(other);
        var find=state.GetMethod("FindInteriorCompletion",methods);Func<object> match=()=>find.Invoke(null,new[]{(object)goals,definition,"keeper_room_builddesk"});
        Require(ReferenceEquals(match(),tracked),"Interior must match original builder goal");
        Require(ReferenceEquals(planner.GetMethod("FindInteriorCompletion",methods).Invoke(null,new[]{definition,(object)"keeper_room_builddesk"}),tracked),"Actual interior router must select the goal");
        Require(find.Invoke(null,new[]{(object)goals,definition,"other"})==null,"Other builder cannot match interior goal");
        foreach(string family in new[]{"keeper_bed_place","keeper_cupboard_place","keeper_table_place","keeper_walls_place","keeper_ornament_place"}) {Field(definition,"end_script",family+":place:item");Require(match()!=null,"Verified interior script family excluded");}
        foreach(string script in new[]{"unknown:place:item","keeper_table_place:apply:item","keeper_table_place:place:","keeper_table_place:place:item:extra","keeper_table_place"}) {Field(definition,"end_script",script);Require(match()==null,"Unsupported callback payload accepted");}Field(definition,"end_script","keeper_table_place:place:table+stand");
        foreach(string flag in new[]{"is_auto","wait_script_callback"}) {Field(definition,flag,false);Require(match()==null,"Required callback flag missing");Field(definition,flag,true);}
        Field(definition,"one_time_craft",true);Require(match()==null,"Immediate one-off upgrade must be excluded");Field(definition,"one_time_craft",false);
        Field(definition,"build_type",Enum.Parse(defType.GetField("build_type").FieldType,"Put"));Require(match()==null,"Free placement must be excluded");Field(definition,"build_type",Enum.Parse(defType.GetField("build_type").FieldType,"None"));
        Field(definition,"needs",null);Require(match()==null,"No-needs interior must be excluded");Field(definition,"needs",needs);
        Field(tracked,"Repeatable",true);Require(match()==null,"Interior goal stays quantity1");Field(tracked,"Repeatable",false);
        Func<object> make=()=>{object a=Activator.CreateInstance(receiptType,true);Field(a,"Goal",tracked);Field(a,"Definition",definition);return a;};
        Action<object,string,object[]> call=(a,name,args)=>receiptType.GetMethod(name,fields).Invoke(a,args);
        Func<object,bool> complete=a=>(bool)receiptType.GetMethod("Complete",fields).Invoke(a,new object[]{goals,null});
        foreach(int failure in new[]{-1,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19})
        {
            object a=make(),script=new object();
            if(failure!=0)call(a,"ObserveDispatch",new object[]{failure!=1});
            if(failure!=2)call(a,"ObserveFinish",new object[]{failure!=3});
            call(a,"ObserveMarker",new[]{failure==4?Activator.CreateInstance(defType):definition,(object)(failure!=5)});
            if(failure==6)call(a,"ObserveMarker",new[]{definition,(object)true});
            call(a,"CaptureScript",new[]{script});if(failure==19)call(a,"CaptureScript",new[]{new object()});
            if(failure!=7)call(a,"ObserveScriptCall",new object[]{failure!=8});
            if(failure==9)call(a,"ObserveScriptCall",new object[]{true});
            Require(!complete(a)&&goals.Contains(tracked),"Early craft/preview must never consume goal");
            if(failure!=13)call(a,"ObserveDecision",new object[]{failure!=14});
            if(failure==15||failure==16)call(a,"ObserveDecision",new object[]{failure!=16});
            call(a,"ObserveTermination",new[]{failure==10?new object():script,(object)(failure!=11)});
            if(failure==12)call(a,"ObserveTermination",new[]{script,(object)true});
            Require(!complete(a)&&goals.Contains(tracked),"Script end alone must wait for decision return");
            if(failure!=18)call(a,"ObserveDecisionReturn",new object[]{failure!=17});
            bool success=complete(a);Require(success==(failure==-1),"Interior failure/duplicate/missing latch misclassified:"+failure);
            if(success){Require(!complete(a)&&!goals.Contains(tracked)&&goals.Contains(other),"Interior once-only/control preservation failed");goals.Add(tracked);Field(tracked,"Quantity",1);}
            else Require(goals.Contains(tracked),"Cancelled/failed attempt removed goal");
        }
        // Asynchronous script end may follow normal Apply-handler return.
        object late=make(),lateScript=new object();call(late,"ObserveDispatch",new object[]{true});call(late,"ObserveFinish",new object[]{true});call(late,"ObserveMarker",new[]{definition,(object)true});call(late,"CaptureScript",new[]{lateScript});call(late,"ObserveScriptCall",new object[]{true});call(late,"ObserveDecision",new object[]{true});call(late,"ObserveDecisionReturn",new object[]{true});Require(!complete(late),"Apply must wait for its script");
        goals.Remove(tracked);object replacement=goal("1|keeper_room_builddesk|keeper_room_builddesk::test");goals.Add(replacement);call(late,"ObserveTermination",new[]{lateScript,(object)true});Require(!complete(late)&&goals.Contains(replacement),"Stale preview consumed re-added goal");
        goals.Remove(replacement);goals.Add(tracked);Field(tracked,"Quantity",1);object asyncAttempt=make();call(asyncAttempt,"ObserveDispatch",new object[]{true});call(asyncAttempt,"ObserveFinish",new object[]{true});call(asyncAttempt,"ObserveMarker",new[]{definition,(object)true});call(asyncAttempt,"CaptureScript",new[]{lateScript});call(asyncAttempt,"ObserveScriptCall",new object[]{true});call(asyncAttempt,"ObserveDecision",new object[]{true});call(asyncAttempt,"ObserveDecisionReturn",new object[]{true});call(asyncAttempt,"ObserveTermination",new[]{lateScript,(object)true});Require(complete(asyncAttempt),"Late normal script end must complete captured goal");
        Require((int)((IDictionary)planner.GetMethod("AggregateRequirements",methods).Invoke(null,null))["interior_material"]==3,"Interior completion must update derived requirements only for its own goal");
        Type integration=candidate.GetType("CraftingPlanner.PlannerInteriorCompletion",true);integration.GetField("_pending",methods).SetValue(null,make());integration.GetMethod("Reset",methods).Invoke(null,null);Require(integration.GetField("_pending",methods).GetValue(null)==null,"Session reset must invalidate captured preview");
        goals.Clear();Console.WriteLine("PASS: interior five-family routing, preview/cancel/error/duplicate preservation, exact async commitment, stale re-add, reset, control and derived requirements.");
    }
    static void CheckInteriorCatalog(Assembly candidate,string path)
    {
        const BindingFlags methods=BindingFlags.Static|BindingFlags.NonPublic;
        Type state=candidate.GetType("CraftingPlanner.PlannerGoalState",true),goalType=candidate.GetType("CraftingPlanner.ProjectGoal",true),defType=state.GetMethod("FindPlacement",methods).GetParameters()[1].ParameterType;
        var find=state.GetMethod("FindInteriorCompletion",methods);int count=0;
        foreach(string row in File.ReadAllLines(path))
        {
            string[] parts=row.Split('\t');object definition=Activator.CreateInstance(defType);Field(definition,"id",parts[0]);Field(definition,"end_script",parts[1]);Field(definition,"is_auto",true);Field(definition,"wait_script_callback",true);Field(definition,"build_type",Enum.Parse(defType.GetField("build_type").FieldType,"None"));
            Type needsType=defType.GetField("needs").FieldType;IList needs=(IList)Activator.CreateInstance(needsType);object need=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]);Field(need,"id","fixture_need");Field(need,"value",1);needs.Add(need);Field(definition,"needs",needs);
            IList goals=(IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(goalType));object g=Activator.CreateInstance(goalType,true);Field(g,"Key","1|keeper_room_builddesk|"+parts[0]);Field(g,"Definition",definition);Field(g,"Quantity",1);goals.Add(g);
            Require(ReferenceEquals(find.Invoke(null,new[]{(object)goals,definition,"keeper_room_builddesk"}),g),"Authored interior excluded:"+parts[0]);count++;
        }
        Require(count==39,"Interior corpus must contain39 definitions");Console.WriteLine("PASS: actual compiled production dispatch accepts all39 authored callback identities/scripts; not39 live acceptances.");
    }
    static void CheckLighting(Assembly candidate)
    {
        const BindingFlags methods=BindingFlags.Static|BindingFlags.NonPublic;
        const BindingFlags fields=BindingFlags.Instance|BindingFlags.NonPublic;
        Type planner=candidate.GetType("CraftingPlanner.Planner",true),state=candidate.GetType("CraftingPlanner.PlannerGoalState",true);
        Type goalType=candidate.GetType("CraftingPlanner.ProjectGoal",true),receiptType=candidate.GetType("CraftingPlanner.WorldCompletionAttempt",true);
        IList goals=(IList)planner.GetField("Goals",methods).GetValue(null);goals.Clear();
        var find=state.GetMethod("FindWorldCompletion",methods);var lighting=state.GetMethod("IsLightingProject",methods);
        Type defType=find.GetParameters()[1].ParameterType;var observe=receiptType.GetMethod("ObserveCompletion",fields);var complete=receiptType.GetMethod("Complete",fields);
        foreach(string id in new[]{"lantern_1","lantern_2","lantern_3"})
        {
            object definition=Activator.CreateInstance(defType);Field(definition,"id",id);Field(definition,"change_wgo",id+"_clone");
            Type needsType=defType.GetField("needs").FieldType;IList needs=(IList)Activator.CreateInstance(needsType);
            object need=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]);Field(need,"id","lamp_material");Field(need,"value",3);needs.Add(need);Field(definition,"needs",needs);
            Require((bool)lighting.Invoke(null,new[]{definition,(object)"lantern_place"}),"Native lamp family must allow repeated planning");
            Require(!(bool)lighting.Invoke(null,new[]{definition,(object)"other"}),"Other owner must not gain quantity20");
            Field(definition,"change_wgo","other");Require(!(bool)lighting.Invoke(null,new[]{definition,(object)"lantern_place"}),"Wrong replacement must fail closed");Field(definition,"change_wgo",id+"_clone");
            Field(definition,"id",id+"_to_lantern_3");Require(!(bool)lighting.Invoke(null,new[]{definition,(object)"lantern_place"}),"Lamp upgrades remain outside this quantity change");Field(definition,"id",id);
            object goal=Activator.CreateInstance(goalType,true);Field(goal,"Key","2|lantern_place|"+id);Field(goal,"Definition",definition);Field(goal,"Quantity",20);Field(goal,"Repeatable",true);goals.Add(goal);
            Require(ReferenceEquals(find.Invoke(null,new[]{(object)goals,definition,"lantern_place"}),goal),"Completion must find repeated lamp goal");
            var adjust=state.GetMethod("AdjustPinned",methods);object[] capped={goals,goal,1,null};Require(!(bool)adjust.Invoke(null,capped),"Lamp quantity must stop at20");
            object receipt=Activator.CreateInstance(receiptType,true);Field(receipt,"Goal",goal);Field(receipt,"Definition",definition);observe.Invoke(receipt,new[]{definition,(object)true});
            Require((bool)complete.Invoke(receipt,new object[]{goals,true,null}) && (int)goalType.GetField("Quantity",fields).GetValue(goal)==19,"One lamp completion must decrement20 to19");
            Require(!(bool)complete.Invoke(receipt,new object[]{goals,true,null}),"Lamp completion receipt must not repeat");
            Require((int)((IDictionary)planner.GetMethod("AggregateRequirements",methods).Invoke(null,null))["lamp_material"]==57,"Lamp materials must multiply remaining quantity");
            Field(goal,"Quantity",1);receipt=Activator.CreateInstance(receiptType,true);Field(receipt,"Goal",goal);Field(receipt,"Definition",definition);observe.Invoke(receipt,new[]{definition,(object)true});
            Require((bool)complete.Invoke(receipt,new object[]{goals,true,null})&&!goals.Contains(goal),"Last lamp must remove goal");
        }
        Require(!(bool)lighting.Invoke(null,new object[]{null,"lantern_place"}),"Null recipe must fail closed");
        Console.WriteLine("PASS: three native lamp families, narrow owner/replacement policy, cap20,20->19, multiplied materials, once-only and zero removal.");
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
    static void CheckBuilderCompletion(Assembly candidate)
    {
        const BindingFlags methods=BindingFlags.Static|BindingFlags.NonPublic, fields=BindingFlags.Instance|BindingFlags.NonPublic;
        Type state=candidate.GetType("CraftingPlanner.PlannerGoalState",true), planner=candidate.GetType("CraftingPlanner.Planner",true), goalType=candidate.GetType("CraftingPlanner.ProjectGoal",true), receiptType=candidate.GetType("CraftingPlanner.WorldCompletionAttempt",true);
        IList goals=(IList)planner.GetField("Goals",methods).GetValue(null); goals.Clear();
        Type defType=state.GetMethod("FindPlacement",methods).GetParameters()[1].ParameterType;
        object definition=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(defType);
        Field(definition,"id","upgrade"); Field(definition,"one_time_craft",true); Field(definition,"is_auto",true); Field(definition,"end_script","native_upgrade");
        Field(definition,"build_type",Enum.Parse(defType.GetField("build_type").FieldType,"None"));
        Type needsType=defType.GetField("needs").FieldType; IList needs=(IList)Activator.CreateInstance(needsType);
        object item=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]); Field(item,"id","upgrade_material"); Field(item,"value",10); needs.Add(item); Field(definition,"needs",needs);
        Func<string,bool,object> goal=delegate(string key,bool repeatable){object g=Activator.CreateInstance(goalType,true);Field(g,"Key",key);Field(g,"Definition",definition);Field(g,"Repeatable",repeatable);Field(g,"Quantity",1);return g;};
        object tracked=goal("1|builder_a|upgrade",false), other=goal("1|builder_b|upgrade",false), world=goal("2|builder_a|upgrade",false);
        goals.Add(tracked);goals.Add(other);goals.Add(world);
        MethodInfo find=state.GetMethod("FindBuilderCompletion",methods);
        Func<object> match=()=>find.Invoke(null,new[]{(object)goals,definition,"builder_a"});
        Require(ReferenceEquals(match(),tracked),"Builder completion must select its exact original builder key");
        Require(ReferenceEquals(planner.GetMethod("FindWorldCompletion",methods).Invoke(null,new[]{definition,(object)"builder_a"}),tracked),"Actual completion hook router must include builder upgrades");
        Require(find.Invoke(null,new[]{(object)goals,definition,"unknown"})==null,"Unknown owner must not fulfil another upgrade");
        foreach(string flag in new[]{"one_time_craft","is_auto"}){Field(definition,flag,false);Require(match()==null,"Unsupported builder flags must fail closed");Field(definition,flag,true);}
        Field(definition,"wait_script_callback",true);Require(match()==null,"Script callback builds remain excluded");Field(definition,"wait_script_callback",false);
        Field(definition,"end_script",null);Require(match()==null,"Unverified scriptless builder shape must be excluded");Field(definition,"end_script","native_upgrade");
        Field(definition,"build_type",Enum.Parse(defType.GetField("build_type").FieldType,"Put"));Require(match()==null,"Placement must not be counted again at later craft completion");Field(definition,"build_type",Enum.Parse(defType.GetField("build_type").FieldType,"None"));
        Field(tracked,"Repeatable",true);Require(match()==null,"One-off completion cannot consume repeatable goals");Field(tracked,"Repeatable",false);
        Func<object> receipt=()=>{object r=Activator.CreateInstance(receiptType,true);Field(r,"Goal",tracked);Field(r,"Definition",definition);return r;};
        MethodInfo mark=receiptType.GetMethod("ObserveCompletion",fields), complete=receiptType.GetMethod("Complete",fields);
        foreach(int mode in new[]{0,1,2,3}) {object r=receipt();if(mode!=0) mark.Invoke(r,new[]{definition,(object)(mode!=1)});if(mode==2)mark.Invoke(r,new[]{definition,(object)true});Require(!(bool)complete.Invoke(r,new object[]{goals,mode!=3,null}) && goals.Contains(tracked),"Absent/failed/duplicate marker or native failure must preserve upgrade");}
        object success=receipt();mark.Invoke(success,new[]{definition,(object)true});
        Require((bool)complete.Invoke(success,new object[]{goals,true,null}) && !goals.Contains(tracked),"Synchronous native upgrade completion must remove its one-off goal");
        Require(!(bool)complete.Invoke(success,new object[]{goals,true,null}) && goals.Contains(other) && goals.Contains(world),"Duplicate receipt or other contexts must not remove unrelated goals");
        IDictionary requirements=(IDictionary)planner.GetMethod("AggregateRequirements",methods).Invoke(null,null);
        Require((int)requirements["upgrade_material"]==20,"Derived materials must retain only the other two goals");
        goals.Clear();
        Console.WriteLine("PASS: builder completion router, original owner/context, unsupported shapes, successful marker, failure preservation, one-off removal and unrelated requirements.");
    }
    static void CheckBuilderCatalog(Assembly candidate,string path)
    {
        const BindingFlags methods=BindingFlags.Static|BindingFlags.NonPublic;
        Type state=candidate.GetType("CraftingPlanner.PlannerGoalState",true), goalType=candidate.GetType("CraftingPlanner.ProjectGoal",true);
        var find=state.GetMethod("FindBuilderCompletion",methods);var placement=state.GetMethod("FindPlacement",methods);Type defType=placement.GetParameters()[1].ParameterType;
        Type listType=typeof(System.Collections.Generic.List<>).MakeGenericType(goalType);IList goals=(IList)Activator.CreateInstance(listType);
        int upgrades=0,places=0,excluded=0;
        foreach(string line in File.ReadAllLines(path))
        {
            string[] row=line.Split('\t');object definition=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(defType);
            Field(definition,"id",row[1]);Field(definition,"build_type",Enum.ToObject(defType.GetField("build_type").FieldType,int.Parse(row[2])));Field(definition,"one_time_craft",true);Field(definition,"is_auto",row[3]=="1");Field(definition,"wait_script_callback",row[4]=="1");Field(definition,"end_script",row[5]);
            Type needsType=defType.GetField("needs").FieldType;IList needs=(IList)Activator.CreateInstance(needsType);
            if(row[6].Length>0)foreach(string value in row[6].Split(',')){int split=value.LastIndexOf(':');object item=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]);Field(item,"id",value.Substring(0,split));Field(item,"value",int.Parse(value.Substring(split+1)));needs.Add(item);}Field(definition,"needs",needs);
            object goal=Activator.CreateInstance(goalType,true);Field(goal,"Key","1|"+row[0]+"|"+row[1]);Field(goal,"Definition",definition);Field(goal,"Repeatable",false);Field(goal,"Quantity",1);goals.Add(goal);
            object matched=find.Invoke(null,new[]{(object)goals,definition,row[0]});object put=placement.Invoke(null,new[]{(object)goals,definition,row[0]});
            if(row[2]=="0"){Require(ReferenceEquals(put,goal) && matched==null,"One-off placement dispatch mismatch: "+row[1]);places++;}
            else if(needs.Count>0){Require(ReferenceEquals(matched,goal) && put==null,"Builder upgrade dispatch mismatch: "+row[1]);upgrades++;}
            else {Require(matched==null && put==null,"No-needs non-goal must remain excluded");excluded++;}
            goals.Clear();
        }
        Require(upgrades==39 && places==17 && excluded==1,"Unexpected authored one-time builder coverage");
        Console.WriteLine("PASS: all57 authored one-time builder definitions dispatch as39 upgrades,17 placements and1 no-needs exclusion. This is static compiled-policy coverage, not57 live executions.");
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
        Field(definition, "one_time_craft", true); Require(find.Invoke(null, new object[] {goals, definition, "builder_a"}) == null, "One-time placement cannot consume a repeatable goal"); Field(definition, "one_time_craft", false);
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
        Field(definition,"one_time_craft",true); object oneoff=goal("1|builder_a|bench",1,false); goals.Add(oneoff);
        Require(ReferenceEquals(find.Invoke(null,new object[]{goals,definition,"builder_a"}),oneoff),"One-time Put must use placement commitment");
        object rejected=receipt(oneoff);observe(rejected,inventory,needs,true);Require(!finish(rejected,true,false) && goals.Contains(oneoff),"Rejected one-time placement must preserve its goal");
        object placed=receipt(oneoff);observe(placed,inventory,needs,true);Require(finish(placed,true,true) && !goals.Contains(oneoff),"Successful one-time placement must remove its goal");
        Require(!finish(placed,true,true),"One-time placement must not apply twice");
        goals.Clear();
        Console.WriteLine("PASS: placement key isolation, matching native spend, once-only receipts, failure preservation, exact requirements and zero removal. Native placement/patch acceptance still requires the game.");
    }
    static void Field(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(target, value);
    }
    static void CheckPersistence(Assembly candidate)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic, fields = BindingFlags.Instance | BindingFlags.NonPublic;
        Type store = candidate.GetType("CraftingPlanner.PlannerPlanStore", true), planner = candidate.GetType("CraftingPlanner.Planner", true);
        Type goalType = candidate.GetType("CraftingPlanner.ProjectGoal", true), savedType = candidate.GetType("CraftingPlanner.SavedGoal", true);
        Type buildType = candidate.GetType("CraftingPlanner.PlannerGoalState", true).GetMethod("FindPlacement", flags).GetParameters()[1].ParameterType;
        IList goals = (IList)planner.GetField("Goals", flags).GetValue(null); goals.Clear();
        Func<string, int, object> make = (key, quantity) => {object g = Activator.CreateInstance(goalType, true); Field(g, "Key", key); Field(g, "Quantity", quantity); return g;};
        MethodInfo encode = store.GetMethod("Encode", flags), decode = store.GetMethod("Decode", flags), resolve = store.GetMethod("Resolve", flags);
        Func<string[], IList> read = lines => (IList)decode.Invoke(null, new object[] {lines});
        string first = "1|builder|builder:p:скамья", second = "2|broken|repair";
        goals.Add(make(first, 20)); goals.Add(make(second, 1));
        string[] encoded = (string[])encode.Invoke(null, new object[] {goals});
        IList restored = read(encoded);
        Require(restored.Count == 2 && (string)savedType.GetField("Key", fields).GetValue(restored[0]) == first && (int)savedType.GetField("Quantity", fields).GetValue(restored[0]) == 20 && (string)savedType.GetField("Key", fields).GetValue(restored[1]) == second, "Persistence must preserve ordered keys, Unicode and quantities");
        goals.Clear(); string[] empty = (string[])encode.Invoke(null, new object[] {goals});
        Require(read(empty).Count == 0, "Saved empty plan must remain empty after deletion/completion");
        Action<string[]> reject = lines => { bool failed = false; try {read(lines);} catch (TargetInvocationException ex) {failed = ex.InnerException is FormatException || ex.InnerException is System.Text.DecoderFallbackException;} Require(failed, "Invalid plan must fail closed"); };
        reject(new string[0]); reject(new[] {"CraftingPlannerGoals\t2"}); reject(new[] {encoded[0], encoded[1], encoded[1]});
        foreach (string qty in new[] {"0", "21", "-1", "1.5", "", "2147483648"}) reject(new[] {encoded[0], encoded[1].Split('\t')[0] + "\t" + qty});
        foreach (string key in new[] {"0|builder|recipe", "1||recipe", "1|owner|", "1|owner|recipe|extra"}) reject(new[] {encoded[0], Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key)) + "\t1"});
        reject(new[] {encoded[0], "not-base64\t1"});
        object definition = Activator.CreateInstance(buildType); Field(definition, "id", "builder:p:скамья");
        Field(definition, "build_type", Enum.Parse(buildType.GetField("build_type").FieldType, "Put"));
        Type needsType = buildType.GetField("needs").FieldType; IList needs = (IList)Activator.CreateInstance(needsType);
        object need = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(needsType.GetGenericArguments()[0]); Field(need, "id", "plank"); Field(need, "value", 3); needs.Add(need); Field(definition, "needs", needs);
        object resolved = resolve.Invoke(null, new[] {restored[0], definition});
        Require(resolved != null && ReferenceEquals(goalType.GetField("Definition", fields).GetValue(resolved), definition) && (bool)goalType.GetField("Repeatable", fields).GetValue(resolved), "Restore must use current host definition and repeatability");
        Require(resolve.Invoke(null, new[] {restored[0], (object)null}) == null, "Removed host definitions must be skipped");
        Field(definition, "one_time_craft", true); Require(resolve.Invoke(null, new[] {restored[0], definition}) == null, "One-off goals must not restore repeatable quantities");
        Field(restored[0], "Quantity", 1); Require(resolve.Invoke(null, new[] {restored[0], definition}) != null, "One-off quantity one must restore");
        Field(definition, "needs", null); Require(resolve.Invoke(null, new[] {restored[0], definition}) == null, "Unsupported materials must fail closed");
        object world = Activator.CreateInstance(buildType.BaseType); Field(world, "id", "repair"); Field(world, "change_wgo", "fixed"); Field(world, "needs", needs);
        Require(resolve.Invoke(null, new[] {restored[1], world}) != null, "Verified world-change shape must restore");
        Field(world, "change_wgo", ""); Require(resolve.Invoke(null, new[] {restored[1], world}) == null, "Ordinary production must not enter restored plan");
        MethodInfo committed = store.GetMethod("Committed", flags);
        Func<string, string, bool, long, long, bool> commit = (a,b,e,x,y) => (bool)committed.Invoke(null,new object[]{a,b,e,x,y});
        Require(commit("a","a",true,1,2) && commit("a","a",false,0,0), "Exact created/rewritten native save must permit publishing");
        Require(!commit("a","b",true,1,2) && !commit("a","a",true,1,1) && !commit(null,null,false,0,1), "Failed, mismatched and uncaptured saves must not publish");
        string root = Path.Combine(Path.GetTempPath(), "BuildingPlannerTests-" + Guid.NewGuid().ToString("N"));
        MethodInfo pathMethod = store.GetMethod("SnapshotPath", flags), write = store.GetMethod("Write", flags);
        Func<string,string,string> path = (slot,hash) => (string)pathMethod.Invoke(null,new object[]{root,Path.Combine(root,slot),hash});
        string a1=path("slotA.dat","world1"), a2=path("slotA.dat","world2"), b1=path("slotB.dat","world1");
        Require(a1 != a2 && a1 != b1 && a2 != b1, "Save slot and native snapshot version must both isolate plans");
        try
        {
            write.Invoke(null,new object[]{a1,encoded}); write.Invoke(null,new object[]{a2,empty}); write.Invoke(null,new object[]{b1,encoded});
            Require(read(File.ReadAllLines(a1)).Count==2 && read(File.ReadAllLines(a2)).Count==0 && read(File.ReadAllLines(b1)).Count==2, "Older world and another slot must retain their own plans");
            write.Invoke(null,new object[]{a1,empty}); Require(read(File.ReadAllLines(a1)).Count==0 && read(File.ReadAllLines(b1)).Count==2, "Atomic overwrite must save empty plan without affecting another slot");
            Require(Directory.GetFiles(Path.GetDirectoryName(a1),"*.tmp").Length==0, "Storage must not leave temporary payloads");
        }
        finally
        {
            foreach(string file in new[]{a1,a2,b1}) if(File.Exists(file)) File.Delete(file);
            foreach(string directory in Directory.GetDirectories(root)) Directory.Delete(directory);
            if(Directory.Exists(root)) Directory.Delete(root);
        }
        Console.WriteLine("Persistence codec, current-definition validation, save commitment and slot/version storage checks passed.");
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
