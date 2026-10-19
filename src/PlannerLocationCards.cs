// SPDX-License-Identifier: MPL-2.0
using System;
using UnityEngine;
namespace CraftingPlanner
{
    internal static class PlannerLocationLayout
    {
        internal static bool IsPlaceholder(string name, string description)
        {
            if (!string.IsNullOrWhiteSpace(description)) return false;
            string title = (name ?? "").Trim();
            return title.Length == 0 || title == "..." || title == "\u2026";
        }
    }
    // Native anchor absolutes own placement; equal edits preserve native width and content.
    internal sealed class PlannerLocationAnchors
    {
        internal int Left, Right, Offset;
        internal void Bind(int left, int right) { Left = left; Right = right; Offset = 0; }
        internal bool Owns(int left, int right) { return left == Left + Offset && right == Right + Offset; }
    }
    internal static class PlannerLocationCards
    {
        private static UIWidget _widget;
        private static UIWidget _additionalAnchor;
        private static UIPanel _panel;
        private static readonly PlannerLocationAnchors Anchors = new PlannerLocationAnchors();
        private static readonly PlannerLocationAnchors AdditionalAnchors = new PlannerLocationAnchors();
        private static bool _failed, _hidden, _limited;
        private static float _nativeAlpha;
        internal static int Revision { get; private set; }
        private static void Alpha(bool hide)
        {
            if (_widget == null || hide == _hidden) return;
            if (hide) { _nativeAlpha = _widget.alpha; _widget.alpha = 0; }
            else if (_widget.alpha == 0) _widget.alpha = _nativeAlpha;
            _hidden = hide; Revision++;
        }
        private static void Place(int offset)
        {
            if (_widget == null || Anchors.Offset == offset) return;
            if (!OwnsAnchors())
                throw new InvalidOperationException("Location anchors changed externally");
            _widget.leftAnchor.absolute = Anchors.Left + offset;
            _widget.rightAnchor.absolute = Anchors.Right + offset;
            _additionalAnchor.leftAnchor.absolute = AdditionalAnchors.Left + offset;
            _additionalAnchor.rightAnchor.absolute = AdditionalAnchors.Right + offset;
            Anchors.Offset = AdditionalAnchors.Offset = offset;
            _widget.ResetAndUpdateAnchors(); _additionalAnchor.ResetAndUpdateAnchors(); Revision++;
        }
        private static bool OwnsAnchors()
        { return _widget != null && _additionalAnchor != null && Anchors.Owns(_widget.leftAnchor.absolute, _widget.rightAnchor.absolute) &&
            AdditionalAnchors.Owns(_additionalAnchor.leftAnchor.absolute, _additionalAnchor.rightAnchor.absolute); }
        internal static void Restore()
        {
            if (_widget == null) return;
            bool changed = _hidden || Anchors.Offset != 0;
            Alpha(false);
            // Restore each owned pair independently; an external edit to one must not strand the other.
            if (Anchors.Owns(_widget.leftAnchor.absolute, _widget.rightAnchor.absolute) && Anchors.Offset != 0)
            {
                _widget.leftAnchor.absolute = Anchors.Left; _widget.rightAnchor.absolute = Anchors.Right;
                _widget.ResetAndUpdateAnchors(); Revision++;
            }
            if (_additionalAnchor != null && AdditionalAnchors.Owns(_additionalAnchor.leftAnchor.absolute, _additionalAnchor.rightAnchor.absolute) && AdditionalAnchors.Offset != 0)
            {
                _additionalAnchor.leftAnchor.absolute = AdditionalAnchors.Left; _additionalAnchor.rightAnchor.absolute = AdditionalAnchors.Right;
                _additionalAnchor.ResetAndUpdateAnchors(); Revision++;
            }
            Anchors.Offset = AdditionalAnchors.Offset = 0;
            _limited = false;
            if (changed) Planner.LogLine("event=location_cards state=native reason=restore");
        }
        internal static void Reset()
        {
            try { Restore(); }
            catch (Exception ex) { _failed = true; Planner.LogLine("event=location_cards state=restore_failed error=" + ex); }
            _widget = _additionalAnchor = null; _panel = null; _hidden = false; Revision++;
        }
        internal static void Prepare(UIPanel panel)
        {
            if (_failed || ReferenceEquals(panel, null)) return;
            GUIElements gui = GUIElements.me;
            if (gui == null || gui.hud == null || gui.hud.zone_name == null) return;
            Transform banner = gui.hud.zone_name.transform.parent;
            if (banner == null || banner.parent == null || banner.parent.parent != panel.transform) return;
            Refresh();
        }
        internal static void Refresh()
        {
            if (_failed) return;
            try
            {
                GUIElements gui = GUIElements.me;
                if (gui == null || gui.hud == null || gui.hud.zone_name == null || gui.hud.zone_descr == null || gui.screen_size_widget == null)
                { Reset(); return; }
                Transform banner = gui.hud.zone_name.transform.parent;
                UIWidget widget = banner != null && banner.parent != null ? banner.parent.GetComponent<UIWidget>() : null;
                if (widget != _widget) Reset();
                if (widget == null) return;
                if (_widget == null)
                {
                    UIPanel panel = widget.transform.parent.GetComponent<UIPanel>();
                    UIWidget additional = gui.relation_additional != null && gui.relation_additional.additional_point != null ?
                        gui.relation_additional.additional_point.GetComponent<UIWidget>() : null;
                    if (panel == null || panel.clipping != UIDrawCall.Clipping.None || panel.transform.parent != gui.hud.transform ||
                        additional == null || additional.leftAnchor.target != gui.screen_size_widget.transform || additional.rightAnchor.target != gui.screen_size_widget.transform ||
                        additional.leftAnchor.relative != 1 || additional.rightAnchor.relative != 1 ||
                        gui.relation == null || additional.transform.parent != gui.relation.transform.parent ||
                        !gui.hud.zone_descr.transform.IsChildOf(widget.transform) ||
                        widget.leftAnchor.target != gui.screen_size_widget.transform || widget.rightAnchor.target != gui.screen_size_widget.transform ||
                        widget.leftAnchor.relative != 1 || widget.rightAnchor.relative != 1 ||
                        widget.transform.localScale != Vector3.one || panel.transform.localScale != Vector3.one || gui.hud.transform.localScale != Vector3.one)
                        throw new InvalidOperationException("Unsupported native location hierarchy/anchors");
                    _widget = widget; _panel = panel; _additionalAnchor = additional;
                    Anchors.Bind(widget.leftAnchor.absolute, widget.rightAnchor.absolute);
                    AdditionalAnchors.Bind(additional.leftAnchor.absolute, additional.rightAnchor.absolute);
                    Planner.LogLine("event=location_cards state=ready");
                }
                PlannerBounds plan, screen;
                if (!OwnsAnchors())
                    throw new InvalidOperationException("Location anchors changed externally");
                if (!PlannerPresentation.TryNpcBounds(_panel.transform.parent, out plan, out screen)) { Restore(); return; }
                bool placeholder = PlannerLocationLayout.IsPlaceholder(gui.hud.zone_name.text, gui.hud.zone_descr.text);
                bool oldHidden = _hidden;
                Alpha(placeholder);
                int previous = Anchors.Offset;
                bool limited = false;
                int offset = 0;
                if (!placeholder)
                {
                    _widget.ResetAndUpdateAnchors();
                    PlannerBounds bounds = new PlannerBounds();
                    foreach (UIWidget part in _panel.widgets)
                        if (part != null && part.transform.IsChildOf(_widget.transform)) PlannerNpcCards.Include(part, _panel.transform.parent, ref bounds);
                    bounds = bounds.Shift(-Anchors.Offset);
                    offset = (int)Math.Ceiling(PlannerNpcLayout.Offset(plan, bounds, new PlannerBounds(), screen.Left, out limited));
                }
                Place(offset);
                if (previous != offset || oldHidden != placeholder || limited != _limited)
                    Planner.LogLine("event=location_cards state=" + (placeholder ? "placeholder_hidden" : offset < 0 ? "shifted" : "native") +
                        " offset_ui=" + offset + " screen_limited=" + limited);
                _limited = limited;
            }
            catch (Exception ex)
            {
                _failed = true; Reset();
                Planner.LogLine("event=location_cards state=disabled_for_process error=" + ex);
            }
        }
    }
}
