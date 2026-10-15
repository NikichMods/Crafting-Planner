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
        public const string PluginVersion = "0.1.5";

        private Harmony _harmony;

        private void Awake()
        {
            Planner.Log = Logger;
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            LazyInput.on_input_changed += PlannerInputHints.OnInputChanged;
            BaseGUI.on_window_opened += PlannerInputHints.OnWindowChanged;
            BaseGUI.on_window_closed += PlannerInputHints.OnWindowChanged;
            Logger.LogInfo("CRAFTING_PLANNER event=loaded version=" + PluginVersion);
        }

        private void OnDestroy()
        {
            LazyInput.on_input_changed -= PlannerInputHints.OnInputChanged;
            BaseGUI.on_window_opened -= PlannerInputHints.OnWindowChanged;
            BaseGUI.on_window_closed -= PlannerInputHints.OnWindowChanged;
            PlannerInputHints.Destroy();
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

        internal static bool CanAdjustFocusedProject
        {
            get { return _focusedProject != null && IsSupportedProject(_focusedProject); }
        }

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
        private static readonly Dictionary<string, string[]> Texts = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", new[] { "Plan", "Have", "Need", "Missing", "Take needed" } },
            { "de", new[] { "Plan", "Da", "Bedarf", "Fehlt", "Benötigtes nehmen" } },
            { "fr", new[] { "Plan", "En sac", "Requis", "Manque", "Prendre le nécessaire" } },
            { "pt-br", new[] { "Plano", "Tenho", "Preciso", "Falta", "Pegar o necessário" } },
            { "es", new[] { "Plan", "Tengo", "Necesito", "Falta", "Tomar lo necesario" } },
            { "ru", new[] { "План", "Есть", "Нужно", "Добрать", "Взять недостающее" } },
            { "it", new[] { "Piano", "Ho", "Serve", "Manca", "Prendi il necessario" } },
            { "pl", new[] { "Plan", "Mam", "Potrzeba", "Brakuje", "Weź potrzebne" } },
            { "ja", new[] { "計画", "所持", "必要", "不足", "不足分を取る" } },
            { "zh-cn", new[] { "计划", "持有", "需要", "缺少", "拿取所需材料" } },
            { "ko", new[] { "계획", "보유", "필요", "부족", "필요한 재료 가져오기" } }
        };
        internal static string Plan { get { return Current[0]; } }
        internal static string Have { get { return Current[1]; } }
        internal static string Required { get { return Current[2]; } }
        internal static string Missing { get { return Current[3]; } }
        internal static string TakeNeeded { get { return Current[4]; } }

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
            PlannerInputHints.Refresh();
            Planner.LogLine("event=language_changed language=" + CurrentLanguage);
        }

        private static string[] Current
        {
            get
            {
                string[] value;
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
            PlannerInputHints.Refresh();
        }
    }

    [HarmonyPatch(typeof(ChestGUI), "Hide")]
    internal static class ChestHidePatch
    {
        private static void Prefix()
        {
            Planner.CloseChest();
            PlannerInputHints.HideChest();
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
