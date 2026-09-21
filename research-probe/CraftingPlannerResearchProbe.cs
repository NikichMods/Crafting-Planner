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
        public const string PluginVersion = "0.2.0";

        private void Awake()
        {
            ResearchState.Log = Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();

            ResearchState.LogLine("event=plugin_loaded version=" + PluginVersion + " purpose=build_world_projects");
        }
    }

    internal static class ResearchState
    {
        internal static ManualLogSource Log;

        internal static bool BuildMenuOpen;
        internal static WorldGameObject LastBuildDesk;
        internal static WorldGameObject LastInteractedTarget;
        internal static float LastPlayerInteractionTime = -1000f;

        private static string _lastFocusSignature = "";

        internal static void EnterBuildMenu(WorldGameObject buildDesk, CraftsInventory craftsInventory)
        {
            BuildMenuOpen = true;
            LastBuildDesk = buildDesk;
            _lastFocusSignature = "";

            List<ObjectCraftDefinition> definitions = null;
            try
            {
                definitions = craftsInventory != null ? craftsInventory.GetObjectCraftsList() : null;
            }
            catch (Exception ex)
            {
                LogLine(
                    "event=build_menu_open result=partial builder=" + ObjId(buildDesk) +
                    " error=" + Safe(ex.GetType().Name));
            }

            int count = definitions != null ? definitions.Count : -1;
            LogLine(
                "event=build_menu_open result=ok builder=" + ObjId(buildDesk) +
                " project_count=" + count);

            if (definitions == null)
            {
                return;
            }

            foreach (ObjectCraftDefinition definition in definitions)
            {
                LogDefinition("build_menu_list", definition, "builder=" + ObjId(buildDesk));
            }
        }

        internal static void MarkNormalCraftMenu(WorldGameObject craftery)
        {
            BuildMenuOpen = false;
            _lastFocusSignature = "";
            LogLine("event=craft_menu_open kind=normal owner=" + ObjId(craftery));
        }

        internal static void ExitCraftGui()
        {
            if (BuildMenuOpen)
            {
                LogLine("event=build_menu_close builder=" + ObjId(LastBuildDesk));
            }

            BuildMenuOpen = false;
            _lastFocusSignature = "";
        }

        internal static void CaptureBuildCard(CraftDefinition craft, string source)
        {
            if (!BuildMenuOpen || craft == null)
            {
                return;
            }

            LogDefinition(source, craft, "builder=" + ObjId(LastBuildDesk));
        }

        internal static void CaptureBuildFocus(CraftDefinition craft, string input)
        {
            if (!BuildMenuOpen || craft == null)
            {
                return;
            }

            string signature = input + "|" + ObjId(LastBuildDesk) + "|" + Safe(craft.id);
            if (signature == _lastFocusSignature)
            {
                return;
            }

            _lastFocusSignature = signature;
            LogDefinition("build_focus", craft, "input=" + input + " builder=" + ObjId(LastBuildDesk));
        }

        internal static void CaptureBuildSelection(CraftDefinition craft)
        {
            if (craft == null)
            {
                LogLine("event=build_select result=null");
                return;
            }

            LogDefinition("build_selected", craft, "builder=" + ObjId(LastBuildDesk));
        }

        internal static void CapturePlayerInteraction(WorldGameObject target, WorldGameObject actor, bool interactionStart)
        {
            if (!interactionStart || target == null || actor == null || !actor.is_player)
            {
                return;
            }

            LastInteractedTarget = target;
            LastPlayerInteractionTime = Time.realtimeSinceStartup;

            ObjectDefinition definition = target.obj_def;
            ObjectInteractionDefinition validInteraction = null;

            if (definition != null)
            {
                try
                {
                    validInteraction = definition.GetValidInteraction(target);
                }
                catch (Exception ex)
                {
                    LogLine(
                        "event=interaction_probe target=" + ObjId(target) +
                        " result=valid_interaction_error error=" + Safe(ex.GetType().Name));
                }
            }

            LogLine(
                "event=player_interaction target=" + ObjId(target) +
                " interaction_type=" + (definition != null ? ((int)definition.interaction_type).ToString() : "null") +
                " has_craft=" + (definition != null && definition.has_craft) +
                " check_only_interactions=" + (definition != null && definition.check_only_interactions) +
                " hint=" + Safe(validInteraction != null ? validInteraction.hint : null) +
                " script=" + Safe(validInteraction != null ? validInteraction.script : null) +
                " attached_script=" + Safe(definition != null ? definition.attached_script : null) +
                " work=" + Safe(definition != null ? definition.work : null) +
                " craft_after_hp0=" + Safe(definition != null ? definition.craft_after_hp_0 : null) +
                " script_after_hp0=" + Safe(definition != null ? definition.script_after_hp_0 : null));

            if (definition != null &&
                (definition.has_craft || definition.interaction_type == ObjectDefinition.InteractionType.Craft))
            {
                LogTargetCrafts(target, "interaction_target");
            }
        }

        internal static void CaptureCraftGuiOpen(WorldGameObject target)
        {
            if (target == null)
            {
                return;
            }

            LogLine("event=craft_gui_open owner=" + ObjId(target));
            LogTargetCrafts(target, "craft_gui_list");
        }

        internal static void CaptureTryStartCraft(WorldGameObject target, string craftName)
        {
            if (target == null)
            {
                return;
            }

            if (target != LastInteractedTarget && !IsWithinRecentPlayerInteraction())
            {
                return;
            }

            LogLine(
                "event=try_start_craft target=" + ObjId(target) +
                " craft=" + Safe(craftName) +
                " recent_interaction=" + IsWithinRecentPlayerInteraction());
        }

        internal static void CaptureCraftStart(CraftComponent component, CraftDefinition craft, int amount)
        {
            if (component == null || craft == null)
            {
                return;
            }

            WorldGameObject owner = component.wgo;
            bool relevant =
                owner == LastInteractedTarget ||
                owner == LastBuildDesk ||
                IsWithinRecentPlayerInteraction() ||
                IsProjectLike(craft);

            if (!relevant)
            {
                return;
            }

            LogDefinition(
                "craft_start",
                craft,
                "owner=" + ObjId(owner) +
                " amount=" + amount +
                " owner_is_last_interaction=" + (owner == LastInteractedTarget) +
                " owner_is_builder=" + (owner == LastBuildDesk));
        }

        private static bool IsProjectLike(CraftDefinition craft)
        {
            if (craft == null)
            {
                return false;
            }

            return craft is ObjectCraftDefinition ||
                   craft.one_time_craft ||
                   !string.IsNullOrEmpty(craft.change_wgo) ||
                   !string.IsNullOrEmpty(craft.end_script) ||
                   !string.IsNullOrEmpty(craft.end_event);
        }

        private static bool IsWithinRecentPlayerInteraction()
        {
            return Time.realtimeSinceStartup - LastPlayerInteractionTime <= 5f;
        }

        private static void LogTargetCrafts(WorldGameObject target, string source)
        {
            try
            {
                ComponentsManager components = target.components;
                CraftComponent craftComponent = components != null ? components.craft : null;
                List<CraftDefinition> crafts = craftComponent != null ? craftComponent.crafts : null;

                LogLine(
                    "event=target_crafts source=" + source +
                    " target=" + ObjId(target) +
                    " count=" + (crafts != null ? crafts.Count : -1));

                if (crafts == null)
                {
                    return;
                }

                foreach (CraftDefinition craft in crafts)
                {
                    LogDefinition(source, craft, "owner=" + ObjId(target));
                }
            }
            catch (Exception ex)
            {
                LogLine(
                    "event=target_crafts source=" + source +
                    " target=" + ObjId(target) +
                    " result=error error=" + Safe(ex.GetType().Name));
            }
        }

        internal static void LogDefinition(string source, CraftDefinition craft, string extra)
        {
            if (craft == null)
            {
                LogLine("event=definition source=" + source + " result=null " + extra);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("event=definition");
            sb.Append(" source=").Append(Safe(source));
            sb.Append(" id=").Append(Safe(craft.id));
            sb.Append(" craft_type=").Append((int)craft.craft_type);
            sb.Append(" needs=").Append(DescribeItems(craft.needs));
            sb.Append(" needs_wgo=").Append(DescribeItems(craft.needs_from_wgo));
            sb.Append(" output=").Append(DescribeItems(craft.output));
            sb.Append(" one_time=").Append(craft.one_time_craft);
            sb.Append(" hidden=").Append(craft.hidden);
            sb.Append(" change_wgo=").Append(Safe(craft.change_wgo));
            sb.Append(" end_script=").Append(Safe(craft.end_script));
            sb.Append(" end_event=").Append(Safe(craft.end_event));
            sb.Append(" craft_after=").Append(Safe(craft.craft_after_finish));

            ObjectCraftDefinition objectCraft = craft as ObjectCraftDefinition;
            if (objectCraft != null)
            {
                sb.Append(" object_craft=True");
                sb.Append(" build_type=").Append((int)objectCraft.build_type);
                sb.Append(" out_obj=").Append(Safe(objectCraft.out_obj));
                sb.Append(" wait_script_callback=").Append(objectCraft.wait_script_callback);
                sb.Append(" enabled=").Append(objectCraft.enabled);
                sb.Append(" builders=").Append(DescribeStrings(objectCraft.builder_ids));
                sb.Append(" locked_builders=").Append(DescribeStrings(objectCraft.locked_builders_ids));
            }
            else
            {
                sb.Append(" object_craft=False");
            }

            if (!string.IsNullOrEmpty(extra))
            {
                sb.Append(" ").Append(extra);
            }

            LogLine(sb.ToString());
        }

        private static string DescribeItems(List<Item> items)
        {
            if (items == null || items.Count == 0)
            {
                return "none";
            }

            StringBuilder sb = new StringBuilder();
            foreach (Item item in items)
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

        private static string DescribeStrings(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "none";
            }

            StringBuilder sb = new StringBuilder();
            foreach (string value in values)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(Safe(value));
            }

            return sb.ToString();
        }

        private static string ObjId(WorldGameObject wgo)
        {
            return wgo == null ? "null" : Safe(wgo.obj_id);
        }

        private static string Safe(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "null";
            }

            return value
                .Replace(" ", "_")
                .Replace("\r", "_")
                .Replace("\n", "_")
                .Replace("\t", "_")
                .Replace("=", "_")
                .Replace("|", "_");
        }

        internal static void LogLine(string details)
        {
            if (Log != null)
            {
                Log.LogInfo("CRAFTING_PLANNER_PROBE " + details);
            }
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenAsBuild", new Type[] { typeof(WorldGameObject), typeof(CraftsInventory) })]
    internal static class CraftGuiOpenAsBuildPatch
    {
        private static void Prefix(WorldGameObject build_desk, CraftsInventory crafts_inventory)
        {
            ResearchState.EnterBuildMenu(build_desk, crafts_inventory);
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenCraftList", new Type[] { typeof(WorldGameObject) })]
    internal static class CraftGuiOpenCraftListPatch
    {
        private static void Prefix(WorldGameObject craftery_wgo)
        {
            ResearchState.MarkNormalCraftMenu(craftery_wgo);
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "Hide", new Type[] { typeof(bool) })]
    internal static class CraftGuiHidePatch
    {
        private static void Postfix()
        {
            ResearchState.ExitCraftGui();
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "Draw", new Type[] { typeof(CraftDefinition) })]
    internal static class CraftItemGuiDrawPatch
    {
        private static void Postfix(CraftDefinition craft_definition)
        {
            ResearchState.CaptureBuildCard(craft_definition, "build_card_draw");
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnMouseOvered")]
    internal static class CraftItemGuiMouseOverPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                ResearchState.CaptureBuildFocus(__instance.current_craft, "mouse");
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnOver")]
    internal static class CraftItemGuiGamepadOverPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                ResearchState.CaptureBuildFocus(__instance.current_craft, "gamepad");
            }
        }
    }

    [HarmonyPatch(typeof(BuildModeLogics), "CraftBuilding", new Type[] { typeof(CraftDefinition) })]
    internal static class BuildModeCraftBuildingPatch
    {
        private static void Prefix(CraftDefinition craft)
        {
            ResearchState.CaptureBuildSelection(craft);
        }
    }

    [HarmonyPatch(typeof(WorldGameObject), "Interact", new Type[] { typeof(WorldGameObject), typeof(bool), typeof(float) })]
    internal static class WorldGameObjectInteractPatch
    {
        private static void Prefix(WorldGameObject __instance, WorldGameObject other_obj, bool interaction_start)
        {
            ResearchState.CapturePlayerInteraction(__instance, other_obj, interaction_start);
        }
    }

    [HarmonyPatch(typeof(GUIElements), "OpenCraftGUI", new Type[] { typeof(WorldGameObject) })]
    internal static class GuiElementsOpenCraftGuiPatch
    {
        private static void Prefix(WorldGameObject craftery_wgo)
        {
            ResearchState.CaptureCraftGuiOpen(craftery_wgo);
        }
    }

    [HarmonyPatch(typeof(WorldGameObject), "TryStartCraft", new Type[] { typeof(string) })]
    internal static class WorldGameObjectTryStartCraftPatch
    {
        private static void Prefix(WorldGameObject __instance, string craft_name)
        {
            ResearchState.CaptureTryStartCraft(__instance, craft_name);
        }
    }

    [HarmonyPatch(
        typeof(CraftComponent),
        "Craft",
        new Type[]
        {
            typeof(CraftDefinition),
            typeof(Item),
            typeof(List<string>),
            typeof(List<Item>),
            typeof(bool),
            typeof(int)
        })]
    internal static class CraftComponentCraftPatch
    {
        private static void Prefix(CraftComponent __instance, CraftDefinition craft, int amount)
        {
            ResearchState.CaptureCraftStart(__instance, craft, amount);
        }
    }
}
