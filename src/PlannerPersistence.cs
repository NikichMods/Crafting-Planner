// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using HarmonyLib;

namespace CraftingPlanner
{
    internal sealed class SavedGoal
    {
        internal string Key;
        internal int Quantity;
    }

    // Own data only: host binary files are read for identity, never modified.
    internal static class PlannerPlanStore
    {
        private const string Header = "CraftingPlannerGoals\t1";
        internal static string Hash(byte[] bytes)
        {
            using (var algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
        internal static string SnapshotPath(string root, string nativePath, string digest)
        { return Path.Combine(root, Hash(Encoding.UTF8.GetBytes(Path.GetFullPath(nativePath))), digest + ".plan"); }
        internal static string[] Encode(IList<ProjectGoal> goals)
        {
            var lines = new List<string> { Header };
            foreach (ProjectGoal goal in goals)
                lines.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(goal.Key)) + "\t" + goal.Quantity.ToString(CultureInfo.InvariantCulture));
            return lines.ToArray();
        }
        internal static List<SavedGoal> Decode(string[] lines)
        {
            if (lines.Length == 0 || lines[0] != Header) throw new FormatException("Unknown plan format");
            var result = new List<SavedGoal>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split('\t');
                int quantity;
                if (row.Length != 2 || !int.TryParse(row[1], NumberStyles.None, CultureInfo.InvariantCulture, out quantity) ||
                    quantity < 1 || quantity > PlannerGoalState.MaxQuantity) throw new FormatException("Invalid plan quantity");
                string key = new UTF8Encoding(false, true).GetString(Convert.FromBase64String(row[0]));
                string[] parts = key.Split('|');
                if (parts.Length != 3 || (parts[0] != "1" && parts[0] != "2") || string.IsNullOrEmpty(parts[1]) ||
                    string.IsNullOrEmpty(parts[2]) || !keys.Add(key)) throw new FormatException("Invalid plan key");
                result.Add(new SavedGoal { Key = key, Quantity = quantity });
            }
            return result;
        }
        internal static ProjectGoal Resolve(SavedGoal saved, CraftDefinition definition)
        {
            string[] parts = saved.Key.Split('|');
            if (parts.Length != 3 || string.IsNullOrEmpty(parts[1]) || !Planner.HasPlainNeeds(definition) || parts[2] != definition.id) return null;
            var build = definition as ObjectCraftDefinition;
            bool repeatable;
            if (parts[0] == "1" && build != null)
                repeatable = build.build_type == ObjectCraftDefinition.BuildType.Put && !build.one_time_craft;
            else if (parts[0] == "2" && build == null && !string.IsNullOrEmpty(definition.change_wgo))
                repeatable = PlannerGoalState.IsLightingProject(definition, parts[1]);
            else return null;
            if (saved.Quantity < 1 || saved.Quantity > (repeatable ? PlannerGoalState.MaxQuantity : 1)) return null;
            return new ProjectGoal { Key = saved.Key, Definition = definition, Quantity = saved.Quantity, Repeatable = repeatable };
        }
        internal static bool Committed(string expected, string actual, bool existed, long before, long after)
        { return expected != null && expected == actual && (!existed || before != after); }
        internal static void Write(string path, string[] lines)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllLines(temporary, lines, new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }

    internal static class PlannerPersistence
    {
        private sealed class LoadedVersion { internal string Digest; }
        private sealed class SaveCall
        {
            internal SaveCall Parent;
            internal GameSave Save;
            internal string NativePath, Digest;
            internal string[] Lines;
            internal bool Existed;
            internal long Before;
        }
        private static Harmony _harmony;
        private static bool _enabled, _writable;
        private static GameSave _session;
        private static readonly ConditionalWeakTable<GameSave, LoadedVersion> Versions = new ConditionalWeakTable<GameSave, LoadedVersion>();
        private static readonly HashSet<string> Unreadable = new HashSet<string>(StringComparer.Ordinal);
        [ThreadStatic] private static SaveCall _saving;
        private static string Root { get { return Path.Combine(Paths.ConfigPath, "CraftingPlanner", "plans"); } }
        private static string Identity(string path)
        { return " slot=" + Path.GetFileName(Path.GetDirectoryName(path)).Substring(0, 12) + " snapshot=" + Path.GetFileNameWithoutExtension(path).Substring(0, 12); }
        private static string NativePath(SaveSlotData slot)
        {
            if (slot == null || string.IsNullOrEmpty(slot.filename_no_extension)) throw new InvalidOperationException("Missing native slot");
            return Path.GetFullPath(PlatformSpecific.GetSaveFolder() + slot.filename_no_extension + ".dat");
        }
        internal static void Install()
        {
            try
            {
                _harmony = new Harmony(Plugin.PluginGuid + ".persistence");
                var writer = AccessTools.Method(typeof(PlatformSpecific), "SaveGameDataToSlot", new[] {typeof(SaveSlotData), typeof(GameSave), typeof(PlatformSpecific.OnCompleteDelegate)});
                var encoder = AccessTools.Method(typeof(GameSave), "ToBinary", Type.EmptyTypes);
                var decoder = AccessTools.Method(typeof(GameSave), "FromBinary", new[] {typeof(byte[])});
                if (writer == null || !writer.IsStatic || writer.ReturnType != typeof(void) || encoder == null || encoder.IsStatic ||
                    encoder.ReturnType != typeof(byte[]) || decoder == null || !decoder.IsStatic || decoder.ReturnType != typeof(GameSave))
                    throw new InvalidOperationException("Unsupported native save signatures");
                _harmony.Patch(writer, prefix: new HarmonyMethod(typeof(PlannerPersistence), nameof(BeforeSave)), finalizer: new HarmonyMethod(typeof(PlannerPersistence), nameof(AfterSave)));
                _harmony.Patch(encoder, postfix: new HarmonyMethod(typeof(PlannerPersistence), nameof(AfterEncode)));
                _harmony.Patch(decoder, postfix: new HarmonyMethod(typeof(PlannerPersistence), nameof(AfterDecode)));
                _enabled = true;
                Planner.LogLine("event=plan_persistence state=ready schema=1");
            }
            catch (Exception ex) { Stop(); Warn("install", ex); }
        }
        internal static void Stop()
        {
            _enabled = false; _writable = false; _session = null; _saving = null;
            try { if (_harmony != null) _harmony.UnpatchSelf(); } catch (Exception ex) { Warn("stop", ex); }
            _harmony = null;
        }
        private static void Warn(string phase, Exception ex)
        { try { if (Planner.Log != null) Planner.Log.LogWarning("CRAFTING_PLANNER event=plan_persistence_error phase=" + phase + " error=" + ex.GetType().Name); } catch { } }
        internal static void Restore()
        {
            _session = MainGame.me != null ? MainGame.me.save : null;
            _writable = false;
            if (!_enabled || _session == null) return;
            string path = null;
            try
            {
                LoadedVersion version;
                if (!Versions.TryGetValue(_session, out version))
                {
                    // A new world has no decoder receipt; its first native save may establish one.
                    _writable = true;
                    Planner.LogLine("event=plan_restore status=no_binary_receipt goals=0");
                    return;
                }
                string native = NativePath(MainGame.me.save_slot);
                path = PlannerPlanStore.SnapshotPath(Root, native, version.Digest);
                if (!File.Exists(path))
                {
                    _writable = true;
                    Planner.LogLine("event=plan_restore status=no_snapshot goals=0" + Identity(path));
                    return;
                }
                var rows = PlannerPlanStore.Decode(File.ReadAllLines(path));
                if (GameBalance.me == null) throw new InvalidOperationException("Balance unavailable");
                var restored = new List<ProjectGoal>();
                int skipped = 0;
                foreach (SavedGoal row in rows)
                {
                    string[] parts = row.Key.Split('|');
                    CraftDefinition definition = parts[0] == "1" ? (CraftDefinition)GameBalance.me.GetDataOrNull<ObjectCraftDefinition>(parts[2]) : GameBalance.me.GetDataOrNull<CraftDefinition>(parts[2]);
                    ProjectGoal goal = PlannerPlanStore.Resolve(row, definition);
                    if (goal != null) restored.Add(goal); else skipped++;
                }
                foreach (ProjectGoal goal in restored)
                {
                    Planner.GetGoals().Add(goal);
                    Planner.LogLine("event=plan_restored_goal key=" + goal.Key + " quantity=" + goal.Quantity);
                }
                _writable = true;
                if (skipped > 0) Unreadable.Add(path); // Keep the original snapshot, even after a later same-world save.
                Planner.LogLine("event=plan_restore status=restored goals=" + restored.Count + " skipped=" + skipped + Identity(path));
            }
            catch (Exception ex) { if (path != null) Unreadable.Add(path); Warn("restore", ex); }
        }
        private static void BeforeSave(SaveSlotData __0, GameSave __1, out SaveCall __state)
        {
            __state = new SaveCall { Parent = _saving };
            _saving = __state;
            try
            {
                if (!_enabled || !_writable || !ReferenceEquals(__1, _session)) return;
                __state.NativePath = NativePath(__0);
                __state.Existed = File.Exists(__state.NativePath);
                __state.Before = __state.Existed ? File.GetLastWriteTimeUtc(__state.NativePath).Ticks : 0;
                __state.Save = __1;
            }
            catch (Exception ex) { Warn("capture", ex); }
        }
        private static void AfterEncode(GameSave __instance, byte[] __result)
        {
            try
            {
                if (_saving == null || _saving.Save == null || !ReferenceEquals(__instance, _saving.Save) || __result == null) return;
                _saving.Digest = PlannerPlanStore.Hash(__result);
                _saving.Lines = PlannerPlanStore.Encode(Planner.GetGoals());
            }
            catch (Exception ex) { if (_saving != null) _saving.Lines = null; Warn("encode", ex); }
        }
        private static void AfterDecode(byte[] __0, GameSave __result)
        {
            try
            {
                if (__result == null || __0 == null) return;
                Versions.Remove(__result);
                Versions.Add(__result, new LoadedVersion { Digest = PlannerPlanStore.Hash(__0) });
            }
            catch (Exception ex) { Warn("decode", ex); }
        }
        private static Exception AfterSave(Exception __exception, SaveCall __state)
        {
            try
            {
                if (__exception != null || __state == null || __state.Save == null || __state.Lines == null ||
                    !ReferenceEquals(__state.Save, _session) || !_writable) return __exception;
                string native = __state.NativePath;
                if (!File.Exists(native) || !PlannerPlanStore.Committed(__state.Digest, PlannerPlanStore.Hash(File.ReadAllBytes(native)),
                    __state.Existed, __state.Before, File.GetLastWriteTimeUtc(native).Ticks))
                { Planner.LogLine("event=plan_save status=native_commit_unverified"); return __exception; }
                string path = PlannerPlanStore.SnapshotPath(Root, native, __state.Digest);
                if (Unreadable.Contains(path)) { Planner.LogLine("event=plan_save status=preserved_unreadable_snapshot"); return __exception; }
                PlannerPlanStore.Write(path, __state.Lines);
                Planner.LogLine("event=plan_save status=saved goals=" + (__state.Lines.Length - 1) + Identity(path));
            }
            catch (Exception ex) { Warn("write", ex); }
            finally { _saving = __state != null ? __state.Parent : null; }
            return __exception;
        }
    }
}
