// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using HarmonyLib;
namespace CraftingPlanner
{
    // A preview's native craft finish precedes the player's decision. These latches
    // accept only one correlated Apply, normal script end and normal decision return.
    internal sealed class InteriorCompletionAttempt
    {
        internal ProjectGoal Goal;
        internal CraftDefinition Definition;
        internal GameSave Save;
        internal long ObjectUid;
        internal object Script;
        internal int Decisions;
        private int _dispatches, _finishes, _markers, _scriptCalls, _terminations, _returns, _applies;
        private bool _failed, _consumed;
        internal void ObserveDispatch(bool normal) { _dispatches++; _failed |= !normal; }
        internal void ObserveFinish(bool normal) { _finishes++; _failed |= !normal; }
        internal void ObserveMarker(CraftDefinition definition, bool normal)
        { if (ReferenceEquals(Definition, definition)) { _markers++; _failed |= !normal; } }
        internal void CaptureScript(object script)
        {
            if (script == null || (Script != null && !ReferenceEquals(Script, script))) _failed = true;
            else Script = script;
        }
        internal void ObserveScriptCall(bool normal) { _scriptCalls++; _failed |= !normal; }
        internal void ObserveDecision(bool apply) { Decisions++; if (apply) _applies++; else _failed = true; }
        internal void ObserveDecisionReturn(bool normal) { _returns++; _failed |= !normal; }
        internal void ObserveTermination(object script, bool normal)
        { if (Script != null && ReferenceEquals(Script, script)) { _terminations++; _failed |= !normal; } }
        internal bool Complete(IList<ProjectGoal> goals, out ProjectGoal updated)
        {
            updated = null;
            if (_consumed || _failed || Script == null || _dispatches != 1 || _finishes != 1 || _markers != 1 ||
                _scriptCalls != 1 || _terminations != 1 || _returns != 1 || Decisions != 1 || _applies != 1) return false;
            _consumed = true;
            return PlannerGoalState.AdjustPinned(goals, Goal, -1, out updated);
        }
    }
    internal static class PlannerInteriorCompletion
    {
        private static Harmony _harmony;
        private static InteriorCompletionAttempt _pending;
        [ThreadStatic] private static FinishScope _finishing;
        private static bool _disabled, _refresh;
        private sealed class FinishScope { internal InteriorCompletionAttempt Attempt; internal FinishScope Parent; }
        private struct DecisionCall { internal InteriorCompletionAttempt Attempt; internal int Before; }
        internal static void Install()
        {
            _disabled = false;
            try
            {
                _harmony = new Harmony(Plugin.PluginGuid + ".interiorcompletion");
                Bind(typeof(BuildModeLogics), "CraftBuilding", new[] {typeof(CraftDefinition)}, nameof(BeforeBuilder), nameof(AfterBuilder));
                Bind(typeof(CraftComponent), "ProcessFinishedCraft", Type.EmptyTypes, nameof(BeforeFinish), nameof(AfterFinish));
                Bind(typeof(GameSave), "OnFinishedCraft", new[] {typeof(CraftDefinition)}, null, nameof(AfterMarker));
                Bind(typeof(CustomFlowScript), "FireEvent", new[] {typeof(string), typeof(string)}, nameof(BeforeScript), nameof(AfterScript));
                Bind(typeof(CustomFlowScript), "TerminateMe", Type.EmptyTypes, nameof(BeforeTermination), nameof(AfterTermination));
                Bind(typeof(BuildModeLogics), "UpdateWhileScriptBuilding", Type.EmptyTypes, nameof(BeforeDecisionCall), nameof(AfterDecisionCall));
                BuildModeLogics.on_apply_while_script_building += OnApply;
                BuildModeLogics.on_cancel_while_script_building += OnCancel;
                Planner.LogLine("event=interior_completion_tracking state=ready");
            }
            catch (Exception ex) { Disable("install", ex); Detach(); }
        }
        private static void Bind(Type owner, string name, Type[] args, string prefix, string finalizer)
        {
            var method = AccessTools.Method(owner, name, args);
            if (method == null || method.IsStatic || method.ReturnType != typeof(void)) throw new InvalidOperationException(owner.Name + "." + name);
            _harmony.Patch(method, prefix: prefix == null ? null : new HarmonyMethod(typeof(PlannerInteriorCompletion), prefix),
                finalizer: new HarmonyMethod(typeof(PlannerInteriorCompletion), finalizer));
        }
        private static void Detach()
        {
            try
            {
                BuildModeLogics.on_apply_while_script_building -= OnApply;
                BuildModeLogics.on_cancel_while_script_building -= OnCancel;
                if (_harmony != null) _harmony.UnpatchSelf();
            }
            catch (Exception ex) { Warning("event=interior_completion_unpatch error=" + ex); }
            _harmony = null;
        }
        internal static void Stop() { _disabled = true; Reset(); Detach(); }
        internal static void Reset() { _pending = null; _finishing = null; _refresh = false; }
        private static void Warning(string text)
        { try { if (Planner.Log != null) Planner.Log.LogWarning("CRAFTING_PLANNER " + text); } catch { } }
        private static void Disable(string phase, Exception ex)
        {
            if (_disabled) return;
            _disabled = true; Reset();
            Warning("event=interior_completion_tracking state=disabled_for_process phase=" + phase + " error=" + ex);
        }
        private static bool Current(InteriorCompletionAttempt attempt)
        {
            return !_disabled && attempt != null && ReferenceEquals(_pending, attempt) && MainGame.me != null && ReferenceEquals(MainGame.me.save, attempt.Save);
        }
        private static void TryComplete(InteriorCompletionAttempt attempt)
        {
            if (!Current(attempt) || !Planner.CompleteInteriorProject(attempt)) return;
            _pending = null; _refresh = true;
        }
        private static void BeforeBuilder(CraftDefinition __0, out InteriorCompletionAttempt __state)
        {
            __state = null;
            if (_disabled) return;
            try
            {
                _pending = null; // A fresh dispatch invalidates any older preview, including untracked ones.
                var owner = BuildModeLogics.last_build_desk;
                ProjectGoal goal = Planner.FindInteriorCompletion(__0, owner != null ? owner.obj_id : null);
                if (goal == null || MainGame.me == null || MainGame.me.save == null) return;
                __state = new InteriorCompletionAttempt {Goal = goal, Definition = __0, Save = MainGame.me.save, ObjectUid = owner.unique_id};
                _pending = __state;
            }
            catch (Exception ex) { Disable("capture", ex); }
        }
        private static Exception AfterBuilder(Exception __exception, InteriorCompletionAttempt __state)
        {
            try { if (Current(__state)) { __state.ObserveDispatch(__exception == null); TryComplete(__state); } }
            catch (Exception ex) { Disable("dispatch", ex); }
            return __exception;
        }
        private static void BeforeFinish(CraftComponent __instance, out FinishScope __state)
        {
            __state = null;
            if (_disabled) return;
            try
            {
                InteriorCompletionAttempt attempt = null;
                if (Current(_pending) && __instance.is_crafting && ReferenceEquals(__instance.current_craft, _pending.Definition) &&
                    __instance.wgo != null && __instance.wgo.unique_id == _pending.ObjectUid) attempt = _pending;
                if (attempt == null && _finishing == null) return;
                __state = new FinishScope {Attempt = attempt, Parent = _finishing}; _finishing = __state;
            }
            catch (Exception ex) { Disable("finish_capture", ex); }
        }
        private static Exception AfterMarker(GameSave __instance, CraftDefinition __0, Exception __exception)
        {
            try
            {
                var attempt = _finishing != null ? _finishing.Attempt : null;
                if (Current(attempt) && ReferenceEquals(__instance, attempt.Save)) attempt.ObserveMarker(__0, __exception == null);
            }
            catch (Exception ex) { Disable("marker", ex); }
            return __exception;
        }
        private static Exception AfterFinish(Exception __exception, FinishScope __state)
        {
            if (__state == null) return __exception;
            try { if (Current(__state.Attempt)) { __state.Attempt.ObserveFinish(__exception == null); TryComplete(__state.Attempt); } }
            catch (Exception ex) { Disable("finish", ex); }
            finally { if (ReferenceEquals(_finishing, __state)) _finishing = __state.Parent; }
            return __exception;
        }
        private static void BeforeScript(CustomFlowScript __instance, string __0, string __1, out InteriorCompletionAttempt __state)
        {
            __state = null;
            if (_disabled) return;
            try
            {
                if (!Current(_pending)) return;
                string[] script = _pending.Definition.end_script.Split(':');
                if (__instance.script_name != script[0] || __0 != script[1] || __1 != script[2]) return;
                __state = _pending; __state.CaptureScript(__instance);
            }
            catch (Exception ex) { Disable("script_capture", ex); }
        }
        private static Exception AfterScript(Exception __exception, InteriorCompletionAttempt __state)
        {
            try { if (Current(__state)) { __state.ObserveScriptCall(__exception == null); TryComplete(__state); } }
            catch (Exception ex) { Disable("script", ex); }
            return __exception;
        }
        private static void Decision(bool apply)
        {
            if (_disabled) return;
            try { if (Current(_pending)) _pending.ObserveDecision(apply); }
            catch (Exception ex) { Disable("decision", ex); }
        }
        private static void OnApply() { Decision(true); }
        private static void OnCancel() { Decision(false); }
        private static void BeforeDecisionCall(out DecisionCall __state)
        {
            __state = default(DecisionCall);
            if (_disabled || _pending == null) return;
            __state.Attempt = _pending; __state.Before = _pending.Decisions;
        }
        private static Exception AfterDecisionCall(Exception __exception, DecisionCall __state)
        {
            // No scans, inventory queries or allocation on unchanged script-mode frames.
            var attempt = __state.Attempt;
            if (attempt == null || (attempt.Decisions == __state.Before && __exception == null)) return __exception;
            try { if (Current(attempt)) { attempt.ObserveDecisionReturn(__exception == null); TryComplete(attempt); } }
            catch (Exception ex) { Disable("decision_return", ex); }
            return __exception;
        }
        private static void BeforeTermination(CustomFlowScript __instance, out InteriorCompletionAttempt __state)
        {
            __state = null;
            if (_disabled) return;
            try { if (Current(_pending) && ReferenceEquals(__instance, _pending.Script)) __state = _pending; }
            catch (Exception ex) { Disable("termination_capture", ex); }
        }
        private static Exception AfterTermination(CustomFlowScript __instance, Exception __exception, InteriorCompletionAttempt __state)
        {
            try { if (Current(__state)) { __state.ObserveTermination(__instance, __exception == null); TryComplete(__state); } }
            catch (Exception ex) { Disable("termination", ex); }
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
