// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using UnityEngine;
namespace CraftingPlanner
{
    // Own only supported-menu tip visuals; native action order, state and input remain host-owned.
    internal static class PlannerBuildTips
    {
        private static UILabel _native, _caption, _fallback;
        private static UIWidget _arrows;
        private static UISprite _left, _right;
        private static readonly List<UILabel> Actions = new List<UILabel>();
        private static string[] _blocks = new string[0], _preview = new string[0];
        private static float _alpha;
        private static bool _rendering, _failed;
        private static GameObject Child(string name, Transform parent, int layer)
        {
            var go = new GameObject(name); go.layer = layer; go.transform.SetParent(parent, false); return go;
        }
        internal static void Capture(ButtonTipsStr source, IEnumerable<GameKeyTip> tips)
        {
            GUIElements gui = GUIElements.me;
            if (_failed || gui == null || gui.craft == null || gui.craft.button_tips != source) return;
            var blocks = new List<string>();
            foreach (GameKeyTip tip in tips) { string text = tip == null ? "" : tip.ToString(); if (!string.IsNullOrEmpty(text)) blocks.Add(text); }
            bool changed = blocks.Count != _blocks.Length;
            if (!changed) for (int i = 0; i < blocks.Count; i++) if (blocks[i] != _blocks[i]) { changed = true; break; }
            if (changed) _blocks = blocks.ToArray();
            if (LazyInput.gamepad_active && !PlannerCalibration.IsOpen) _preview = _blocks;
            if (changed) Refresh();
        }
        private static int Measure(UILabel label, UILabel native, string text, int fontSize)
        {
            PlannerCard.ApplyNativeFont(label, native); label.fontSize = fontSize;
            label.applyGradient = native.applyGradient; label.gradientTop = native.gradientTop; label.gradientBottom = native.gradientBottom;
            Color color = native.color; color.a = _alpha; label.color = color;
            label.pivot = UIWidget.Pivot.Center; label.alignment = NGUIText.Alignment.Center; label.maxLineCount = 1;
            label.overflowMethod = UILabel.Overflow.ResizeFreely; label.text = text;
            int width = Math.Max(1, (int)Math.Ceiling(label.printedSize.x));
            label.height = Math.Max(22, (int)Math.Ceiling(label.printedSize.y)); label.width = width;
            label.overflowMethod = UILabel.Overflow.ClampContent; label.depth = native.depth + 2; return width;
        }
        private static void Place(UIWidget widget, UILabel native, int x, int width, int height, int y)
        {
            widget.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
            widget.SetAnchor(native.gameObject, 1, x, .5f, y - height / 2, 1, x + width, .5f, y + (height + 1) / 2);
            // The native tip's horizontal owner is the menu frame background, not its text bounds.
            widget.leftAnchor.target = widget.rightAnchor.target = native.rightAnchor.target;
            widget.leftAnchor.relative = widget.rightAnchor.relative = 1;
            widget.leftAnchor.absolute = x; widget.rightAnchor.absolute = x + width;
            widget.ResetAndUpdateAnchors(); widget.gameObject.SetActive(true);
        }
        internal static void Refresh()
        {
            if (_failed || _rendering) return;
            _rendering = true;
            try
            {
                GUIElements gui = GUIElements.me;
                if (gui == null || gui.craft == null || !gui.craft.is_shown_and_top || !PlannerRowControls.HasSupportedRows(gui.craft)) { Hide(); return; }
                UILabel native = gui.craft.button_tips != null ? gui.craft.button_tips.label : null;
                if (native == null || native.rightAnchor.target == null) { Hide(); return; }
                if (_native != native) { Restore(); _native = native; _alpha = native.alpha; }
                UIPanel panel = native.GetComponentInParent<UIPanel>();
                if (panel == null) { Hide(); return; }
                if (_caption == null)
                {
                    _caption = Child("planner-action-caption", panel.transform, native.gameObject.layer).AddComponent<UILabel>();
                    _arrows = Child("planner-action-arrows", panel.transform, native.gameObject.layer).AddComponent<UIWidget>();
                    _left = Child("left", _arrows.transform, native.gameObject.layer).AddComponent<UISprite>();
                    _right = Child("right", _arrows.transform, native.gameObject.layer).AddComponent<UISprite>();
                    _fallback = Child("fallback", _arrows.transform, native.gameObject.layer).AddComponent<UILabel>();
                }
                PlannerStyle style = PlannerStyle.Current;
                string[] blocks = PlannerCalibration.IsOpen && _preview.Length > 0 ? _preview : LazyInput.gamepad_active ? _blocks : new string[0];
                while (Actions.Count < blocks.Length) Actions.Add(Child("native-build-action", panel.transform, native.gameObject.layer).AddComponent<UILabel>());
                int captionWidth = Measure(_caption, native, PlannerLocalization.PlanColumn, (int)style.MenuCaptionFont);
                int arrowWidth = DrawArrows(native);
                int[] widths = new int[blocks.Length + 1]; widths[0] = arrowWidth + 6 + captionWidth;
                for (int i = 0; i < blocks.Length; i++) widths[i + 1] = Measure(Actions[i], native, blocks[i], native.fontSize);
                int[] starts = PlannerLayout.RightAlignedStarts(widths, -8 + (int)style.MenuCaptionX, (int)style.MenuActionGap);
                int y = (int)style.MenuCaptionY;
                Place(_arrows, native, starts[0], arrowWidth, 22, y);
                Place(_caption, native, starts[0] + arrowWidth + 6, captionWidth, _caption.height, y);
                for (int i = 0; i < Actions.Count; i++)
                {
                    if (i < blocks.Length) Place(Actions[i], native, starts[i + 1], widths[i + 1], Actions[i].height, y);
                    else Actions[i].gameObject.SetActive(false);
                }
                native.alpha = 0;
            }
            catch (Exception ex) { _failed = true; Destroy(); Planner.LogLine("event=build_tips result=disabled_for_session error=" + ex); }
            finally { _rendering = false; }
        }
        private static int DrawArrows(UILabel native)
        {
            UIAtlas atlas = native.bitmapFont != null ? native.bitmapFont.atlas : null;
            bool sprites = atlas != null && atlas.GetSprite("gp_SW_arrow_L") != null && atlas.GetSprite("gp_SW_arrow_R") != null;
            _left.gameObject.SetActive(sprites); _right.gameObject.SetActive(sprites); _fallback.gameObject.SetActive(!sprites);
            if (sprites)
            {
                UISprite[] icons = { _left, _right }; string[] names = { "gp_SW_arrow_L", "gp_SW_arrow_R" };
                for (int i = 0; i < icons.Length; i++)
                {
                    icons[i].atlas = atlas; icons[i].spriteName = names[i]; icons[i].width = icons[i].height = 18;
                    icons[i].pivot = UIWidget.Pivot.Center; icons[i].depth = native.depth + 2;
                    icons[i].color = new Color(1, 1, 1, .9f * _alpha); icons[i].transform.localPosition = new Vector3(i == 0 ? -10 : 10, 0, 0);
                }
                return 38;
            }
            Measure(_fallback, native, "< >", (int)PlannerStyle.Current.MenuCaptionFont);
            _fallback.transform.localPosition = Vector3.zero; return _fallback.width;
        }
        private static void Restore() { if (_native != null) _native.alpha = _alpha; _native = null; }
        private static void Hide()
        {
            Restore();
            if (_caption != null) _caption.gameObject.SetActive(false);
            if (_arrows != null) _arrows.gameObject.SetActive(false);
            foreach (UILabel label in Actions) if (label != null) label.gameObject.SetActive(false);
        }
        internal static void Clear() { Hide(); _blocks = _preview = new string[0]; }
        internal static void Destroy()
        {
            Clear();
            if (_caption != null) UnityEngine.Object.Destroy(_caption.gameObject); _caption = null;
            if (_arrows != null) UnityEngine.Object.Destroy(_arrows.gameObject); _arrows = null; _left = _right = null; _fallback = null;
            foreach (UILabel label in Actions) if (label != null) UnityEngine.Object.Destroy(label.gameObject);
            Actions.Clear(); _blocks = _preview = new string[0];
        }
    }
}
