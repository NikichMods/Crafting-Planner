// SPDX-License-Identifier: MPL-2.0
using System;
using HarmonyLib;
using UnityEngine;
namespace CraftingPlanner
{
    internal struct PlannerBounds
    {
        internal float Left, Bottom, Right, Top;
        internal bool Valid;
        internal PlannerBounds(float left, float bottom, float right, float top)
        { Left = left; Bottom = bottom; Right = right; Top = top; Valid = right > left && top > bottom; }
        internal void Include(float x, float y)
        {
            if (!Valid) { Left = Right = x; Bottom = Top = y; Valid = true; return; }
            Left = Math.Min(Left, x); Right = Math.Max(Right, x); Bottom = Math.Min(Bottom, y); Top = Math.Max(Top, y);
        }
        internal void Include(PlannerBounds other)
        { if (other.Valid) { Include(other.Left, other.Bottom); Include(other.Right, other.Top); } }
        internal PlannerBounds Shift(float x)
        { return Valid ? new PlannerBounds(Left + x, Bottom, Right + x, Top) : new PlannerBounds(); }
        internal bool Intersects(PlannerBounds other)
        { return Valid && other.Valid && Left < other.Right && Right > other.Left && Bottom < other.Top && Top > other.Bottom; }
    }
    internal static class PlannerNpcLayout
    {
        internal const float Gap = 8;
        internal static float Offset(PlannerBounds plan, PlannerBounds primary, PlannerBounds additional, float screenLeft, out bool limited)
        {
            limited = false;
            if (!plan.Intersects(primary) && !plan.Intersects(additional)) return 0;
            PlannerBounds group = primary; group.Include(additional);
            float desired = Math.Min(0, plan.Left - Gap - group.Right);
            float available = Math.Min(0, screenLeft + Gap - group.Left);
            float offset = Math.Max(desired, available);
            limited = offset > desired + .01f;
            return offset;
        }
    }
    // Own only an applied translation; do not mistake a new external/native position for our baseline.
    internal sealed class PlannerNpcTranslation
    {
        private float _offset, _written;
        internal float Owned(float current) { return Math.Abs(current - _written) < .01f ? _offset : 0; }
        internal float Place(float current, float offset)
        { _written = current - Owned(current) + offset; _offset = offset; return _written; }
        internal float Clear(float current) { return Place(current, 0); }
        internal void Forget() { _offset = _written = 0; }
    }
    internal static class PlannerNpcCards
    {
        private static Harmony _harmony;
        private static UIPanel _panel;
        private static Transform _group;
        private static RelationGUI _primary, _additional;
        private static UIWidget[] _anchors;
        private static readonly PlannerNpcTranslation Translation = new PlannerNpcTranslation();
        private static bool _failed, _shifted, _limited;
        private static int _visible;
        private static int _primaryLocationRevision = -1, _additionalLocationRevision = -1;
        internal static void Install()
        {
            try
            {
                var draw = AccessTools.DeclaredMethod(typeof(UIPanel), "UpdateSelf", Type.EmptyTypes);
                var update = AccessTools.DeclaredMethod(typeof(RelationGUI), "Update", Type.EmptyTypes);
                if (draw == null || update == null || draw.ReturnType != typeof(void) || update.ReturnType != typeof(void))
                    throw new MissingMethodException("Verified NPC layout consumers are unavailable");
                _harmony = new Harmony(Plugin.PluginGuid + ".npc_cards");
                _harmony.Patch(draw, prefix: new HarmonyMethod(typeof(PlannerNpcRenderPatch), "Prefix"));
                _harmony.Patch(update, prefix: new HarmonyMethod(typeof(PlannerNpcNativePositionPatch), "Prefix"),
                    finalizer: new HarmonyMethod(typeof(PlannerNpcNativePositionPatch), "Finalizer"));
                Planner.LogLine("event=npc_cards_hooks state=ready");
            }
            catch (Exception ex) { Disable(ex); Detach(); }
        }
        private static void Detach()
        {
            try { if (_harmony != null) _harmony.UnpatchSelf(); }
            catch (Exception ex) { Planner.LogLine("event=npc_cards_hooks state=unpatch_failed error=" + ex); }
            _harmony = null;
        }
        internal static void Stop() { _failed = true; Reset(); Detach(); }
        internal static void Include(UIWidget widget, Transform space, ref PlannerBounds bounds)
        {
            if (widget == null || !widget.enabled || !widget.gameObject.activeInHierarchy || widget.alpha <= .001f) return;
            IncludeGeometry(widget, space, ref bounds);
        }
        internal static void IncludeGeometry(UIWidget widget, Transform space, ref PlannerBounds bounds)
        {
            if (widget == null) return;
            foreach (Vector3 corner in widget.worldCorners)
            {
                Vector3 local = space.InverseTransformPoint(corner);
                bounds.Include(local.x, local.y);
            }
        }
        private static void RestoreOffset()
        {
            if (_group != null)
            {
                Vector3 position = _group.localPosition;
                float native = Translation.Clear(position.x);
                bool moved = Math.Abs(native - position.x) > .001f;
                if (moved)
                {
                    position.x = native; _group.localPosition = position;
                    if (_anchors != null) foreach (UIWidget anchor in _anchors) if (anchor != null) anchor.ResetAndUpdateAnchors();
                }
            }
        }
        private static void Restore()
        {
            RestoreOffset();
            if (_shifted) Planner.LogLine("event=npc_cards state=native reason=restore");
            _shifted = _limited = false; _visible = 0;
        }
        internal static void Reset()
        {
            try { Restore(); }
            catch (Exception ex) { _failed = true; Planner.LogLine("event=npc_cards state=restore_failed error=" + ex); }
            _panel = null; _group = null; _primary = _additional = null; _anchors = null; Translation.Forget();
        }
        internal static float BeforeNativeUpdate(RelationGUI card)
        {
            GUIElements gui = GUIElements.me;
            if (gui == null || (!ReferenceEquals(card, gui.relation) && !ReferenceEquals(card, gui.relation_additional))) return float.NaN;
            PlannerLocationCards.Refresh();
            if (_failed || _group == null || (!ReferenceEquals(card, _primary) && !ReferenceEquals(card, _additional))) return float.NaN;
            try
            {
                float offset = Translation.Owned(_group.localPosition.x);
                int revision = PlannerLocationCards.Revision;
                bool changed = ReferenceEquals(card, _primary) ? _primaryLocationRevision != revision : _additionalLocationRevision != revision;
                if (ReferenceEquals(card, _primary)) _primaryLocationRevision = revision; else _additionalLocationRevision = revision;
                if (Math.Abs(offset) <= .001f)
                {
                    if (changed && _anchors != null) foreach (UIWidget anchor in _anchors) if (anchor != null) anchor.ResetAndUpdateAnchors();
                    return float.NaN;
                }
                RestoreOffset();
                return offset;
            }
            catch (Exception ex) { Disable(ex); return float.NaN; }
        }
        internal static void AfterNativeUpdate(RelationGUI card, float offset)
        {
            if (float.IsNaN(offset) || _failed || _group == null || (!ReferenceEquals(card, _primary) && !ReferenceEquals(card, _additional))) return;
            try
            {
                Vector3 position = _group.localPosition;
                position.x = Translation.Place(position.x, offset); _group.localPosition = position;
            }
            catch (Exception ex) { Disable(ex); }
        }
        private static void Disable(Exception ex)
        {
            _failed = true; Reset();
            Planner.LogLine("event=npc_cards state=disabled_for_process error=" + ex);
        }
        internal static void Prepare(UIPanel panel)
        {
            if (_failed || ReferenceEquals(panel, null)) return;
            try
            {
                GUIElements gui = GUIElements.me;
                if (gui == null || gui.relation == null || gui.relation_additional == null)
                { if (_panel == panel) Reset(); return; }
                Transform group = gui.relation.transform.parent;
                if (group == null || group != panel.transform) return;
                if (_panel != panel) Reset();
                RelationGUI first = gui.relation, second = gui.relation_additional;
                if (second.transform.parent != group || group.parent == null || panel.clipping != UIDrawCall.Clipping.None ||
                    !Anchors(first, group) || !Anchors(second, group) || second.additional_point == null || second.additional_point.parent != group ||
                    first.anchor_zone_name != second.anchor_zone_name || first.anchor_zone_info != second.anchor_zone_info || first.anchor_no_zone != second.anchor_no_zone)
                    throw new InvalidOperationException("Unsupported native relation panel/anchor hierarchy");
                if (_panel == null)
                {
                    _panel = panel; _group = group;
                    _primary = first; _additional = second;
                    _anchors = new[] {first.anchor_zone_name.GetComponent<UIWidget>(), first.anchor_zone_info.GetComponent<UIWidget>(),
                        first.anchor_no_zone.GetComponent<UIWidget>(), second.additional_point.GetComponent<UIWidget>()};
                    foreach (UIWidget anchor in _anchors) if (anchor == null) throw new InvalidOperationException("Native relation anchor widget missing");
                    Planner.LogLine("event=npc_cards state=ready scope=two_native_cards");
                }
                PlannerBounds plan, screen;
                if (!PlannerPresentation.TryNpcBounds(group.parent, out plan, out screen)) { Restore(); return; }
                bool primaryVisible = Visible(first), additionalVisible = Visible(second);
                if (!primaryVisible && !additionalVisible) { Restore(); return; }
                PlannerBounds primary = new PlannerBounds(), additional = new PlannerBounds();
                foreach (UIWidget widget in panel.widgets)
                {
                    if (widget == null) continue;
                    if (primaryVisible && widget.transform.IsChildOf(first.transform)) Include(widget, group.parent, ref primary);
                    else if (additionalVisible && widget.transform.IsChildOf(second.transform)) Include(widget, group.parent, ref additional);
                }
                Vector3 position = group.localPosition;
                float owned = Translation.Owned(position.x);
                primary = primary.Shift(-owned); additional = additional.Shift(-owned);
                bool limited;
                float offset = PlannerNpcLayout.Offset(plan, primary, additional, screen.Left, out limited);
                bool shifted = offset < -.01f;
                int visible = (primary.Valid ? 1 : 0) | (additional.Valid ? 2 : 0);
                float nextX = Translation.Place(position.x, offset);
                if (Math.Abs(nextX - position.x) > .001f) { position.x = nextX; group.localPosition = position; }
                if (shifted != _shifted || limited != _limited || visible != _visible)
                    Planner.LogLine("event=npc_cards state=" + (shifted ? "shifted" : "native") + " visible_mask=" + visible +
                        " offset_ui=" + offset.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + " screen_limited=" + limited);
                _shifted = shifted; _limited = limited; _visible = visible;
            }
            catch (Exception ex)
            {
                Disable(ex);
            }
        }
        private static bool Visible(RelationGUI card)
        { return card.gameObject.activeInHierarchy && card.back != null && card.back.alpha > .001f; }
        private static bool Anchors(RelationGUI card, Transform group)
        { return card.anchor_zone_name != null && card.anchor_zone_name.parent == group && card.anchor_zone_info != null &&
            card.anchor_zone_info.parent == group && card.anchor_no_zone != null && card.anchor_no_zone.parent == group; }
    }
    // Exact native 1.407 final mesh consumer; all other panels return without modification.
    internal static class PlannerNpcRenderPatch
    { private static void Prefix(UIPanel __instance) { PlannerPresentation.BeforeDraw(__instance); PlannerLocationCards.Prepare(__instance); PlannerNpcCards.Prepare(__instance); } }
    internal static class PlannerNpcNativePositionPatch
    {
        private static void Prefix(RelationGUI __instance, out float __state) { __state = PlannerNpcCards.BeforeNativeUpdate(__instance); }
        private static Exception Finalizer(RelationGUI __instance, float __state, Exception __exception)
        { PlannerNpcCards.AfterNativeUpdate(__instance, __state); return __exception; }
    }
}
