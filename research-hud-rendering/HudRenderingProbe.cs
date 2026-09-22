using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CraftingPlannerHudRenderingProbe
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("nikich.gyk.craftingplanner", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.gyk.craftingplanner.hudrenderingprobe";
        public const string PluginName = "Crafting Planner HUD Rendering Probe";
        public const string PluginVersion = "0.3.0";

        internal static Plugin Instance;
        internal static ManualLogSource Log;

        private Harmony _harmony;
        private bool _initialHudDumpDone;
        private bool _resolutionDumpPending;

        private UILabel _controlSibling;
        private UILabel _controlPanel;
        private HUD _controlHud;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();

            LogLine(
                "event=loaded version=" + PluginVersion +
                " screen=" + Screen.width + "x" + Screen.height);
        }

        private void OnDestroy()
        {
            DestroyControls();

            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }

            Instance = null;
            Log = null;
        }

        internal void OnHudOpen(HUD hud)
        {
            if (hud == null)
            {
                return;
            }

            if (!_initialHudDumpDone)
            {
                _initialHudDumpDone = true;
                ScheduleSnapshot("hud_open_initial", true);
            }

            if (_resolutionDumpPending)
            {
                _resolutionDumpPending = false;
                ScheduleSnapshot("hud_open_after_resolution", true);
            }
        }

        internal void OnResolutionChanged(int requestedWidth, int requestedHeight)
        {
            _resolutionDumpPending = true;

            LogLine(
                "event=resolution_changed requested=" + requestedWidth + "x" + requestedHeight +
                " screen_now=" + Screen.width + "x" + Screen.height);

            ScheduleSnapshot("resolution_event", false);
        }

        internal void ScheduleSnapshot(string reason, bool ensureControls)
        {
            StartCoroutine(SnapshotAfterFrames(reason, ensureControls));
        }

        private IEnumerator SnapshotAfterFrames(string reason, bool ensureControls)
        {
            yield return null;
            yield return null;

            if (ensureControls)
            {
                EnsureControls();
                yield return null;
            }

            DumpSnapshot(reason);
        }

        private void EnsureControls()
        {
            HUD hud = GUIElements.me != null ? GUIElements.me.hud : null;
            if (hud == null || hud.panel == null || !hud.gameObject.activeInHierarchy)
            {
                return;
            }

            if (_controlHud != hud)
            {
                DestroyControls();
                _controlHud = hud;
            }

            if (_controlSibling != null && _controlPanel != null)
            {
                return;
            }

            UILabel reference = FindVisibleVanillaHudLabel(hud);
            if (reference == null)
            {
                LogLine("event=controls result=skipped reason=no_visible_vanilla_label");
                return;
            }

            int maxDepth = hud.GetComponentsInChildren<UIWidget>(true)
                .Where(x => x != null)
                .Select(x => x.depth)
                .DefaultIfEmpty(reference.depth)
                .Max();

            _controlSibling = UnityEngine.Object.Instantiate(reference, reference.transform.parent, false);
            _controlSibling.gameObject.name = "CraftingPlannerHudProbeSibling";
            _controlSibling.text = "[HUD PROBE A - sibling]";
            _controlSibling.depth = maxDepth + 20;
            _controlSibling.alpha = 1f;
            _controlSibling.gameObject.SetActive(true);
            _controlSibling.transform.localPosition =
                reference.transform.localPosition +
                new Vector3(0f, -Mathf.Max(reference.height, 24) - 12f, 0f);

            _controlPanel = UnityEngine.Object.Instantiate(reference, hud.panel.transform, false);
            _controlPanel.gameObject.name = "CraftingPlannerHudProbePanel";
            _controlPanel.text = "[HUD PROBE B - panel]";
            _controlPanel.SetAnchor((GameObject)null);
            _controlPanel.depth = maxDepth + 30;
            _controlPanel.alpha = 1f;
            _controlPanel.gameObject.SetActive(true);

            Vector3 referenceInPanel = hud.panel.transform.InverseTransformPoint(reference.transform.position);
            _controlPanel.transform.localPosition =
                referenceInPanel +
                new Vector3(Mathf.Max(reference.width, 120) + 30f, -Mathf.Max(reference.height, 24) - 12f, 0f);

            LogLine(
                "event=controls result=created reference=" + Safe(reference.gameObject.name) +
                " reference_path=" + Safe(Path(reference.transform)) +
                " max_depth=" + maxDepth);
        }

        private UILabel FindVisibleVanillaHudLabel(HUD hud)
        {
            UILabel[] labels = hud.GetComponentsInChildren<UILabel>(true);

            foreach (UILabel label in labels)
            {
                if (label == null || IsProbeOrPlanner(label))
                {
                    continue;
                }

                if (label.gameObject.activeInHierarchy &&
                    label.enabled &&
                    label.isVisible &&
                    label.finalAlpha > 0.001f &&
                    label.hasVertices)
                {
                    return label;
                }
            }

            if (hud.day_label != null && !IsProbeOrPlanner(hud.day_label))
            {
                return hud.day_label;
            }

            return labels.FirstOrDefault(x => x != null && !IsProbeOrPlanner(x));
        }

        private static bool IsProbeOrPlanner(UILabel label)
        {
            string n = label != null && label.gameObject != null ? label.gameObject.name : "";
            return n.StartsWith("CraftingPlanner", StringComparison.Ordinal);
        }

        private void DestroyControls()
        {
            if (_controlSibling != null)
            {
                UnityEngine.Object.Destroy(_controlSibling.gameObject);
            }

            if (_controlPanel != null)
            {
                UnityEngine.Object.Destroy(_controlPanel.gameObject);
            }

            _controlSibling = null;
            _controlPanel = null;
            _controlHud = null;
        }

        private void DumpSnapshot(string reason)
        {
            HUD hud = GUIElements.me != null ? GUIElements.me.hud : null;
            UIRoot root =
                MainGame.me != null && MainGame.me.ui_root != null
                    ? MainGame.me.ui_root
                    : (hud != null ? hud.GetComponentInParent<UIRoot>() : null);

            StringBuilder summary = new StringBuilder();
            summary.Append("event=snapshot");
            summary.Append(" reason=").Append(Safe(reason));
            summary.Append(" screen=").Append(Screen.width).Append("x").Append(Screen.height);
            summary.Append(" gui_pixel_zoom=").Append(MainGame.me != null ? MainGame.me.gui_pixel_zoom : -1);

            if (root != null)
            {
                summary.Append(" root_name=").Append(Safe(root.gameObject.name));
                summary.Append(" root_scaling=").Append(root.scalingStyle);
                summary.Append(" root_active_height=").Append(root.activeHeight);
                summary.Append(" root_manual=").Append(root.manualWidth).Append("x").Append(root.manualHeight);
                summary.Append(" root_pixel_adjust=").Append(F(root.pixelSizeAdjustment));
                summary.Append(" root_local_scale=").Append(V(root.transform.localScale));
            }
            else
            {
                summary.Append(" root=null");
            }

            if (hud != null)
            {
                summary.Append(" hud_active=").Append(hud.gameObject.activeInHierarchy);
                summary.Append(" hud_local_scale=").Append(V(hud.transform.localScale));
            }
            else
            {
                summary.Append(" hud=null");
            }

            LogLine(summary.ToString());

            if (hud != null && hud.panel != null)
            {
                DumpPanel("hud_panel", hud.panel);
            }

            if (hud != null)
            {
                DumpLabel("vanilla_day", hud.day_label);
                DumpLabel("vanilla_zone_descr", hud.zone_descr);
                DumpLabel("vanilla_version", hud.version_label);

                UILabel planner = FindNamedLabel(hud.transform, "CraftingPlannerHUD");
                DumpLabel("planner_hud", planner);

                DumpLabel("control_sibling", _controlSibling);
                DumpLabel("control_panel", _controlPanel);

                UILabel visibleReference = FindVisibleVanillaHudLabel(hud);
                DumpLabel("visible_reference", visibleReference);
            }

            CraftGUI craft = GUIElements.me != null ? GUIElements.me.craft : null;
            if (craft != null)
            {
                UILabel craftPlanner = FindNamedLabel(craft.transform.root, "CraftingPlannerCraftOverlay");
                DumpLabel("planner_craft_overlay", craftPlanner);

                UIPanel craftPanel = craft.GetComponentInParent<UIPanel>();
                if (craftPanel != null)
                {
                    DumpPanel("craft_panel", craftPanel);
                }
            }
        }

        private static UILabel FindNamedLabel(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            UILabel[] labels = root.GetComponentsInChildren<UILabel>(true);
            return labels.FirstOrDefault(
                x => x != null && x.gameObject != null && x.gameObject.name == name);
        }

        private static void DumpPanel(string role, UIPanel panel)
        {
            if (panel == null)
            {
                LogLine("event=panel role=" + role + " value=null");
                return;
            }

            LogLine(
                "event=panel role=" + role +
                " name=" + Safe(panel.gameObject.name) +
                " path=" + Safe(Path(panel.transform)) +
                " active=" + panel.gameObject.activeInHierarchy +
                " enabled=" + panel.enabled +
                " depth=" + panel.depth +
                " alpha=" + F(panel.alpha) +
                " final_alpha=" + F(panel.finalAlpha) +
                " clipping=" + panel.clipping +
                " base_clip=" + V4(panel.baseClipRegion) +
                " final_clip=" + V4(panel.finalClipRegion) +
                " clip_offset=" + V2(panel.clipOffset) +
                " size=" + F(panel.width) + "x" + F(panel.height) +
                " local_pos=" + V(panel.transform.localPosition) +
                " local_scale=" + V(panel.transform.localScale));
        }

        private static void DumpLabel(string role, UILabel label)
        {
            if (label == null)
            {
                LogLine("event=label role=" + role + " value=null");
                return;
            }

            UIPanel panel = label.panel != null ? label.panel : label.GetComponentInParent<UIPanel>();
            string screenRect = ScreenRect(label, panel);

            LogLine(
                "event=label role=" + role +
                " name=" + Safe(label.gameObject.name) +
                " path=" + Safe(Path(label.transform)) +
                " active_self=" + label.gameObject.activeSelf +
                " active_hierarchy=" + label.gameObject.activeInHierarchy +
                " enabled=" + label.enabled +
                " is_visible=" + label.isVisible +
                " has_vertices=" + label.hasVertices +
                " depth=" + label.depth +
                " panel=" + Safe(panel != null ? panel.gameObject.name : null) +
                " panel_depth=" + (panel != null ? panel.depth : int.MinValue) +
                " alpha=" + F(label.alpha) +
                " final_alpha=" + F(label.finalAlpha) +
                " color=" + C(label.color) +
                " anchored=" + label.isAnchored +
                " anchors=" + Anchors(label) +
                " pivot=" + label.pivot +
                " size=" + label.width + "x" + label.height +
                " local_pos=" + V(label.transform.localPosition) +
                " world_pos=" + V(label.transform.position) +
                " local_scale=" + V(label.transform.localScale) +
                " screen_rect=" + screenRect +
                " text=" + Safe(CompactText(label.text)));
        }

        private static string ScreenRect(UILabel label, UIPanel panel)
        {
            if (label == null)
            {
                return "null";
            }

            Camera camera = panel != null ? panel.anchorCamera : null;
            if (camera == null)
            {
                return "no_camera";
            }

            Vector3[] corners = label.worldCorners;
            if (corners == null || corners.Length < 4)
            {
                return "no_corners";
            }

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 p = camera.WorldToScreenPoint(corners[i]);
                minX = Mathf.Min(minX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x);
                maxY = Mathf.Max(maxY, p.y);
            }

            bool intersects =
                maxX >= 0f &&
                maxY >= 0f &&
                minX <= Screen.width &&
                minY <= Screen.height;

            return
                F(minX) + "," + F(minY) + "," +
                F(maxX) + "," + F(maxY) +
                ",intersects=" + intersects;
        }

        private static string Anchors(UIRect rect)
        {
            if (rect == null)
            {
                return "null";
            }

            return
                "L[" + Anchor(rect.leftAnchor) + "]" +
                "R[" + Anchor(rect.rightAnchor) + "]" +
                "B[" + Anchor(rect.bottomAnchor) + "]" +
                "T[" + Anchor(rect.topAnchor) + "]";
        }

        private static string Anchor(UIRect.AnchorPoint anchor)
        {
            if (anchor == null)
            {
                return "null";
            }

            return
                Safe(anchor.target != null ? Path(anchor.target) : "none") +
                ",rel=" + F(anchor.relative) +
                ",abs=" + anchor.absolute;
        }

        private static string Path(Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            List<string> names = new List<string>();
            Transform current = transform;
            int guard = 0;

            while (current != null && guard++ < 32)
            {
                names.Add(current.gameObject.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        private static string CompactText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "empty";
            }

            string text = value.Replace("\r", " ").Replace("\n", "\\n").Replace("\t", " ");
            return text.Length <= 100 ? text : text.Substring(0, 100) + "...";
        }

        private static string V(Vector3 value)
        {
            return F(value.x) + "," + F(value.y) + "," + F(value.z);
        }

        private static string V2(Vector2 value)
        {
            return F(value.x) + "," + F(value.y);
        }

        private static string V4(Vector4 value)
        {
            return F(value.x) + "," + F(value.y) + "," + F(value.z) + "," + F(value.w);
        }

        private static string C(Color value)
        {
            return F(value.r) + "," + F(value.g) + "," + F(value.b) + "," + F(value.a);
        }

        private static string F(float value)
        {
            return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string Safe(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "null";
            }

            return value
                .Replace(" ", "_")
                .Replace("=", "_")
                .Replace("|", "_")
                .Replace("\r", "_")
                .Replace("\n", "_")
                .Replace("\t", "_");
        }

        internal static void LogLine(string details)
        {
            if (Log != null)
            {
                Log.LogInfo("CRAFTING_PLANNER_HUD_PROBE " + details);
            }
        }
    }

    [HarmonyPatch(typeof(HUD), "Open")]
    internal static class HudOpenPatch
    {
        private static void Postfix(HUD __instance)
        {
            if (Plugin.Instance != null)
            {
                Plugin.Instance.OnHudOpen(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(MainGame), "OnScreenSizeChanged")]
    internal static class ScreenSizeChangedPatch
    {
        private static void Postfix(int w, int h)
        {
            if (Plugin.Instance != null)
            {
                Plugin.Instance.OnResolutionChanged(w, h);
            }
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenAsBuild")]
    internal static class CraftGuiOpenPatch
    {
        private static void Postfix()
        {
            if (Plugin.Instance != null)
            {
                Plugin.Instance.ScheduleSnapshot("craft_open", false);
            }
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "Hide")]
    internal static class CraftGuiHidePatch
    {
        private static void Postfix()
        {
            if (Plugin.Instance != null)
            {
                Plugin.Instance.ScheduleSnapshot("craft_closed", true);
            }
        }
    }

    [HarmonyPatch(typeof(BaseGUI), "OnPressedNextSubTab")]
    internal static class RtPatch
    {
        private static void Postfix(BaseGUI __instance)
        {
            if (__instance is CraftGUI && Plugin.Instance != null)
            {
                Plugin.Instance.ScheduleSnapshot("after_rt", false);
            }
        }
    }

    [HarmonyPatch(typeof(BaseGUI), "OnPressedPrevSubTab")]
    internal static class LtPatch
    {
        private static void Postfix(BaseGUI __instance)
        {
            if (__instance is CraftGUI && Plugin.Instance != null)
            {
                Plugin.Instance.ScheduleSnapshot("after_lt", false);
            }
        }
    }
}
