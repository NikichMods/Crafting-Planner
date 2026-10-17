// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using HarmonyLib;
namespace CraftingPlanner
{
    // One receipt per native call; never changes host inventory or world state.
    internal sealed class PlacementAttempt
    {
        internal ProjectGoal Goal;
        internal CraftDefinition Definition;
        internal MultiInventory Inventory;
        internal WorldGameObject Object;
        internal PlacementAttempt Parent;
        private int _spends, _successes;
        private bool _consumed;
        internal void ObserveSpend(MultiInventory inventory, List<Item> needs, bool success)
        {
            if (_consumed || Goal == null || Definition == null || !ReferenceEquals(Inventory, inventory) || !ReferenceEquals(Definition.needs, needs)) return;
            _spends++; if (success) _successes++;
        }
        internal bool Complete(IList<ProjectGoal> goals, bool nativeCompleted, bool becameBuilt, out ProjectGoal updated)
        {
            updated = null;
            if (_consumed) return false;
            _consumed = true;
            if (!nativeCompleted || !becameBuilt || _spends != 1 || _successes != 1) return false;
            return PlannerGoalState.AdjustPinned(goals, Goal, -1, out updated);
        }
    }

    internal static class PlannerPlacement
    {
        private static Harmony _harmony;
        [ThreadStatic] private static PlacementAttempt _active;
        private static bool _disabled, _refresh;
        internal static void Install()
        {
            _disabled = false;
            try
            {
                var target = AccessTools.Method(typeof(BuildModeLogics), "DoPlace", Type.EmptyTypes);
                var definition = AccessTools.Field(typeof(BuildModeLogics), "_cd");
                var inventory = AccessTools.Field(typeof(BuildModeLogics), "_multi_inventory");
                var spend = AccessTools.Method(typeof(MultiInventory), "RemoveItems", new[] {
                    typeof(List<Item>), typeof(MultiInventory.DestinationType), typeof(List<string>), typeof(List<Item>) });
                if (target == null || target.ReturnType != typeof(void) || definition == null || definition.IsStatic || definition.FieldType != typeof(ObjectCraftDefinition) ||
                    inventory == null || inventory.IsStatic || inventory.FieldType != typeof(MultiInventory) || spend == null || spend.ReturnType != typeof(bool))
                    throw new InvalidOperationException("Native placement contract unavailable");
                _harmony = new Harmony(Plugin.PluginGuid + ".placement");
                _harmony.Patch(target, prefix: new HarmonyMethod(typeof(PlannerPlacement), nameof(BeforePlace)), finalizer: new HarmonyMethod(typeof(PlannerPlacement), nameof(AfterPlace)));
                _harmony.Patch(spend, postfix: new HarmonyMethod(typeof(PlannerPlacement), nameof(AfterSpend)));
                Planner.LogLine("event=placement_tracking state=ready");
            }
            catch (Exception ex)
            {
                Disable("install", ex);
                if (_harmony != null) try { _harmony.UnpatchSelf(); } catch (Exception cleanup) { Warning("event=placement_unpatch error=" + cleanup); }
            }
        }
        internal static void Stop()
        {
            _disabled = true; Reset();
            if (_harmony != null) try { _harmony.UnpatchSelf(); } catch (Exception ex) { Warning("event=placement_unpatch error=" + ex); }
            _harmony = null;
        }
        internal static void Reset() { _active = null; _refresh = false; }
        private static void Disable(string phase, Exception ex)
        {
            if (_disabled) return;
            _disabled = true;
            Warning("event=placement_tracking state=disabled_for_process phase=" + phase + " error=" + ex);
        }
        private static void Warning(string details)
        { try { if (Planner.Log != null) Planner.Log.LogWarning("CRAFTING_PLANNER " + details); } catch { } }
        private static void BeforePlace(ObjectCraftDefinition ____cd, MultiInventory ____multi_inventory, out PlacementAttempt __state)
        {
            __state = null;
            if (_disabled) return;
            try
            {
                WorldGameObject builder = BuildModeLogics.last_build_desk;
                ProjectGoal goal = Planner.FindPlacement(____cd, builder != null ? builder.obj_id : null);
                var floating = FloatingWorldGameObject.cur_floating;
                WorldGameObject obj = floating != null ? floating.wobj : null;
                if (obj == null || obj.just_built || ____multi_inventory == null) goal = null;
                if (goal == null && _active == null) return;
                // A nested untracked call suspends its parent rather than attributing its spend to it.
                __state = new PlacementAttempt { Goal = goal, Definition = ____cd, Inventory = ____multi_inventory, Object = obj, Parent = _active };
                _active = __state;
            }
            catch (Exception ex) { Disable("capture", ex); }
        }
        private static void AfterSpend(MultiInventory __instance, List<Item> __0, bool __result)
        {
            if (_disabled || _active == null) return;
            try { _active.ObserveSpend(__instance, __0, __result); }
            catch (Exception ex) { Disable("spend", ex); }
        }
        private static Exception AfterPlace(Exception __exception, PlacementAttempt __state)
        {
            if (__state == null) return __exception;
            try
            {
                if (!_disabled && Planner.CompletePlacement(__state, __exception == null, __state.Object != null && __state.Object.just_built)) _refresh = true;
            }
            catch (Exception ex) { Disable("complete", ex); }
            finally { _active = __state.Parent; }
            return __exception;
        }
        internal static void Flush()
        {
            if (!_refresh) return;
            _refresh = false;
            try { PlannerRowControls.RefreshAll(); PlannerPresentation.Refresh(); }
            catch (Exception ex) { Disable("refresh", ex); }
        }
    }
}
