using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CraftingPlanner
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.gyk.craftingplanner";
        public const string PluginName = "Crafting Planner";
        public const string PluginVersion = "0.1.4";

        private Harmony _harmony;

        private void Awake()
        {
            Planner.Log = Logger;
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            Logger.LogInfo("CRAFTING_PLANNER event=loaded version=" + PluginVersion);
        }

        private void OnDestroy()
        {
            PlannerCraftOverlay.Destroy();
            PlannerHud.Destroy();
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
        }
    }

    internal enum ProjectContextKind
    {
        None,
        Builder,
        WorldChange
    }

    internal sealed class ProjectGoal
    {
        internal string Key;
        internal CraftDefinition Definition;
        internal int Quantity;
        internal bool Repeatable;
    }

    internal static class Planner
    {
        internal static ManualLogSource Log;

        private static readonly List<ProjectGoal> Goals = new List<ProjectGoal>();
        private static readonly HashSet<CraftDefinition> BuildDefinitions = new HashSet<CraftDefinition>();

        private static ProjectContextKind _context;
        private static WorldGameObject _contextOwner;
        private static CraftDefinition _focusedProject;
        private static WorldGameObject _currentChest;

        internal static bool HasGoals
        {
            get { return Goals.Count > 0; }
        }

        internal static void ResetSession()
        {
            Goals.Clear();
            ClearProjectContext();
            _currentChest = null;
            PlannerCraftOverlay.Hide();
            PlannerHud.Refresh();
            LogLine("event=session_reset");
        }

        internal static void OpenBuildContext(WorldGameObject builder, CraftsInventory craftsInventory)
        {
            _context = ProjectContextKind.Builder;
            _contextOwner = builder;
            _focusedProject = null;
            BuildDefinitions.Clear();

            List<ObjectCraftDefinition> definitions = null;
            try
            {
                definitions = craftsInventory != null ? craftsInventory.GetObjectCraftsList() : null;
            }
            catch (Exception ex)
            {
                LogLine("event=project_context kind=builder result=error error=" + ex.GetType().Name);
            }

            if (definitions != null)
            {
                foreach (ObjectCraftDefinition definition in definitions)
                {
                    if (definition != null)
                    {
                        BuildDefinitions.Add(definition);
                    }
                }
            }

            LogLine(
                "event=project_context kind=builder owner=" + Safe(builder != null ? builder.obj_id : null) +
                " count=" + BuildDefinitions.Count);
        }

        internal static void OpenWorldContext(WorldGameObject owner)
        {
            _context = ProjectContextKind.WorldChange;
            _contextOwner = owner;
            _focusedProject = null;
            BuildDefinitions.Clear();

            LogLine("event=project_context kind=world owner=" + Safe(owner != null ? owner.obj_id : null));
        }

        internal static void ClearProjectContext()
        {
            _context = ProjectContextKind.None;
            _contextOwner = null;
            _focusedProject = null;
            BuildDefinitions.Clear();
        }

        internal static void Focus(CraftDefinition definition)
        {
            _focusedProject = IsSupportedProject(definition) ? definition : null;
            PlannerCraftOverlay.Refresh();
        }

        internal static void Blur(CraftDefinition definition)
        {
            if (_focusedProject == definition)
            {
                _focusedProject = null;
                PlannerCraftOverlay.Refresh();
            }
        }

        internal static bool ShouldShowCraftOverlay
        {
            get { return _context == ProjectContextKind.Builder || _focusedProject != null; }
        }

        internal static bool AdjustFocusedProject(int delta, string trigger)
        {
            CraftDefinition definition = _focusedProject;
            if (definition == null || !IsSupportedProject(definition))
            {
                return false;
            }

            string key = BuildGoalKey(definition);
            ProjectGoal goal = Goals.FirstOrDefault(x => x.Key == key);

            if (delta > 0)
            {
                bool repeatable = IsRepeatable(definition);
                if (goal == null)
                {
                    goal = new ProjectGoal
                    {
                        Key = key,
                        Definition = definition,
                        Quantity = 1,
                        Repeatable = repeatable
                    };
                    Goals.Add(goal);
                }
                else if (goal.Repeatable)
                {
                    if (goal.Quantity < int.MaxValue)
                    {
                        goal.Quantity++;
                    }
                }

                LogLine(
                    "event=goal_adjust action=add trigger=" + trigger +
                    " id=" + Safe(definition.id) +
                    " quantity=" + goal.Quantity +
                    " repeatable=" + goal.Repeatable);
            }
            else if (delta < 0)
            {
                if (goal == null)
                {
                    return false;
                }

                goal.Quantity--;
                if (goal.Quantity <= 0)
                {
                    Goals.Remove(goal);
                    LogLine(
                        "event=goal_adjust action=remove trigger=" + trigger +
                        " id=" + Safe(definition.id) +
                        " quantity=0");
                }
                else
                {
                    LogLine(
                        "event=goal_adjust action=decrement trigger=" + trigger +
                        " id=" + Safe(definition.id) +
                        " quantity=" + goal.Quantity);
                }
            }
            else
            {
                return false;
            }

            PlannerCraftOverlay.Refresh();
            PlannerHud.Refresh();
            return true;
        }

        internal static Dictionary<string, int> AggregateRequirements()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (ProjectGoal goal in Goals)
            {
                if (goal == null || goal.Definition == null || goal.Quantity <= 0)
                {
                    continue;
                }

                List<Item> needs = goal.Definition.needs;
                if (needs == null)
                {
                    continue;
                }

                foreach (Item need in needs)
                {
                    if (need == null || string.IsNullOrEmpty(need.id) || need.value <= 0)
                    {
                        continue;
                    }

                    long amount = (long)need.value * goal.Quantity;
                    int current;
                    result.TryGetValue(need.id, out current);
                    long total = (long)current + amount;
                    result[need.id] = total > int.MaxValue ? int.MaxValue : (int)total;
                }
            }

            return result;
        }

        internal static IList<ProjectGoal> GetGoals()
        {
            return Goals;
        }

        internal static int GetPlayerHave(string itemId)
        {
            if (MainGame.me == null || MainGame.me.player == null || MainGame.me.player.data == null)
            {
                return 0;
            }

            return MainGame.me.player.data.GetTotalCount(itemId, true);
        }

        internal static string GetItemName(string itemId)
        {
            try
            {
                ItemDefinition definition = GameBalance.me.GetDataOrNull<ItemDefinition>(itemId);
                if (definition != null)
                {
                    return definition.GetItemName(true);
                }
            }
            catch
            {
            }

            return itemId;
        }

        internal static void OpenChest(WorldGameObject chest)
        {
            _currentChest = chest;
            LogLine("event=chest_open chest=" + Safe(chest != null ? chest.obj_id : null));
        }

        internal static void CloseChest()
        {
            _currentChest = null;
        }

        internal static bool TakeNeeded(ChestGUI chestGui, string trigger)
        {
            if (_currentChest == null || !HasGoals || MainGame.me == null || MainGame.me.player == null)
            {
                return false;
            }

            WorldGameObject player = MainGame.me.player;
            MultiInventory source = _currentChest.GetMultiInventoryOfWGOWithoutWorldZone(true);
            MultiInventory target = player.GetMultiInventoryOfWGOWithoutWorldZone(true);

            if (source == null || target == null)
            {
                LogLine("event=take_needed result=failed reason=null_inventory trigger=" + trigger);
                return true;
            }

            Dictionary<string, int> required = AggregateRequirements();
            bool movedAnything = false;

            foreach (KeyValuePair<string, int> pair in required)
            {
                string itemId = pair.Key;
                int requiredCount = pair.Value;
                int haveBefore = GetPlayerHave(itemId);
                int missing = Math.Max(requiredCount - haveBefore, 0);
                int available = source.GetTotalCount(itemId, MultiInventory.DestinationType.AllFromFirst, true);
                int capacity = target.CanAddCount(itemId, true);
                int requested = Math.Min(missing, Math.Min(available, capacity));
                bool success = true;

                if (requested > 0)
                {
                    success = source.MoveItemTo(
                        target,
                        new Item(itemId, requested),
                        requested,
                        false,
                        true);

                    movedAnything |= success;
                }

                LogLine(
                    "event=take_item trigger=" + trigger +
                    " item=" + Safe(itemId) +
                    " required=" + requiredCount +
                    " have=" + haveBefore +
                    " missing=" + missing +
                    " storage=" + available +
                    " capacity=" + capacity +
                    " requested=" + requested +
                    " success=" + success);
            }

            if (movedAnything && chestGui != null)
            {
                if (chestGui.player_panel != null)
                {
                    chestGui.player_panel.Redraw();
                }

                if (chestGui.chest_panel != null)
                {
                    chestGui.chest_panel.Redraw();
                }

                TooltipsManager.Redraw();
            }

            PlannerHud.Refresh();
            LogLine("event=take_needed result=complete trigger=" + trigger + " moved_any=" + movedAnything);
            return true;
        }

        private static bool IsSupportedProject(CraftDefinition definition)
        {
            if (!HasPlainNeeds(definition))
            {
                return false;
            }

            if (_context == ProjectContextKind.Builder)
            {
                return definition is ObjectCraftDefinition && BuildDefinitions.Contains(definition);
            }

            if (_context == ProjectContextKind.WorldChange)
            {
                return !(definition is ObjectCraftDefinition) &&
                       !string.IsNullOrEmpty(definition.change_wgo);
            }

            return false;
        }

        private static bool HasPlainNeeds(CraftDefinition definition)
        {
            if (definition == null || definition.needs == null || definition.needs.Count == 0)
            {
                return false;
            }

            if (definition.needs_from_wgo != null && definition.needs_from_wgo.Count > 0)
            {
                return false;
            }

            foreach (Item need in definition.needs)
            {
                if (need == null || string.IsNullOrEmpty(need.id) || need.value <= 0)
                {
                    return false;
                }

                if (need.multiquality_items != null && need.multiquality_items.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsRepeatable(CraftDefinition definition)
        {
            ObjectCraftDefinition objectCraft = definition as ObjectCraftDefinition;
            return _context == ProjectContextKind.Builder &&
                   objectCraft != null &&
                   objectCraft.build_type == ObjectCraftDefinition.BuildType.Put &&
                   !definition.one_time_craft;
        }

        private static string BuildGoalKey(CraftDefinition definition)
        {
            string owner = _contextOwner != null ? _contextOwner.obj_id : "";
            return ((int)_context).ToString() + "|" + owner + "|" + definition.id;
        }

        internal static string GetProjectName(CraftDefinition definition)
        {
            try
            {
                string token = definition.GetNameNonLocalized();
                string localized = GJL.L(token);
                return string.IsNullOrEmpty(localized) ? definition.id : localized;
            }
            catch
            {
                return definition.id;
            }
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value)
                ? "null"
                : value.Replace(" ", "_").Replace("=", "_").Replace("\r", "_").Replace("\n", "_");
        }

        internal static void LogLine(string details)
        {
            if (Log != null)
            {
                Log.LogInfo("CRAFTING_PLANNER " + details);
            }
        }
    }

    internal static class PlannerLocalization
    {
        private sealed class TextSet
        {
            internal readonly string Projects;
            internal readonly string Materials;

            internal TextSet(string projects, string materials)
            {
                Projects = projects;
                Materials = materials;
            }
        }

        private static readonly Dictionary<string, TextSet> Texts =
            new Dictionary<string, TextSet>(StringComparer.OrdinalIgnoreCase)
            {
                { "en",    new TextSet("Projects", "Materials — Required / Have / Missing") },
                { "de",    new TextSet("Projekte", "Materialien — Benötigt / Vorhanden / Fehlt") },
                { "fr",    new TextSet("Projets", "Matériaux — Requis / Possédés / Manquants") },
                { "pt-br", new TextSet("Projetos", "Materiais — Necessário / Possui / Faltando") },
                { "es",    new TextSet("Proyectos", "Materiales — Necesario / Tienes / Falta") },
                { "ru",    new TextSet("Проекты", "Материалы — Нужно / Есть / Не хватает") },
                { "it",    new TextSet("Progetti", "Materiali — Richiesti / Disponibili / Mancanti") },
                { "pl",    new TextSet("Projekty", "Materiały — Potrzeba / Masz / Brakuje") },
                { "ja",    new TextSet("プロジェクト", "材料 — 必要 / 所持 / 不足") },
                { "zh-cn", new TextSet("项目", "材料 — 需要 / 持有 / 缺少") },
                { "ko",    new TextSet("프로젝트", "재료 — 필요 / 보유 / 부족") }
            };

        internal static string Projects
        {
            get { return Current.Projects; }
        }

        internal static string Materials
        {
            get { return Current.Materials; }
        }

        internal static string CurrentLanguage
        {
            get
            {
                string language = "";
                try
                {
                    language = GameSettings.GetCurrentLanguage();
                }
                catch
                {
                }

                return Normalize(language);
            }
        }

        internal static void ApplyFont(UILabel label)
        {
            if (label == null)
            {
                return;
            }

            try
            {
                GJL.EnsureLabelHasCorrectFont(label, false);
            }
            catch
            {
            }
        }

        internal static void OnLanguageChanged()
        {
            PlannerHud.OnLanguageChanged();
            PlannerCraftOverlay.OnLanguageChanged();
            Planner.LogLine("event=language_changed language=" + CurrentLanguage);
        }

        private static TextSet Current
        {
            get
            {
                TextSet value;
                return Texts.TryGetValue(CurrentLanguage, out value) ? value : Texts["en"];
            }
        }

        private static string Normalize(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return "en";
            }

            return language.Trim().ToLowerInvariant().Replace('_', '-');
        }
    }

    internal static class PlannerHud
    {
        private const string LabelName = "CraftingPlannerHUD";
        private const float GapBelowZoneName = 12f;

        private static UILabel _label;
        private static HUD _hud;
        private static UILabel _reference;
        private static Transform _parent;
        private static UIPanel _panel;
        private static bool _geometryLogged;

        internal static void Ensure(HUD hud)
        {
            if (hud == null)
            {
                return;
            }

            if (_label != null && _hud == hud)
            {
                Reposition();
                return;
            }

            Destroy();

            UILabel reference = hud.zone_name;
            if (reference == null || reference.transform.parent == null)
            {
                Planner.LogLine("event=hud result=skipped reason=no_zone_name_reference");
                return;
            }

            Transform parent = reference.transform.parent;
            UIPanel panel = reference.GetComponentInParent<UIPanel>();
            if (panel == null)
            {
                Planner.LogLine("event=hud result=skipped reason=no_zone_name_panel");
                return;
            }

            _hud = hud;
            _reference = reference;
            _parent = parent;
            _panel = panel;
            _geometryLogged = false;

            _label = UnityEngine.Object.Instantiate(reference, parent, false);
            PrepareLabel(_label, LabelName, reference.gameObject.layer);
            _label.pivot = UIWidget.Pivot.TopRight;
            Reposition();
            Refresh();

            Planner.LogLine(
                "event=hud result=created reference=" + reference.gameObject.name +
                " parent=" + parent.gameObject.name +
                " panel=" + panel.gameObject.name +
                " layer=" + _label.gameObject.layer +
                " pos=" + FormatVector(_label.transform.localPosition));
        }

        internal static void Refresh()
        {
            if (_label == null)
            {
                if (GUIElements.me != null && GUIElements.me.hud != null)
                {
                    Ensure(GUIElements.me.hud);
                }

                if (_label == null)
                {
                    return;
                }
            }

            if (!Planner.HasGoals)
            {
                _label.text = "";
                _label.gameObject.SetActive(false);
                return;
            }

            _label.text = BuildText();
            PlannerLocalization.ApplyFont(_label);
            Reposition();
            _label.gameObject.SetActive(true);

            Planner.LogLine(
                "event=hud_refresh visible=true goals=" + Planner.GetGoals().Count +
                " active_self=" + _label.gameObject.activeSelf +
                " active_hierarchy=" + _label.gameObject.activeInHierarchy +
                " pos=" + FormatVector(_label.transform.localPosition));

            if (!_geometryLogged && _label.gameObject.activeInHierarchy)
            {
                LogGeometry("first_visible");
                _geometryLogged = true;
            }
        }

        internal static void RefreshIfVisible()
        {
            if (!Planner.HasGoals || GUIElements.me == null || GUIElements.me.hud == null)
            {
                return;
            }

            if (GUIElements.me.hud.gameObject.activeInHierarchy)
            {
                Refresh();
            }
        }

        internal static void OnLanguageChanged()
        {
            PlannerLocalization.ApplyFont(_label);
            Refresh();
        }

        internal static void Destroy()
        {
            if (_label != null)
            {
                UnityEngine.Object.Destroy(_label.gameObject);
            }

            _label = null;
            _hud = null;
            _reference = null;
            _parent = null;
            _panel = null;
            _geometryLogged = false;
        }

        internal static string BuildText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Crafting Planner\n");
            sb.Append(PlannerLocalization.Projects);
            sb.Append(":\n");

            foreach (ProjectGoal goal in Planner.GetGoals())
            {
                sb.Append("• ");
                sb.Append(Planner.GetProjectName(goal.Definition));
                sb.Append(" ×");
                sb.Append(goal.Quantity);
                sb.Append("\n");
            }

            Dictionary<string, int> required = Planner.AggregateRequirements();
            List<KeyValuePair<string, int>> rows = required.ToList();
            rows.Sort(delegate(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
            {
                return string.Compare(
                    Planner.GetItemName(a.Key),
                    Planner.GetItemName(b.Key),
                    StringComparison.CurrentCultureIgnoreCase);
            });

            sb.Append("\n");
            sb.Append(PlannerLocalization.Materials);
            sb.Append(":\n");

            foreach (KeyValuePair<string, int> pair in rows)
            {
                int have = Planner.GetPlayerHave(pair.Key);
                int missing = Math.Max(pair.Value - have, 0);
                sb.Append(Planner.GetItemName(pair.Key));
                sb.Append(": ");
                sb.Append(pair.Value);
                sb.Append(" / ");
                sb.Append(have);
                sb.Append(" / ");
                sb.Append(missing);
                sb.Append("\n");
            }

            return sb.ToString().TrimEnd();
        }

        internal static void PrepareLabel(UILabel label, string name, int layer)
        {
            label.gameObject.name = name;
            label.gameObject.layer = layer;
            label.SetAnchor((GameObject)null);
            label.transform.localRotation = Quaternion.identity;
            label.transform.localScale = Vector3.one;
            label.pivot = UIWidget.Pivot.TopLeft;
            label.width = 620;
            label.height = 300;
            label.multiLine = true;
            label.overflowMethod = UILabel.Overflow.ResizeHeight;
            label.alpha = 1f;
            label.depth = Math.Max(label.depth, 5000);
            PlannerLocalization.ApplyFont(label);
        }

        internal static string FormatVector(Vector3 value)
        {
            return value.x.ToString("0.0") + "," + value.y.ToString("0.0") + "," + value.z.ToString("0.0");
        }

        private static void Reposition()
        {
            if (_label == null || _reference == null || _parent == null)
            {
                return;
            }

            Vector3[] corners = _reference.localCorners;
            if (corners == null || corners.Length < 4)
            {
                return;
            }

            Vector3 bottomRightWorld = _reference.transform.TransformPoint(corners[3]);
            Vector3 bottomRightInParent = _parent.InverseTransformPoint(bottomRightWorld);

            _label.transform.localPosition = new Vector3(
                bottomRightInParent.x,
                bottomRightInParent.y - GapBelowZoneName,
                bottomRightInParent.z);
        }

        private static void LogGeometry(string reason)
        {
            if (_label == null || _reference == null || _parent == null || _panel == null)
            {
                return;
            }

            Vector3[] referenceCorners = _reference.worldCorners;
            Vector3[] labelCorners = _label.worldCorners;

            Vector3 referenceBottomRight = referenceCorners != null && referenceCorners.Length > 3
                ? _parent.InverseTransformPoint(referenceCorners[3])
                : Vector3.zero;
            Vector3 labelBottomLeft = labelCorners != null && labelCorners.Length > 0
                ? _parent.InverseTransformPoint(labelCorners[0])
                : Vector3.zero;
            Vector3 labelTopRight = labelCorners != null && labelCorners.Length > 2
                ? _parent.InverseTransformPoint(labelCorners[2])
                : Vector3.zero;

            Planner.LogLine(
                "event=hud_geometry reason=" + reason +
                " language=" + PlannerLocalization.CurrentLanguage +
                " reference=" + _reference.gameObject.name +
                " parent=" + _parent.gameObject.name +
                " panel=" + _panel.gameObject.name +
                " panel_alpha=" + _panel.alpha.ToString("0.00") +
                " panel_clip=" + _panel.clipping +
                " parent_scale=" + FormatVector(_parent.lossyScale) +
                " reference_br=" + FormatVector(referenceBottomRight) +
                " label=" + FormatVector(_label.transform.localPosition) +
                " label_depth=" + _label.depth +
                " label_alpha=" + _label.alpha.ToString("0.00") +
                " label_visible=" + _label.isVisible +
                " corners_bl=" + FormatVector(labelBottomLeft) +
                " corners_tr=" + FormatVector(labelTopRight));
        }
    }

    internal static class PlannerCraftOverlay
    {
        private const string LabelName = "CraftingPlannerCraftOverlay";
        private static UILabel _label;
        private static CraftGUI _craftGui;
        private static UIPanel _panel;

        internal static void Open(CraftGUI craftGui)
        {
            if (craftGui == null)
            {
                return;
            }

            if (_label != null && _craftGui == craftGui)
            {
                Refresh();
                return;
            }

            Destroy();

            UIPanel panel = craftGui.GetComponentInParent<UIPanel>();
            UILabel reference = FindReferenceLabel(craftGui, panel);
            if (panel == null || reference == null)
            {
                Planner.LogLine("event=craft_overlay result=skipped reason=no_reference_or_panel");
                return;
            }

            _craftGui = craftGui;
            _panel = panel;
            _label = UnityEngine.Object.Instantiate(reference, panel.transform, false);
            PlannerHud.PrepareLabel(_label, LabelName, panel.gameObject.layer);

            Vector3 craftOrigin = panel.transform.InverseTransformPoint(craftGui.transform.position);
            _label.transform.localPosition = craftOrigin + new Vector3(-560f, 300f, 0f);
            Refresh();

            Planner.LogLine(
                "event=craft_overlay result=created reference=" + reference.gameObject.name +
                " panel=" + panel.gameObject.name +
                " pos=" + PlannerHud.FormatVector(_label.transform.localPosition));
        }

        internal static void Refresh()
        {
            if (_label == null)
            {
                if (GUIElements.me != null && GUIElements.me.craft != null && GUIElements.me.craft.gameObject.activeInHierarchy)
                {
                    Open(GUIElements.me.craft);
                }

                if (_label == null)
                {
                    return;
                }
            }

            bool visible = Planner.HasGoals && Planner.ShouldShowCraftOverlay && _craftGui != null && _craftGui.gameObject.activeInHierarchy;
            if (!visible)
            {
                _label.text = "";
                _label.gameObject.SetActive(false);
                return;
            }

            _label.text = PlannerHud.BuildText();
            _label.gameObject.SetActive(true);

            Planner.LogLine(
                "event=craft_overlay_refresh visible=true goals=" + Planner.GetGoals().Count +
                " active_self=" + _label.gameObject.activeSelf +
                " active_hierarchy=" + _label.gameObject.activeInHierarchy +
                " pos=" + PlannerHud.FormatVector(_label.transform.localPosition));
        }

        internal static void OnLanguageChanged()
        {
            PlannerLocalization.ApplyFont(_label);
            Refresh();
        }

        internal static void Hide()
        {
            if (_label != null)
            {
                _label.gameObject.SetActive(false);
            }
        }

        internal static void Destroy()
        {
            if (_label != null)
            {
                UnityEngine.Object.Destroy(_label.gameObject);
            }

            _label = null;
            _craftGui = null;
            _panel = null;
        }

        private static UILabel FindReferenceLabel(CraftGUI craftGui, UIPanel panel)
        {
            if (craftGui == null || panel == null)
            {
                return null;
            }

            UILabel[] labels = craftGui.GetComponentsInChildren<UILabel>(true);
            foreach (UILabel label in labels)
            {
                if (label == null || !label.gameObject.activeInHierarchy)
                {
                    continue;
                }

                UIPanel owner = label.GetComponentInParent<UIPanel>();
                if (owner == panel)
                {
                    return label;
                }
            }

            return labels.Length > 0 ? labels[0] : null;
        }
    }

    [HarmonyPatch(typeof(MainGame), "OnGameStartedPlaying")]
    internal static class SessionResetPatch
    {
        private static void Postfix()
        {
            Planner.ResetSession();
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenAsBuild", new Type[] { typeof(WorldGameObject), typeof(CraftsInventory) })]
    internal static class BuildContextPatch
    {
        private static void Prefix(WorldGameObject build_desk, CraftsInventory crafts_inventory)
        {
            Planner.OpenBuildContext(build_desk, crafts_inventory);
        }

        private static void Postfix(CraftGUI __instance)
        {
            PlannerCraftOverlay.Open(__instance);
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenCraftList", new Type[] { typeof(WorldGameObject) })]
    internal static class WorldContextPatch
    {
        private static void Prefix(WorldGameObject craftery_wgo)
        {
            Planner.OpenWorldContext(craftery_wgo);
        }

        private static void Postfix(CraftGUI __instance)
        {
            PlannerCraftOverlay.Open(__instance);
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "Hide", new Type[] { typeof(bool) })]
    internal static class CraftGuiHidePatch
    {
        private static void Postfix()
        {
            PlannerCraftOverlay.Hide();
            Planner.ClearProjectContext();
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnMouseOvered")]
    internal static class MouseFocusPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Focus(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnMouseOuted")]
    internal static class MouseBlurPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Blur(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnOver")]
    internal static class GamepadFocusPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Focus(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnOut")]
    internal static class GamepadBlurPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Blur(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(BaseGUI), "OnPressedNextSubTab")]
    internal static class AddGoalGamepadPatch
    {
        private static bool Prefix(BaseGUI __instance, ref bool __result)
        {
            if (!(__instance is CraftGUI))
            {
                return true;
            }

            if (!Planner.AdjustFocusedProject(1, "gamepad_rt"))
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(BaseGUI), "OnPressedPrevSubTab")]
    internal static class RemoveGoalGamepadPatch
    {
        private static bool Prefix(BaseGUI __instance, ref bool __result)
        {
            if (!(__instance is CraftGUI))
            {
                return true;
            }

            if (!Planner.AdjustFocusedProject(-1, "gamepad_lt"))
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameSettings), "ApplyLanguageChange")]
    internal static class LanguageChangePatch
    {
        private static void Postfix()
        {
            PlannerLocalization.OnLanguageChanged();
        }
    }

    [HarmonyPatch(typeof(HUD), "Init")]
    internal static class HudInitPatch
    {
        private static void Postfix(HUD __instance)
        {
            PlannerHud.Ensure(__instance);
        }
    }

    [HarmonyPatch(typeof(HUD), "Open")]
    internal static class HudOpenPatch
    {
        private static void Postfix(HUD __instance)
        {
            PlannerHud.Ensure(__instance);
            PlannerHud.Refresh();
        }
    }

    [HarmonyPatch(typeof(Item), "AddItem", new Type[] { typeof(Item), typeof(bool) })]
    internal static class PlayerAddItemPatch
    {
        private static void Postfix(Item __instance, bool __result)
        {
            if (__result &&
                MainGame.me != null &&
                MainGame.me.player != null &&
                __instance == MainGame.me.player.data)
            {
                PlannerHud.RefreshIfVisible();
            }
        }
    }

    [HarmonyPatch(typeof(Item), "RemoveItem", new Type[] { typeof(Item), typeof(int), typeof(Item) })]
    internal static class PlayerRemoveItemPatch
    {
        private static void Postfix(Item __instance, bool __result)
        {
            if (__result &&
                MainGame.me != null &&
                MainGame.me.player != null &&
                __instance == MainGame.me.player.data)
            {
                PlannerHud.RefreshIfVisible();
            }
        }
    }

    [HarmonyPatch(typeof(ChestGUI), "Open")]
    internal static class ChestOpenPatch
    {
        private static void Postfix(WorldGameObject chest_obj)
        {
            Planner.OpenChest(chest_obj);
        }
    }

    [HarmonyPatch(typeof(ChestGUI), "Hide")]
    internal static class ChestHidePatch
    {
        private static void Prefix()
        {
            Planner.CloseChest();
        }
    }

    [HarmonyPatch(typeof(BaseGUI), "OnPressedOption2")]
    internal static class TakeNeededGamepadPatch
    {
        private static bool Prefix(BaseGUI __instance, ref bool __result)
        {
            ChestGUI chest = __instance as ChestGUI;
            if (chest == null)
            {
                return true;
            }

            if (!Planner.TakeNeeded(chest, "gamepad_y"))
            {
                return true;
            }

            __result = true;
            return false;
        }
    }
}
