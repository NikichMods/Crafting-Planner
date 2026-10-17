// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using HarmonyLib;
namespace CraftingPlanner
{
    internal sealed class WorldCompletionAttempt
    {
        internal ProjectGoal Goal;
        internal CraftDefinition Definition;
        internal long ObjectUid;
        internal WorldCompletionAttempt Parent;
        private int _markers, _successes;
        private bool _consumed;
        internal void ObserveCompletion(CraftDefinition definition, bool success)
        {
            if (_consumed || Goal == null || !ReferenceEquals(Definition, definition)) return;
            _markers++; if (success) _successes++;
        }
        internal bool Complete(IList<ProjectGoal> goals, bool nativeCompleted, out ProjectGoal updated)
        {
            updated = null;
            if (_consumed) return false;
            _consumed = true;
            if (!nativeCompleted || _markers != 1 || _successes != 1) return false;
            return PlannerGoalState.AdjustPinned(goals, Goal, -1, out updated);
        }
    }
    internal static class PlannerWorldCompletion
    {
        private static Harmony _harmony;
        [ThreadStatic] private static WorldCompletionAttempt _active;
        private static bool _disabled, _refresh;
        internal static void Install()
        {
            _disabled = false;
            try
            {
                var finish = AccessTools.Method(typeof(CraftComponent), "ProcessFinishedCraft", Type.EmptyTypes);
                var marker = AccessTools.Method(typeof(GameSave), "OnFinishedCraft", new[] {typeof(CraftDefinition)});
                if (finish == null || finish.IsStatic || finish.ReturnType != typeof(void) || marker == null || marker.IsStatic || marker.ReturnType != typeof(void))
                    throw new InvalidOperationException("Native world completion contract unavailable");
                _harmony = new Harmony(Plugin.PluginGuid + ".worldcompletion");
                _harmony.Patch(finish, prefix: new HarmonyMethod(typeof(PlannerWorldCompletion), nameof(BeforeFinish)), finalizer: new HarmonyMethod(typeof(PlannerWorldCompletion), nameof(AfterFinish)));
                _harmony.Patch(marker, finalizer: new HarmonyMethod(typeof(PlannerWorldCompletion), nameof(AfterMarker)));
                Planner.LogLine("event=world_completion_tracking state=ready");
            }
            catch (Exception ex)
            {
                Disable("install", ex);
                if (_harmony != null) try { _harmony.UnpatchSelf(); } catch (Exception cleanup) { Warning("event=world_completion_unpatch error=" + cleanup); }
            }
        }
        internal static void Stop()
        {
            _disabled = true; Reset();
            if (_harmony != null) try { _harmony.UnpatchSelf(); } catch (Exception ex) { Warning("event=world_completion_unpatch error=" + ex); }
            _harmony = null;
        }
        internal static void Reset() { _active = null; _refresh = false; }
        private static void Warning(string text)
        { try { if (Planner.Log != null) Planner.Log.LogWarning("CRAFTING_PLANNER " + text); } catch { } }
        private static void Disable(string phase, Exception ex)
        {
            if (_disabled) return;
            _disabled = true;
            Warning("event=world_completion_tracking state=disabled_for_process phase=" + phase + " error=" + ex);
        }
        private static void BeforeFinish(CraftComponent __instance, out WorldCompletionAttempt __state)
        {
            __state = null;
            if (_disabled) return;
            try
            {
                CraftDefinition definition = __instance.current_craft;
                WorldGameObject owner = __instance.wgo;
                ProjectGoal goal = owner != null && __instance.is_crafting ? Planner.FindWorldCompletion(definition, owner.obj_id) : null;
                if (goal == null && _active == null) return;
                // Capture before replacement/DestroyMe; a nested untracked finish masks its parent.
                __state = new WorldCompletionAttempt {Goal=goal, Definition=definition, ObjectUid=owner != null ? owner.unique_id : 0, Parent=_active};
                _active = __state;
            }
            catch (Exception ex) { Disable("capture", ex); }
        }
        private static Exception AfterMarker(CraftDefinition __0, Exception __exception)
        {
            if (!_disabled && _active != null)
            {
                try { _active.ObserveCompletion(__0, __exception == null); }
                catch (Exception ex) { Disable("marker", ex); }
            }
            return __exception;
        }
        private static Exception AfterFinish(Exception __exception, WorldCompletionAttempt __state)
        {
            if (__state == null) return __exception;
            try { if (!_disabled && Planner.CompleteWorldProject(__state, __exception == null)) _refresh = true; }
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
