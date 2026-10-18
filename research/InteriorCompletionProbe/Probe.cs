// SPDX-License-Identifier: MPL-2.0
// Passive research only: never changes craft, input, inventory, world or planner state.
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]
namespace NikichMods.InteriorResearch
{
    [BepInPlugin("nikich.gyk.interiorcompletionprobe","Building Planner Interior Completion Probe","0.1.0")]
    public sealed class Probe : BaseUnityPlugin
    {
        private static ManualLogSource _log;
        private static bool _disabled;
        private static Attempt _pending;
        private static long _sequence, _decisions;
        private Harmony _harmony;
        private sealed class Attempt
        {
            internal long Seq, OwnerUid;
            internal ObjectCraftDefinition Definition;
            internal GameSave Save;
            internal CustomFlowScript Script;
            internal string ScriptName, Outcome="none";
            internal bool Dispatching;
            internal int Applies, Cancels, Finishes, Saves, Spends;
        }
        private void Awake()
        {
            _log=Logger;
            try
            {
                _harmony=new Harmony("nikich.gyk.interiorcompletionprobe");
                Bind(typeof(BuildModeLogics),"CraftBuilding",typeof(void),new[]{typeof(CraftDefinition)},"BeforeBuilder","AfterBuilder");
                Bind(typeof(BuildModeLogics),"UpdateWhileScriptBuilding",typeof(void),Type.EmptyTypes,"BeforeTick","AfterTick");
                Bind(typeof(CraftComponent),"ProcessFinishedCraft",typeof(void),Type.EmptyTypes,"BeforeFinish","AfterFinish");
                Bind(typeof(GameSave),"OnFinishedCraft",typeof(void),new[]{typeof(CraftDefinition)},null,"AfterSave");
                Bind(typeof(MultiInventory),"RemoveItems",typeof(bool),new[]{typeof(List<Item>),typeof(MultiInventory.DestinationType),typeof(List<string>),typeof(List<Item>)},null,"AfterSpend");
                Bind(typeof(CustomFlowScript),"FireEvent",typeof(void),new[]{typeof(string),typeof(string)},"BeforeScript",null);
                Bind(typeof(CustomFlowScript),"TerminateMe",typeof(void),Type.EmptyTypes,"BeforeTerminate","AfterTerminate");
                Bind(typeof(GS),"Spawn",typeof(WorldGameObject),new[]{typeof(string),typeof(Transform),typeof(string)},null,"AfterSpawn");
                Bind(typeof(WorldGameObject),"ReplaceWithObject",typeof(void),new[]{typeof(string),typeof(bool),typeof(int)},"BeforeReplace","AfterReplace");
                Bind(typeof(WorldGameObject),"DestroyMe",typeof(void),Type.EmptyTypes,"BeforeDestroy",null);
                Bind(typeof(MainGame),"OnGameStartedPlaying",typeof(void),Type.EmptyTypes,"Reset",null);
                BuildModeLogics.on_apply_while_script_building+=OnApply;
                BuildModeLogics.on_cancel_while_script_building+=OnCancel;
                Write("event=ready version=0.1.0 mode=observe_only host_mvid="+typeof(CraftComponent).Assembly.ManifestModule.ModuleVersionId);
            }
            catch(Exception ex) { Fail(ex); Detach(); }
        }
        private void Bind(Type type,string name,Type result,Type[] args,string prefix,string finalizer)
        {
            var method=AccessTools.Method(type,name,args);
            if(method==null || method.ReturnType!=result || method.IsStatic!=(type==typeof(GS))) throw new InvalidOperationException(type.Name+"."+name);
            _harmony.Patch(method,prefix:prefix==null?null:new HarmonyMethod(typeof(Probe),prefix),finalizer:finalizer==null?null:new HarmonyMethod(typeof(Probe),finalizer));
        }
        private void Detach() { BuildModeLogics.on_apply_while_script_building-=OnApply; BuildModeLogics.on_cancel_while_script_building-=OnCancel; if(_harmony!=null) _harmony.UnpatchSelf(); }
        private void OnDestroy() { _disabled=true;_pending=null;try {Detach();}catch{} }
        private static void Write(string text) { try { if(_log!=null) _log.LogInfo("INTERIOR_COMPLETION_PROBE "+text); }catch { _disabled=true; } }
        private static void Fail(Exception ex) { if(!_disabled)Write("event=observer_error action=disable_observation error="+ex);_disabled=true; }
        private static void Safe(Action action) { if(!_disabled)try {action();}catch(Exception ex){Fail(ex);} }
        private static string Token(string text) {return string.IsNullOrEmpty(text)?"null":text.Replace(' ','_').Replace('\n','_').Replace('\r','_');}
        private static string Error(Exception ex) {return ex==null?"none":ex.GetType().Name;}
        private static string Id(Attempt a) {return "seq="+a.Seq+" craft="+Token(a.Definition.id)+" owner_uid="+a.OwnerUid+" outcome="+a.Outcome;}
        private static bool Current() {return _pending!=null && MainGame.me!=null && ReferenceEquals(MainGame.me.save,_pending.Save);}
        private static bool Supported(ObjectCraftDefinition d)
        {
            return d!=null && d.build_type==ObjectCraftDefinition.BuildType.None && d.wait_script_callback && d.is_auto && !d.one_time_craft &&
                d.id!=null && d.id.StartsWith("keeper_room_builddesk::",StringComparison.Ordinal) && !string.IsNullOrEmpty(d.end_script);
        }
        private static string Counts(Attempt a)
        {
            if(MainGame.me==null || MainGame.me.player==null || !ReferenceEquals(MainGame.me.save,a.Save)) return "unavailable";
            var inventory=MainGame.me.player.GetMultiInventoryOfWGOWithoutWorldZone(true);
            if(inventory==null || a.Definition.needs==null)return "unavailable";
            var values=new List<string>();foreach(var n in a.Definition.needs) if(n!=null)values.Add(Token(n.id)+":"+inventory.GetTotalCount(n.id,MultiInventory.DestinationType.AllFromFirst,true));
            return string.Join(",",values.ToArray());
        }
        private static bool RoomTag(string tag) {return tag=="hero_bed" || tag=="home_cupboard" || tag=="home_cook_table" || tag=="home_cook_stand" || tag=="home_walls" || tag=="home_ornament";}
        private static void BeforeBuilder(CraftDefinition __0,out Attempt __state)
        {
            __state=null;if(_disabled)return;
            try
            {
                var d=__0 as ObjectCraftDefinition;if(!Supported(d))return;
                var owner=BuildModeLogics.last_build_desk;
                if(owner==null || owner.obj_id!="keeper_room_builddesk" || MainGame.me==null || MainGame.me.save==null)return;
                if(_pending!=null)Write("event=previous_attempt "+Id(_pending)+" applies="+_pending.Applies+" cancels="+_pending.Cancels);
                __state=new Attempt {Seq=++_sequence,Definition=d,Save=MainGame.me.save,OwnerUid=owner.unique_id,ScriptName=d.end_script.Split(':')[0],Dispatching=true};_pending=__state;
                Write("event=dispatch_begin "+Id(__state)+" script="+Token(d.end_script)+" counts="+Counts(__state));
            }catch(Exception ex){Fail(ex);}
        }
        private static Exception AfterBuilder(Exception __exception,Attempt __state)
        {
            if(__state!=null)Safe(()=>{__state.Dispatching=false;Write("event=dispatch_end "+Id(__state)+" exception="+Error(__exception)+" finishes="+__state.Finishes+" saves="+__state.Saves+" counts="+Counts(__state));});
            return __exception;
        }
        private static void BeforeFinish(CraftComponent __instance,out Attempt __state)
        {__state=null;if(_disabled)return;try {if(Current() && ReferenceEquals(__instance.current_craft,_pending.Definition) && __instance.wgo!=null && __instance.wgo.unique_id==_pending.OwnerUid) {__state=_pending;Write("event=finish_begin "+Id(_pending)+" is_crafting="+__instance.is_crafting);}}catch(Exception ex){Fail(ex);}}
        private static Exception AfterFinish(Exception __exception,Attempt __state)
        {if(__state!=null)Safe(()=>{if(__exception==null)__state.Finishes++;Write("event=finish_end "+Id(__state)+" exception="+Error(__exception));});return __exception;}
        private static Exception AfterSave(CraftDefinition __0,Exception __exception)
        {Safe(()=>{if(Current() && ReferenceEquals(__0,_pending.Definition)){if(__exception==null)_pending.Saves++;Write("event=save_finish "+Id(_pending)+" exception="+Error(__exception));}});return __exception;}
        private static Exception AfterSpend(List<Item> __0,bool __result,Exception __exception)
        {Safe(()=>{if(Current() && _pending.Dispatching){_pending.Spends++;Write("event=spend "+Id(_pending)+" result="+__result+" needs_same="+ReferenceEquals(__0,_pending.Definition.needs)+" exception="+Error(__exception));}});return __exception;}
        private static void BeforeScript(CustomFlowScript __instance,string __0,string __1)
        {Safe(()=>{if(Current() && __instance.script_name==_pending.ScriptName && __0=="place"){_pending.Script=__instance;Write("event=script_place "+Id(_pending)+" payload="+Token(__1));}});}
        private static void Decision(bool apply)
        {Safe(()=>{if(!Current())return;_decisions++;if(apply)_pending.Applies++;else _pending.Cancels++;_pending.Outcome=apply?"apply":"cancel";Write("event=decision "+Id(_pending)+" applies="+_pending.Applies+" cancels="+_pending.Cancels+" counts="+Counts(_pending));});}
        private static void OnApply(){Decision(true);} private static void OnCancel(){Decision(false);}
        private static void BeforeTick(out long __state){__state=_decisions;}
        private static Exception AfterTick(Exception __exception,long __state)
        {if(_disabled || _pending==null || (_decisions==__state && __exception==null))return __exception; Safe(()=>{if(Current())Write("event=decision_return "+Id(_pending)+" exception="+Error(__exception)+" counts="+Counts(_pending));});return __exception;}
        private static void BeforeTerminate(CustomFlowScript __instance,out Attempt __state)
        {__state=null;if(_disabled)return;try {if(Current() && ReferenceEquals(__instance,_pending.Script))__state=_pending;}catch(Exception ex){Fail(ex);}}
        private static Exception AfterTerminate(Exception __exception,Attempt __state)
        {if(__state!=null)Safe(()=>Write("event=script_terminated "+Id(__state)+" applies="+__state.Applies+" cancels="+__state.Cancels+" exception="+Error(__exception)+" counts="+Counts(__state)));return __exception;}
        private static Exception AfterSpawn(string __0,string __2,WorldGameObject __result,Exception __exception)
        {Safe(()=>{if(Current() && RoomTag(__2))Write("event=room_spawn "+Id(_pending)+" target="+Token(__0)+" tag="+Token(__2)+" alive="+(__result!=null)+" uid="+(__result!=null?__result.unique_id:0)+" exception="+Error(__exception));});return __exception;}
        private static void BeforeReplace(WorldGameObject __instance,string __0,out Attempt __state)
        {__state=null;Safe(()=>{if(Current() && __instance!=null && RoomTag(__instance.custom_tag))Write("event=room_replace_begin "+Id(_pending)+" old="+Token(__instance.obj_id)+" target="+Token(__0)+" uid="+__instance.unique_id);});if(!_disabled && Current() && __instance!=null && RoomTag(__instance.custom_tag))__state=_pending;}
        private static Exception AfterReplace(WorldGameObject __instance,Exception __exception,Attempt __state)
        {if(__state!=null)Safe(()=>Write("event=room_replace_end "+Id(__state)+" after="+Token(__instance!=null?__instance.obj_id:null)+" exception="+Error(__exception)));return __exception;}
        private static void BeforeDestroy(WorldGameObject __instance)
        {Safe(()=>{if(Current() && __instance!=null && RoomTag(__instance.custom_tag))Write("event=room_destroy "+Id(_pending)+" target="+Token(__instance.obj_id)+" uid="+__instance.unique_id);});}
        private static void Reset(){Safe(()=>{if(_pending!=null)Write("event=session_reset "+Id(_pending));_pending=null;});}
    }
}
