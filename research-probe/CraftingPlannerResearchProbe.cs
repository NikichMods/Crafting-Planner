using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CraftingPlannerResearchProbe
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.gyk.craftingplanner.researchprobe";
        public const string PluginName = "Crafting Planner Research Probe";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;

        private void Awake()
        {
            ResearchState.Log = Logger;
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            Logger.LogInfo("CRAFTING_PLANNER_PROBE event=loaded version=" + PluginVersion);
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
        }
    }

    internal static class ResearchState
    {
        internal static ManualLogSource Log;
        internal static CraftDefinition LastRecipe;
        internal static string LastRecipeKind = "none";
        internal static WorldGameObject CurrentChest;

        internal static void CaptureRecipe(CraftDefinition craft, string kind)
        {
            if (craft == null)
            {
                return;
            }

            LastRecipe = craft;
            LastRecipeKind = kind;
            LogLine(
                "event=recipe_focus kind=" + kind +
                " id=" + Safe(craft.id) +
                " type=" + ((int)craft.craft_type) +
                " object_craft=" + (craft is ObjectCraftDefinition) +
                " needs=" + DescribeNeeds(craft.needs) +
                " needs_from_wgo=" + DescribeNeeds(craft.needs_from_wgo));
        }

        internal static void OpenChest(WorldGameObject chest)
        {
            CurrentChest = chest;
            LogLine("event=chest_open chest=" + Safe(chest != null ? chest.obj_id : null));
        }

        internal static void CloseChest()
        {
            LogLine("event=chest_close chest=" + Safe(CurrentChest != null ? CurrentChest.obj_id : null));
            CurrentChest = null;
        }

        internal static bool RunTransferProbe(ChestGUI chestGui, string trigger)
        {
            if (CurrentChest == null || LastRecipe == null || MainGame.me == null || MainGame.me.player == null)
            {
                LogLine(
                    "event=take_needed result=skipped trigger=" + trigger +
                    " reason=missing_context chest=" + (CurrentChest != null) +
                    " recipe=" + (LastRecipe != null) +
                    " player=" + (MainGame.me != null && MainGame.me.player != null));
                return false;
            }

            if (LastRecipe.craft_type != CraftDefinition.CraftType.None)
            {
                LogLine(
                    "event=take_needed result=skipped trigger=" + trigger +
                    " reason=unsupported_craft_type id=" + Safe(LastRecipe.id) +
                    " type=" + ((int)LastRecipe.craft_type));
                return false;
            }

            if (LastRecipe.needs_from_wgo != null && LastRecipe.needs_from_wgo.Count > 0)
            {
                LogLine(
                    "event=take_needed result=skipped trigger=" + trigger +
                    " reason=needs_from_wgo id=" + Safe(LastRecipe.id));
                return false;
            }

            if (LastRecipe.needs == null || LastRecipe.needs.Count == 0)
            {
                LogLine(
                    "event=take_needed result=skipped trigger=" + trigger +
                    " reason=no_needs id=" + Safe(LastRecipe.id));
                return false;
            }

            foreach (Item need in LastRecipe.needs)
            {
                if (need != null && need.multiquality_items != null && need.multiquality_items.Count > 0)
                {
                    LogLine(
                        "event=take_needed result=skipped trigger=" + trigger +
                        " reason=multiquality id=" + Safe(LastRecipe.id) +
                        " item=" + Safe(need.id));
                    return false;
                }
            }

            WorldGameObject player = MainGame.me.player;
            MultiInventory source = CurrentChest.GetMultiInventoryOfWGOWithoutWorldZone(true);
            MultiInventory target = player.GetMultiInventoryOfWGOWithoutWorldZone(true);
            MultiInventory interactionInventory = player.GetMultiInventoryForInteraction(null);

            if (source == null || target == null)
            {
                LogLine(
                    "event=take_needed result=failed trigger=" + trigger +
                    " reason=null_inventory id=" + Safe(LastRecipe.id));
                return false;
            }

            Dictionary<string, int> required = AggregateNeeds(LastRecipe.needs);
            bool movedAnything = false;

            foreach (KeyValuePair<string, int> pair in required)
            {
                string itemId = pair.Key;
                int requiredCount = pair.Value;
                int haveBefore = player.data.GetTotalCount(itemId, true);
                int interactionHave = interactionInventory != null
                    ? interactionInventory.GetTotalCount(itemId, MultiInventory.DestinationType.AllFromFirst, true)
                    : -1;
                int missing = Math.Max(requiredCount - haveBefore, 0);
                int available = source.GetTotalCount(itemId, MultiInventory.DestinationType.AllFromFirst, true);
                int capacity = target.CanAddCount(itemId, true);
                int requested = Math.Min(missing, Math.Min(available, capacity));
                bool success = true;

                if (requested > 0)
                {
                    Item transferRequest = new Item(itemId, requested);
                    success = source.MoveItemTo(target, transferRequest, requested, false, true);
                    movedAnything |= success;
                }

                int haveAfter = player.data.GetTotalCount(itemId, true);
                int storageAfter = source.GetTotalCount(itemId, MultiInventory.DestinationType.AllFromFirst, true);

                LogLine(
                    "event=take_item trigger=" + trigger +
                    " recipe=" + Safe(LastRecipe.id) +
                    " kind=" + LastRecipeKind +
                    " item=" + Safe(itemId) +
                    " required=" + requiredCount +
                    " have_before=" + haveBefore +
                    " interaction_have=" + interactionHave +
                    " missing=" + missing +
                    " storage_before=" + available +
                    " capacity=" + capacity +
                    " requested=" + requested +
                    " success=" + success +
                    " have_after=" + haveAfter +
                    " storage_after=" + storageAfter);
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

            LogLine(
                "event=take_needed result=complete trigger=" + trigger +
                " recipe=" + Safe(LastRecipe.id) +
                " moved_any=" + movedAnything);

            return true;
        }

        private static Dictionary<string, int> AggregateNeeds(List<Item> needs)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (Item item in needs)
            {
                if (item == null || string.IsNullOrEmpty(item.id) || item.value <= 0)
                {
                    continue;
                }

                int current;
                result.TryGetValue(item.id, out current);
                result[item.id] = current + item.value;
            }

            return result;
        }

        private static string DescribeNeeds(List<Item> needs)
        {
            if (needs == null || needs.Count == 0)
            {
                return "none";
            }

            StringBuilder sb = new StringBuilder();
            foreach (Item item in needs)
            {
                if (item == null)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(Safe(item.id));
                sb.Append(":");
                sb.Append(item.value);
                if (item.multiquality_items != null && item.multiquality_items.Count > 0)
                {
                    sb.Append(":mq");
                }
            }

            return sb.Length == 0 ? "none" : sb.ToString();
        }

        private static string Safe(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "null";
            }

            return value.Replace(" ", "_").Replace("=", "_");
        }

        internal static void LogLine(string details)
        {
            if (Log != null)
            {
                Log.LogInfo("CRAFTING_PLANNER_PROBE " + details);
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnOver")]
    internal static class CraftItemGuiOnOverPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                ResearchState.CaptureRecipe(__instance.current_craft, "craft");
            }
        }
    }

    [HarmonyPatch(typeof(BuildItemGUI), "OnOver")]
    internal static class BuildItemGuiOnOverPatch
    {
        private static void Postfix(BuildItemGUI __instance)
        {
            if (__instance != null)
            {
                ResearchState.CaptureRecipe(__instance.definition, "build");
            }
        }
    }

    [HarmonyPatch(typeof(ChestGUI), "Open")]
    internal static class ChestGuiOpenPatch
    {
        private static void Postfix(WorldGameObject chest_obj)
        {
            ResearchState.OpenChest(chest_obj);
        }
    }

    [HarmonyPatch(typeof(ChestGUI), "Hide")]
    internal static class ChestGuiHidePatch
    {
        private static void Prefix()
        {
            ResearchState.CloseChest();
        }
    }

    [HarmonyPatch(typeof(BaseGUI), "OnPressedOption2")]
    internal static class ChestOption2Patch
    {
        private static bool Prefix(BaseGUI __instance, ref bool __result)
        {
            ChestGUI chest = __instance as ChestGUI;
            if (chest == null || ResearchState.CurrentChest == null)
            {
                return true;
            }

            bool handled = ResearchState.RunTransferProbe(chest, "gamepad_option2");
            if (!handled)
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(ChestGUI), "Update")]
    internal static class ChestKeyboardProbePatch
    {
        private static void Prefix(ChestGUI __instance)
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ResearchState.RunTransferProbe(__instance, "keyboard_f8");
            }
        }
    }
}
