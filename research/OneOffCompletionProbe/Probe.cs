// SPDX-License-Identifier: MPL-2.0
// Research only: scoped observation of real native work. No planner/game mutation.
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]
namespace NikichMods.OneOffCompletionResearch
{
    [BepInPlugin(Id, "Building Planner One-Off Completion Probe", "0.1.0")]
    public sealed class Probe : BaseUnityPlugin
    {
        internal const string Id = "nikich.gyk.oneoffcompletionprobe";
        private static ManualLogSource _log;
        private static bool _disabled;
        private static int _sequence;
        [ThreadStatic] private static Scope _active;
        private Harmony _harmony;
        private sealed class Scope
        {
            internal Scope Parent;
            internal int Seq;
            internal string Phase, Owner, Craft, Family;
            internal long Uid;
            internal bool Tracked;
            internal CraftDefinition Definition;
            internal CraftComponent Component;
            internal WorldGameObject Object;
            internal int Spends, Successes, Replaces, Saves, Ends;
        }
        private void Awake()
        {
            _log = Logger; _harmony = new Harmony(Id);
            try
            {
                Bind(typeof(CraftComponent), "CraftReally", typeof(bool), new[] {typeof(CraftDefinition),typeof(Item),typeof(List<string>),typeof(List<Item>),typeof(bool),typeof(int),typeof(bool),typeof(bool),typeof(bool)}, "BeforeStart", "AfterStart");
                Bind(typeof(CraftComponent), "ProcessFinishedCraft", typeof(void), Type.EmptyTypes, "BeforeFinish", "AfterFinish");
                Bind(typeof(CraftComponent), "Cancel", typeof(void), Type.EmptyTypes, "BeforeCancel", null);
                Bind(typeof(CraftComponent), "End", typeof(void), Type.EmptyTypes, "BeforeEnd", null);
                Bind(typeof(MultiInventory), "RemoveItems", typeof(bool), new[] {typeof(List<Item>),typeof(MultiInventory.DestinationType),typeof(List<string>),typeof(List<Item>)}, null, "AfterSpend");
                Bind(typeof(WorldGameObject), "ReplaceWithObject", typeof(void), new[] {typeof(string),typeof(bool),typeof(int)}, "BeforeReplace", "AfterReplace");
                Bind(typeof(GameSave), "OnFinishedCraft", typeof(void), new[] {typeof(CraftDefinition)}, null, "AfterSaveFinish");
                Bind(typeof(BuildModeLogics), "CraftBuilding", typeof(void), new[] {typeof(CraftDefinition)}, "BeforeBuilder", null);
                Write("event=ready version=0.1.0 assembly_mvid=" + typeof(CraftComponent).Assembly.ManifestModule.ModuleVersionId + " mode=observe_only");
            }
            catch (Exception ex)
            {
                Fail(ex);
                try { _harmony.UnpatchSelf(); } catch (Exception cleanup) { Write("event=cleanup_error error=" + cleanup.GetType().Name); }
            }
        }
        private void Bind(Type owner, string name, Type result, Type[] args, string prefix, string finalizer)
        {
            var method = AccessTools.Method(owner, name, args);
            if (method == null || method.IsStatic || method.ReturnType != result) throw new InvalidOperationException(owner.Name + "." + name);
            _harmony.Patch(method, prefix: prefix == null ? null : new HarmonyMethod(typeof(Probe), prefix), finalizer: finalizer == null ? null : new HarmonyMethod(typeof(Probe), finalizer));
        }
        private void OnDestroy()
        {
            _disabled = true; _active = null;
            try { if (_harmony != null) _harmony.UnpatchSelf(); } catch { }
        }
        private static void Write(string text)
        { try { if (_log != null) _log.LogInfo("ONEOFF_COMPLETION_PROBE " + text); } catch { _disabled = true; } }
        private static void Fail(Exception ex)
        { if (!_disabled) Write("event=observer_error action=disable_observation error=" + ex); _disabled = true; }
        private static string Token(string text)
        { return string.IsNullOrEmpty(text) ? "null" : text.Replace(' ', '_').Replace('\r','_').Replace('\n','_').Replace('=','_'); }
        private static string Family(CraftDefinition definition)
        {
            if (definition == null || definition.needs == null || definition.needs.Count == 0) return null;
            var build = definition as ObjectCraftDefinition;
            if (build != null) return build.one_time_craft && build.build_type == ObjectCraftDefinition.BuildType.None ? "builder_oneoff" : null;
            return string.IsNullOrEmpty(definition.change_wgo) ? null : "world_change";
        }
        private static string Items(List<Item> items)
        {
            if (items == null) return "null";
            var values = new List<string>();
            foreach (var item in items) if (item != null) values.Add(Token(item.id) + ":" + item.value);
            return values.Count == 0 ? "empty" : string.Join(",", values.ToArray());
        }
        private static string Identity(Scope s)
        { return "seq=" + s.Seq + " parent=" + (s.Parent != null ? s.Parent.Seq : 0) + " phase=" + s.Phase + " family=" + s.Family + " owner=" + s.Owner + " owner_uid=" + s.Uid + " craft=" + s.Craft; }
        private static Scope Begin(CraftComponent component, CraftDefinition definition, string phase)
        {
            string family = Family(definition);
            if (family == null && _active == null) return null;
            var s = new Scope {Parent=_active, Seq=++_sequence, Phase=phase, Tracked=family!=null, Family=family, Component=component, Definition=definition};
            if (s.Tracked)
            {
                s.Object=component.wgo; s.Owner=Token(s.Object != null ? s.Object.obj_id : null); s.Uid=s.Object != null ? s.Object.unique_id : 0; s.Craft=Token(definition.id);
            }
            _active=s;
            if (s.Tracked) Write("event=" + phase + "_begin " + Identity(s) + " current_same=" + ReferenceEquals(component.current_craft,definition) + " is_crafting=" + component.is_crafting + " needs=" + Items(definition.needs) + " change_wgo=" + Token(definition.change_wgo) + " end_script=" + Token(definition.end_script) + " end_event=" + Token(definition.end_event));
            return s;
        }
        private static void BeforeStart(CraftComponent __instance, CraftDefinition __0, out Scope __state)
        {
            __state=null; if (_disabled) return;
            try { __state=Begin(__instance,__0,"start"); } catch(Exception ex) { Fail(ex); }
        }
        private static void BeforeFinish(CraftComponent __instance, out Scope __state)
        {
            __state=null; if (_disabled) return;
            try { __state=Begin(__instance,__instance.current_craft,"finish"); } catch(Exception ex) { Fail(ex); }
        }
        private static Exception Close(Scope scope, Exception error, string result)
        {
            if (scope == null) return error;
            try
            {
                if (!_disabled && scope.Tracked) Write("event=" + scope.Phase + "_end " + Identity(scope) + " result=" + result + " exception=" + (error==null?"none":error.GetType().Name) + " current_same=" + ReferenceEquals(scope.Component.current_craft,scope.Definition) + " is_crafting=" + scope.Component.is_crafting + " destroyed=" + scope.Component.destroyed + " owner_alive=" + (scope.Object!=null) + " owner_after=" + Token(scope.Object!=null?scope.Object.obj_id:null) + " spends=" + scope.Spends + " spend_successes=" + scope.Successes + " replaces=" + scope.Replaces + " save_finishes=" + scope.Saves + " ends=" + scope.Ends);
            }
            catch(Exception ex) { Fail(ex); }
            finally { _active=scope.Parent; }
            return error;
        }
        private static Exception AfterStart(bool __result, Exception __exception, Scope __state)
        { return Close(__state,__exception,__result.ToString()); }
        private static Exception AfterFinish(Exception __exception, Scope __state)
        { return Close(__state,__exception,"void"); }
        private static Exception AfterSpend(MultiInventory __instance, List<Item> __0, bool __result, Exception __exception)
        {
            if (!_disabled && _active!=null && _active.Tracked)
            {
                try { var s=_active; s.Spends++; if (__exception==null && __result) s.Successes++; Write("event=spend " + Identity(s) + " result=" + __result + " exception=" + (__exception==null?"none":__exception.GetType().Name) + " needs_same=" + ReferenceEquals(__0,s.Definition.needs) + " items=" + Items(__0)); }
                catch(Exception ex) { Fail(ex); }
            }
            return __exception;
        }
        private static void BeforeReplace(WorldGameObject __instance, string __0, out Scope __state)
        {
            __state=null; if (_disabled) return;
            try { if (_active!=null && _active.Tracked && ReferenceEquals(__instance,_active.Object)) {__state=_active; Write("event=replace_begin " + Identity(__state) + " target=" + Token(__0));} }
            catch(Exception ex) { Fail(ex); }
        }
        private static Exception AfterReplace(string __0, Exception __exception, Scope __state)
        {
            if (!_disabled && __state!=null)
            {
                try { if (__exception==null) __state.Replaces++; Write("event=replace_end " + Identity(__state) + " target=" + Token(__0) + " exception=" + (__exception==null?"none":__exception.GetType().Name) + " alive=" + (__state.Object!=null) + " after=" + Token(__state.Object!=null?__state.Object.obj_id:null)); }
                catch(Exception ex) { Fail(ex); }
            }
            return __exception;
        }
        private static Exception AfterSaveFinish(CraftDefinition __0, Exception __exception)
        {
            if (!_disabled && _active!=null && _active.Tracked && ReferenceEquals(__0,_active.Definition))
            {
                try { if (__exception==null) _active.Saves++; Write("event=save_finish " + Identity(_active) + " exception=" + (__exception==null?"none":__exception.GetType().Name)); }
                catch(Exception ex) { Fail(ex); }
            }
            return __exception;
        }
        private static void BeforeEnd(CraftComponent __instance)
        {
            if (_disabled) return;
            try { if (_active!=null && _active.Tracked && ReferenceEquals(__instance,_active.Component)) {_active.Ends++; Write("event=end " + Identity(_active));} }
            catch(Exception ex) { Fail(ex); }
        }
        private static void BeforeCancel(CraftComponent __instance)
        {
            if (_disabled) return;
            try { if (Family(__instance.current_craft)!=null) Write("event=cancel owner=" + Token(__instance.wgo!=null?__instance.wgo.obj_id:null) + " owner_uid=" + (__instance.wgo!=null?__instance.wgo.unique_id:0) + " craft=" + Token(__instance.current_craft.id)); }
            catch(Exception ex) { Fail(ex); }
        }
        private static void BeforeBuilder(CraftDefinition __0)
        {
            if (_disabled) return;
            try { if (Family(__0)=="builder_oneoff") {var owner=BuildModeLogics.last_build_desk; Write("event=builder_dispatch owner=" + Token(owner!=null?owner.obj_id:null) + " owner_uid=" + (owner!=null?owner.unique_id:0) + " craft=" + Token(__0.id));} }
            catch(Exception ex) { Fail(ex); }
        }
    }
}
