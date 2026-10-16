// SPDX-License-Identifier: MPL-2.0
// Research only: observes native calls; never adjusts planner or host state.
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

namespace NikichMods.PlacementCommitResearch
{
    [BepInPlugin(Id, "Building Planner Placement Commit Probe", "0.1.0")]
    public sealed class Probe : BaseUnityPlugin
    {
        internal const string Id = "nikich.gyk.placementcommitprobe";
        internal static ManualLogSource Log;
        internal static bool Disabled;
        [ThreadStatic] internal static Attempt Active;
        private Harmony _harmony;
        private static int _sequence;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(Id);
            try
            {
                _harmony.PatchAll(typeof(Probe).Assembly);
                Write("event=ready version=0.1.0 assembly_mvid=" + typeof(BuildModeLogics).Assembly.ManifestModule.ModuleVersionId + " mode=observe_only");
            }
            catch (Exception ex)
            {
                Fail(ex);
                _harmony.UnpatchSelf();
            }
        }

        private void OnDestroy()
        {
            if (_harmony != null) _harmony.UnpatchSelf();
            Active = null;
        }

        internal static void Write(string line)
        {
            try { if (Log != null) Log.LogInfo("PLACEMENT_COMMIT_PROBE " + line); }
            catch { Disabled = true; } // Observation must never interrupt native work.
        }
        internal static string Token(string value) { return string.IsNullOrEmpty(value) ? "null" : value.Replace(' ', '_').Replace('\r', '_').Replace('\n', '_').Replace('=', '_'); }
        internal static void Fail(Exception ex)
        {
            if (!Disabled) Write("event=observer_error action=disable_observation error=" + ex.GetType().Name);
            Disabled = true;
        }

        internal sealed class Attempt
        {
            internal int Sequence;
            internal Attempt Parent;
            internal ObjectCraftDefinition Definition;
            internal MultiInventory Inventory;
            internal WorldGameObject Object;
            internal string Identity;
            internal bool BuiltBefore;
            internal int SpendCalls, SpendSuccesses, LeaveCalls, BuiltRedraws;
        }

        [HarmonyPatch(typeof(BuildModeLogics), "DoPlace")]
        private static class Placement
        {
            private static void Prefix(ObjectCraftDefinition ____cd, MultiInventory ____multi_inventory, out Attempt __state)
            {
                __state = null;
                if (Disabled) return;
                try
                {
                    var floating = FloatingWorldGameObject.cur_floating;
                    var obj = floating != null ? floating.wobj : null;
                    var desk = BuildModeLogics.last_build_desk;
                    __state = new Attempt {
                        Sequence = ++_sequence, Parent = Active, Definition = ____cd,
                        Inventory = ____multi_inventory, Object = obj,
                        BuiltBefore = obj != null && obj.just_built,
                        Identity = " craft=" + Token(____cd != null ? ____cd.id : null) +
                            " builder=" + Token(desk != null ? desk.obj_id : null) +
                            " builder_uid=" + (desk != null ? desk.unique_id : 0) +
                            " object_uid=" + (obj != null ? obj.unique_id : 0)
                    };
                    Active = __state;
                    Write("event=attempt seq=" + __state.Sequence + __state.Identity +
                        " place_allowed=" + FloatingWorldGameObject.can_be_built +
                        " built_before=" + __state.BuiltBefore +
                        " build_type=" + (____cd != null ? ____cd.build_type.ToString() : "null") +
                        " one_time=" + (____cd != null && ____cd.one_time_craft));
                }
                catch (Exception ex) { Fail(ex); }
            }

            // Finalizer observes native exceptions and always preserves them.
            private static Exception Finalizer(Exception __exception, Attempt __state)
            {
                if (__state == null) return __exception;
                try
                {
                    if (!Disabled) Write("event=attempt_end seq=" + __state.Sequence + __state.Identity +
                        " spend_calls=" + __state.SpendCalls + " spend_successes=" + __state.SpendSuccesses +
                        " leave_calls=" + __state.LeaveCalls + " built_redraws=" + __state.BuiltRedraws +
                        " built_after=" + (__state.Object != null && __state.Object.just_built) +
                        " exception=" + (__exception != null ? __exception.GetType().Name : "none"));
                }
                catch (Exception ex) { Fail(ex); }
                finally { Active = __state.Parent; }
                return __exception;
            }
        }

        [HarmonyPatch(typeof(MultiInventory), "RemoveItems", new Type[] {
            typeof(List<Item>), typeof(MultiInventory.DestinationType), typeof(List<string>), typeof(List<Item>) })]
        private static class Spend
        {
            private static void Prefix(MultiInventory __instance, List<Item> __0, MultiInventory.DestinationType __1, out Attempt __state)
            {
                __state = null;
                if (Disabled) return;
                try
                {
                    var a = Active;
                    if (a == null || a.Definition == null || !ReferenceEquals(a.Inventory, __instance) || !ReferenceEquals(a.Definition.needs, __0)) return;
                    __state = a;
                    a.SpendCalls++;
                    Counts(a, __0, __1, "before");
                }
                catch (Exception ex) { Fail(ex); }
            }

            private static void Postfix(List<Item> __0, MultiInventory.DestinationType __1, bool __result, Attempt __state)
            {
                if (Disabled) return;
                try
                {
                    var a = __state;
                    if (a == null) return;
                    if (__result) a.SpendSuccesses++;
                    Write("event=spend seq=" + a.Sequence + " result=" + __result);
                    Counts(a, __0, __1, "after");
                }
                catch (Exception ex) { Fail(ex); }
            }

            private static void Counts(Attempt a, List<Item> needs, MultiInventory.DestinationType destination, string phase)
            {
                foreach (var item in needs)
                {
                    if (item == null) continue;
                    Write("event=material seq=" + a.Sequence + " phase=" + phase + " id=" + Token(item.id) +
                        " need=" + item.value + " count=" + a.Inventory.GetTotalCount(item.id, destination, true) +
                        " destination=" + destination);
                }
            }
        }

        [HarmonyPatch(typeof(FloatingWorldGameObject), "StopCurrentFloating")]
        private static class LeavePreview
        {
            private static void Prefix(bool __0, out Attempt __state)
            {
                __state = null;
                if (Disabled) return;
                try
                {
                    var floating = FloatingWorldGameObject.cur_floating;
                    if (Active != null && __0 && floating != null && ReferenceEquals(floating.wobj, Active.Object)) __state = Active;
                }
                catch (Exception ex) { Fail(ex); }
            }
            private static void Postfix(Attempt __state)
            {
                if (__state != null && !Disabled) __state.LeaveCalls++;
            }
        }

        [HarmonyPatch(typeof(WorldGameObject), "Redraw", new Type[] { typeof(bool), typeof(bool), typeof(bool) })]
        private static class BuiltRedraw
        {
            private static void Postfix(WorldGameObject __instance)
            {
                if (Disabled) return;
                try
                {
                    var a = Active;
                    if (a != null && ReferenceEquals(a.Object, __instance) && __instance.just_built) a.BuiltRedraws++;
                }
                catch (Exception ex) { Fail(ex); }
            }
        }

        [HarmonyPatch(typeof(BuildModeLogics), "CancelPlacing")]
        private static class Cancel
        {
            private static void Postfix()
            {
                if (!Disabled) Write("event=cancel seq=" + (Active != null ? Active.Sequence : 0));
            }
        }
    }
}
