// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace CraftingPlanner
{
    internal enum TransferNotice { None, NoStock, NoSpace, Complete, Unavailable }
    internal static class PlannerChestControls
    {
        private static UILabel _button;
        private static UILabel _label;
        private static UILabel _nativeTips;
        private static readonly List<UILabel> Actions = new List<UILabel>();
        private static string[] _nativeBlocks = new string[0], _lastGamepadBlocks = new string[0];
        private static string _lastGamepadTake = "";
        private static UIWidget _feedbackRoot;
        private static float _tipsAlpha;
        private static bool _rendering;
        private static bool _failed;
        private static readonly List<GameObject> Feedback = new List<GameObject>();
        private static List<KeyValuePair<string, int>> _moved = new List<KeyValuePair<string, int>>();
        private static TransferNotice _notice;
        private static GameObject Child(string name, Transform parent, int layer)
        {
            GameObject go = new GameObject(name); go.layer = layer; go.transform.SetParent(parent, false); return go;
        }
        private static UILabel Text(string text, Transform parent, UILabel native, int x, int y, int width, Color color)
        {
            GameObject go = Child("result-text", parent, native.gameObject.layer); Feedback.Add(go);
            UILabel label = go.AddComponent<UILabel>(); PlannerCard.ApplyNativeFont(label, native);
            label.pivot = UIWidget.Pivot.Left; label.alignment = NGUIText.Alignment.Left;
            label.width = width; label.height = Math.Max(22, native.fontSize + 4); label.maxLineCount = 1; label.overflowMethod = UILabel.Overflow.ClampContent;
            label.color = color; label.depth = native.depth + 4; label.text = text;
            go.transform.localPosition = new Vector3(x, y, 0); return label;
        }
        internal static void Refresh()
        {
            if (_failed || _rendering) return;
            _rendering = true;
            try
            {
                GUIElements gui = GUIElements.me;
                if (gui == null || gui.chest == null || !gui.chest.is_shown_and_top || !Planner.HasGoals)
                { RestoreTips(); HideOwned(); return; }
                UILabel native = gui.chest.button_tips != null ? gui.chest.button_tips.label : null;
                if (native == null) { RestoreTips(); HideOwned(); return; }
                if (_nativeTips != native)
                {
                    RestoreTips(); _nativeTips = native; _tipsAlpha = native.alpha;
                }
                PlannerStyle style = PlannerStyle.Current;
                if (_button == null)
                {
                    _button = Child("CraftingPlannerTakeNeeded", gui.chest.transform, native.gameObject.layer).AddComponent<UILabel>();
                    _button.gameObject.AddComponent<BoxCollider2D>(); _label = _button;
                    var listener = UIEventListener.Get(_button.gameObject);
                    listener.onHover = delegate(GameObject go, bool over) { if (_nativeTips != null) _label.color = TipColor(over); };
                    listener.onClick = delegate(GameObject go)
                    {
                        if (!PlannerCalibration.IsOpen && GUIElements.me != null && GUIElements.me.chest.is_shown_and_top) Planner.TakeNeeded(GUIElements.me.chest, "mouse_storage_row");
                    };
                }
                bool preview = PlannerCalibration.IsOpen && _lastGamepadBlocks.Length > 0;
                string take = preview ? _lastGamepadTake : GameKeyTip.Get(LazyInput.gamepad_active ? GameKey.Option2 : GameKey.Work, PlannerLocalization.TakeNeeded, true, false, false, false);
                if (string.IsNullOrEmpty(take)) take = PlannerLocalization.TakeNeeded;
                string[] blocks = preview ? _lastGamepadBlocks : LazyInput.gamepad_active ? _nativeBlocks : new string[0];
                while (Actions.Count < blocks.Length)
                    Actions.Add(Child("native-action", gui.chest.transform, native.gameObject.layer).AddComponent<UILabel>());
                int width = Measure(_button, take, native), height = _button.height;
                Place(_button, native, (int)style.ChestRowX, (int)style.ChestRowY, width, height);
                _button.color = TipColor(UICamera.hoveredObject == _button.gameObject);
                BoxCollider2D collider = _button.GetComponent<BoxCollider2D>(); collider.size = new Vector2(width, height);
                int[] widths = new int[blocks.Length + 1];
                for (int i = 0; i < blocks.Length; i++) widths[i] = Measure(Actions[i], blocks[i], native);
                widths[blocks.Length] = width;
                int[] starts = PlannerLayout.RightAlignedStarts(widths, (int)style.ChestRowX, (int)style.ChestActionGap);
                for (int i = 0; i < Actions.Count; i++)
                {
                    if (i >= blocks.Length) { Actions[i].gameObject.SetActive(false); continue; }
                    Place(Actions[i], native, starts[i] + widths[i], (int)style.ChestRowY, widths[i], Actions[i].height);
                    Actions[i].color = TipColor(false);
                }
                // Preserve the result's previous independent origin; action-row offsets cannot move it.
                if (_feedbackRoot == null) _feedbackRoot = Child("transfer-result", gui.chest.transform, native.gameObject.layer).AddComponent<UIWidget>();
                int legacyWidth = Math.Max(100, width + 20), legacyHeight = Math.Max(28, (int)Math.Ceiling(_button.printedSize.y) + 8);
                int fx = (int)style.FeedbackX - legacyWidth - 8, fy = (int)style.FeedbackY - legacyHeight / 2;
                _feedbackRoot.SetAnchor(native.gameObject, 0, fx - 1, .5f, fy - 1, 0, fx + 1, .5f, fy + 1);
                _feedbackRoot.ResetAndUpdateAnchors(); _feedbackRoot.gameObject.SetActive(true);
                DrawFeedback(native, gui.craft.craft_item_prefab.item_gui.x1.icon);
                native.alpha = 0;
            }
            catch (Exception ex) { _failed = true; Destroy(); Planner.LogLine("event=storage_controls result=disabled_for_session error=" + ex); }
            finally { _rendering = false; }
        }
        private static Color TipColor(bool hover)
        {
            if (hover) return new Color(1, .94f, .72f, _tipsAlpha);
            Color color = _nativeTips.color; color.a = _tipsAlpha; return color;
        }
        private static int Measure(UILabel label, string text, UILabel native)
        {
            PlannerCard.ApplyNativeFont(label, native);
            label.pivot = UIWidget.Pivot.Center; label.alignment = NGUIText.Alignment.Center;
            label.maxLineCount = 1; label.overflowMethod = UILabel.Overflow.ResizeFreely;
            label.text = text;
            int width = Math.Max(1, (int)Math.Ceiling(label.printedSize.x));
            label.height = Math.Max(22, (int)Math.Ceiling(label.printedSize.y));
            label.width = width; label.overflowMethod = UILabel.Overflow.ClampContent;
            label.depth = native.depth + 3; return width;
        }
        private static void Place(UILabel label, UILabel native, int right, int y, int width, int height)
        {
            label.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
            label.SetAnchor(native.gameObject, 1, right - width, .5f, y - height / 2, 1, right, .5f, y + (height + 1) / 2);
            // Horizontal position belongs to the chest frame, not the native label's current text width.
            label.leftAnchor.target = label.rightAnchor.target = native.rightAnchor.target;
            label.leftAnchor.relative = label.rightAnchor.relative = native.rightAnchor.relative;
            label.leftAnchor.absolute = native.rightAnchor.absolute + right - width;
            label.rightAnchor.absolute = native.rightAnchor.absolute + right;
            label.ResetAndUpdateAnchors(); label.gameObject.SetActive(true);
        }
        internal static void CaptureTips(ButtonTipsStr source, IEnumerable<GameKeyTip> tips)
        {
            GUIElements gui = GUIElements.me;
            if (_failed || gui == null || gui.chest == null || gui.chest.button_tips != source || !LazyInput.gamepad_active) return;
            var blocks = new List<string>();
            foreach (GameKeyTip tip in tips) { string text = tip == null ? "" : tip.ToString(); if (!string.IsNullOrEmpty(text)) blocks.Add(text); }
            _nativeBlocks = blocks.ToArray();
            if (!PlannerCalibration.IsOpen)
            {
                _lastGamepadBlocks = _nativeBlocks;
                _lastGamepadTake = GameKeyTip.Get(GameKey.Option2, PlannerLocalization.TakeNeeded, true, false, false, false);
            }
            Refresh();
        }
        private static void DrawFeedback(UILabel native, UI2DSprite template)
        {
            foreach (GameObject go in Feedback) if (go != null) { go.SetActive(false); UnityEngine.Object.Destroy(go); }
            Feedback.Clear();
            Color green = new Color(.533f, .843f, .486f), red = new Color(.894f, .467f, .400f);
            PlannerStyle style = PlannerStyle.Current;
            int line = Math.Max(22, native.fontSize + 4) + (int)style.FeedbackLineGap;
            int left = 0, x = left, y = 0;
            int itemStep = 48 + (int)style.FeedbackGap;
            if (_moved.Count > 0)
            {
                UILabel prefix = Text(PlannerLocalization.Taken, _feedbackRoot.transform, native, x, y, 100, green);
                x += Math.Min(100, (int)Math.Ceiling(prefix.printedSize.x)) + (int)style.FeedbackGap;
                foreach (var pair in _moved)
                {
                    if (x + itemStep > left + 324) { x = left; y -= line; }
                    ItemDefinition definition = GameBalance.me.GetDataOrNull<ItemDefinition>(pair.Key);
                    Sprite sprite = definition != null ? EasySpritesCollection.GetSprite(definition.GetIcon(), false, "") : null;
                    if (sprite != null && template != null)
                    {
                        UI2DSprite icon = UnityEngine.Object.Instantiate(template, _feedbackRoot.transform, false);
                        icon.SetAnchor((GameObject)null); icon.pivot = UIWidget.Pivot.Center; icon.width = icon.height = 20; icon.fixedAspect = false;
                        icon.transform.localScale = Vector3.one; icon.transform.localPosition = new Vector3(x + 10, y, 0);
                        icon.depth = native.depth + 4; icon.sprite2D = sprite; icon.gameObject.SetActive(true); Feedback.Add(icon.gameObject);
                        Text(pair.Value.ToString(), _feedbackRoot.transform, native, x + 24, y, 30, green); x += itemStep;
                    }
                    else
                    {
                        y -= line; x = left;
                        Text(Planner.GetItemName(pair.Key) + " ×" + pair.Value, _feedbackRoot.transform, native, x, y, 324, green); y -= line;
                    }
                }
            }
            if (_notice != TransferNotice.None)
            {
                if (_moved.Count > 0) y -= line;
                Text(PlannerLocalization.TransferMessage(_notice), _feedbackRoot.transform, native, left, y, 324, _notice == TransferNotice.Complete ? green : red);
            }
        }
        internal static void Show(List<KeyValuePair<string, int>> moved, TransferNotice notice) { _moved = moved; _notice = notice; Refresh(); }
        private static void RestoreTips()
        {
            if (_nativeTips != null) _nativeTips.alpha = _tipsAlpha;
            _nativeTips = null;
        }
        private static void HideOwned()
        {
            foreach (UILabel action in Actions) if (action != null) action.gameObject.SetActive(false);
            if (_button != null) _button.gameObject.SetActive(false);
            if (_feedbackRoot != null) _feedbackRoot.gameObject.SetActive(false);
        }
        internal static void Clear()
        {
            RestoreTips(); HideOwned(); _nativeBlocks = _lastGamepadBlocks = new string[0]; _lastGamepadTake = "";
            _moved.Clear(); _notice = TransferNotice.None;
        }
        internal static void Destroy()
        {
            Clear();
            foreach (UILabel action in Actions) if (action != null) UnityEngine.Object.Destroy(action.gameObject);
            Actions.Clear();
            if (_button != null) UnityEngine.Object.Destroy(_button.gameObject); _button = _label = null;
            if (_feedbackRoot != null) UnityEngine.Object.Destroy(_feedbackRoot.gameObject); _feedbackRoot = null;
            Feedback.Clear();
        }
    }
    [HarmonyPatch(typeof(ButtonTipsStr), "Print", new[] { typeof(List<GameKeyTip>), typeof(string) })]
    internal static class PlannerChestTipsPatch
    {
        private static void Postfix(ButtonTipsStr __instance, List<GameKeyTip> __0) { PlannerChestControls.CaptureTips(__instance, __0); }
    }
    [HarmonyPatch(typeof(ButtonTipsStr), "Print", new[] { typeof(GameKeyTip) })]
    internal static class PlannerChestSingleTipPatch
    {
        private static void Postfix(ButtonTipsStr __instance, GameKeyTip __0) { PlannerChestControls.CaptureTips(__instance, new[] { __0 }); }
    }
    [HarmonyPatch(typeof(BaseGUI), "Update")]
    internal static class PlannerChestKeyboardPatch
    {
        private static void Postfix(BaseGUI __instance)
        {
            ChestGUI chest = __instance as ChestGUI;
            if (chest == null || !chest.is_shown_and_top || LazyInput.gamepad_active || PlannerCalibration.IsOpen || !LazyInput.GetKeyDown(GameKey.Work)) return;
            if (Planner.TakeNeeded(chest, "keyboard_storage_work")) LazyInput.WaitForRelease(GameKey.Work);
        }
    }
}
