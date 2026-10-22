// SPDX-License-Identifier: MPL-2.0
// Research-only passive timings; never invokes, skips or modifies measured gameplay/UI operations.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using BepInEx;
using HarmonyLib;
using UnityEngine;
[assembly: AssemblyVersion("0.1.1.0")]
[assembly: AssemblyFileVersion("0.1.1.0")]
namespace NikichMods.MenuOpeningResearch
{
    [BepInPlugin("nikich.gyk.planner.menuopeningprobe", "Building Planner Menu Opening Probe", "0.1.1")]
    public sealed class Probe : BaseUnityPlugin
    {
        private struct Metric { internal string Name; internal int Count; internal long Inclusive, Self; }
        private struct Span { internal int Metric; internal long Start, Children; }
        private static readonly Dictionary<MethodBase, int> Methods = new Dictionary<MethodBase, int>();
        private static readonly Metric[] Metrics = new Metric[5];
        private static readonly Span[] Spans = new Span[256];
        private static Probe _instance;
        private Harmony _harmony;
        private static int _size, _depth, _root, _sample, _thread, _endFrame, _errors;
        private static long _start, _rootTicks;
        private static bool _active, _returned, _failed, _overlap;
        private void Awake()
        {
            _instance = this;
            try
            {
                _harmony = new Harmony("nikich.gyk.planner.menuopeningprobe");
                Add(typeof(CraftGUI), "OpenAsBuild", "build_open", typeof(WorldGameObject), typeof(CraftsInventory));
                Add(typeof(CraftGUI), "SwitchTab", "tab_switch", typeof(string));
                Add(typeof(CraftGUI), "ProceedOpen", "native_open", typeof(WorldGameObject), typeof(CraftsInventory), typeof(string));
                Add(typeof(CraftItemGUI), "Redraw", "native_row_redraw");
                Add(typeof(UITable), "Reposition", "native_table_layout");
                Logger.LogInfo("MENU_OPEN_PROBE event=ready version=0.1.1 native_only=True limit=24 passive=True");
            }
            catch (Exception ex) { Disable(ex); if (_harmony != null) _harmony.UnpatchSelf(); }
        }
        private void Add(Type type, string name, string label, params Type[] args)
        {
            MethodInfo method = AccessTools.DeclaredMethod(type, name, args);
            if (method == null) throw new MissingMethodException(type.FullName, name);
            int index = _size++;
            Metrics[index].Name = label; Methods.Add(method, index);
            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(Probe), "Begin") { priority = Priority.First },
                finalizer: new HarmonyMethod(typeof(Probe), "End") { priority = Priority.Last });
        }
        private static void Begin(MethodBase __originalMethod, out int __state)
        {
            __state = -1;
            if (_failed || (_sample >= 24 && !_active)) return;
            try
            {
                int index;
                if (!Methods.TryGetValue(__originalMethod, out index)) return;
                if (!_active)
                {
                    if (index > 1) return;
                    for (int i = 0; i < _size; i++) { Metrics[i].Count = 0; Metrics[i].Inclusive = Metrics[i].Self = 0; }
                    _active = true; _returned = _overlap = false; _depth = _errors = 0;
                    _root = index; _thread = Thread.CurrentThread.ManagedThreadId; _start = Stopwatch.GetTimestamp(); _sample++;
                }
                if (Thread.CurrentThread.ManagedThreadId != _thread) return;
                if (index <= 1 && _returned && _depth == 0) _overlap = true;
                if (_depth >= Spans.Length) throw new InvalidOperationException("Timing stack capacity exceeded");
                int slot = _depth++;
                Spans[slot].Metric = index; Spans[slot].Children = 0; Spans[slot].Start = Stopwatch.GetTimestamp();
                __state = slot;
            }
            catch (Exception ex) { Disable(ex); }
        }
        private static Exception End(int __state, Exception __exception)
        {
            long end = Stopwatch.GetTimestamp();
            if (_failed || !_active || __state < 0) return __exception;
            try
            {
                if (_depth != __state + 1) throw new InvalidOperationException("Timing stack order mismatch");
                Span span = Spans[__state]; long elapsed = end - span.Start;
                Metrics[span.Metric].Count++; Metrics[span.Metric].Inclusive += elapsed;
                Metrics[span.Metric].Self += Math.Max(0, elapsed - span.Children);
                _depth--;
                if (_depth > 0) Spans[_depth - 1].Children += elapsed;
                if (__exception != null) _errors++;
                if (!_returned && __state == 0 && span.Metric == _root)
                { _rootTicks = elapsed; _endFrame = Time.frameCount; _returned = true; }
            }
            catch (Exception ex) { Disable(ex); }
            return __exception;
        }
        private static string Ms(long ticks) { return (ticks * 1000d / Stopwatch.Frequency).ToString("0.###", CultureInfo.InvariantCulture); }
        private void LateUpdate()
        {
            if (!_active || _failed || !_returned || _depth != 0 || Time.frameCount < _endFrame + 2) return;
            long window = Stopwatch.GetTimestamp() - _start;
            _active = false;
            Logger.LogInfo("MENU_OPEN_PROBE event=sample id=" + _sample + " operation=" + Metrics[_root].Name +
                " root_method_ms=" + Ms(_rootTicks) + " window_ms=" + Ms(window) + " overlap=" + _overlap + " exceptions=" + _errors);
            for (int i = 0; i < _size; i++)
                if (Metrics[i].Count > 0) Logger.LogInfo("MENU_OPEN_PROBE event=metric id=" + _sample + " name=" + Metrics[i].Name +
                    " calls=" + Metrics[i].Count + " inclusive_ms=" + Ms(Metrics[i].Inclusive) + " self_traced_ms=" + Ms(Metrics[i].Self));
            if (_sample >= 24) Logger.LogInfo("MENU_OPEN_PROBE event=limit_reached samples=24");
        }
        private static void Disable(Exception ex)
        {
            _failed = true; _active = false;
            if (_instance != null) _instance.Logger.LogWarning("MENU_OPEN_PROBE event=disabled error=" + ex);
        }
        private void OnDestroy() { _active = false; _failed = true; if (_harmony != null) _harmony.UnpatchSelf(); }
    }
}
