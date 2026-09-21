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
        public const string PluginVersion = "0.1.0";

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
        internal string DisplayName;
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
        }

        internal static void Blur(CraftDefinition definition)
        {
            if (_focusedProject == definition)
            {
                _focusedProject = null;
            }
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
                        DisplayName = GetProjectName(definition),
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

        private static string GetProjectName(CraftDefinition definition)
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

    internal static class PlannerHud
    {
        private const string LabelName = "CraftingPlannerHUD";
        private static UILabel _label;
        private static HUD _hud;

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

            UILabel reference = hud.zone_descr != null ? hud.zone_descr : hud.version_label;
            if (reference == null)
            {
                Planner.LogLine("event=hud result=skipped reason=no_reference_label");
                return;
            }

            _hud = hud;
            _label = UnityEngine.Object.Instantiate(reference, hud.transform, false);
            _label.gameObject.name = LabelName;
            _label.transform.localScale = Vector3.one;
            _label.pivot = UIWidget.Pivot.TopLeft;
            _label.width = 620;
            Reposition();
            Refresh();
            Planner.LogLine("event=hud result=created");
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

            _label.gameObject.SetActive(true);
            _label.text = BuildText();
            Reposition();
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

        internal static void Destroy()
        {
            if (_label != null)
            {
                UnityEngine.Object.Destroy(_label.gameObject);
            }

            _label = null;
            _hud = null;
        }

        private static string BuildText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Crafting Planner\n");
            sb.Append("Проекты:\n");

            foreach (ProjectGoal goal in Planner.GetGoals())
            {
                sb.Append("• ");
                sb.Append(goal.DisplayName);
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

            sb.Append("\nМатериалы — Нужно / Есть / Не хватает:\n");
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

        private static void Reposition()
        {
            if (_label == null || _hud == null)
            {
                return;
            }

            UIRoot root = _hud.GetComponentInParent<UIRoot>();
            float h = root != null && root.activeHeight > 0 ? root.activeHeight : 720f;
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            float w = h * aspect;

            _label.transform.localPosition = new Vector3(-w * 0.5f + 24f, h * 0.5f - 105f, 0f);
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
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenCraftList", new Type[] { typeof(WorldGameObject) })]
    internal static class WorldContextPatch
    {
        private static void Prefix(WorldGameObject craftery_wgo)
        {
            Planner.OpenWorldContext(craftery_wgo);
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "Hide", new Type[] { typeof(bool) })]
    internal static class CraftGuiHidePatch
    {
        private static void Postfix()
        {
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

    [HarmonyPatch(typeof(CraftGUI), "Update")]
    internal static class CraftKeyboardPatch
    {
        private static void Postfix(CraftGUI __instance)
        {
            if (__instance == null || BaseGUI.opened_windows == null || BaseGUI.opened_windows.Count == 0)
            {
                return;
            }

            if (BaseGUI.opened_windows[BaseGUI.opened_windows.Count - 1] != __instance)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                Planner.AdjustFocusedProject(1, "keyboard_plus");
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                Planner.AdjustFocusedProject(-1, "keyboard_minus");
            }
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
