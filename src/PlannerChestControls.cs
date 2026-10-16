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
        private static UILabel _tipsPreview;
        private static string _lastGamepadTips = "";
        private static int _tipsLeft, _tipsRight;
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
            if (_failed) return;
            try
            {
                GUIElements gui = GUIElements.me;
                if (gui == null || gui.chest == null || !gui.chest.is_shown_and_top || !Planner.HasGoals)
                { RestoreTips(); if (_tipsPreview != null) _tipsPreview.gameObject.SetActive(false); if (_button != null) _button.gameObject.SetActive(false); return; }
                UILabel native = gui.chest.button_tips != null ? gui.chest.button_tips.label : null;
                if (native == null) return;
                if (_nativeTips != native)
                {
                    RestoreTips(); _nativeTips = native; _tipsLeft = native.leftAnchor.absolute; _tipsRight = native.rightAnchor.absolute;
                }
                PlannerStyle style = PlannerStyle.Current;
                native.leftAnchor.absolute = _tipsLeft + (int)style.ChestTipsX; native.rightAnchor.absolute = _tipsRight + (int)style.ChestTipsX;
                native.ResetAndUpdateAnchors();
                bool preview = PlannerCalibration.IsOpen && !string.IsNullOrEmpty(_lastGamepadTips) && (!native.gameObject.activeInHierarchy || string.IsNullOrEmpty(native.text));
                if (preview)
                {
                    if (_tipsPreview == null) _tipsPreview = Child("native-tips-preview", gui.chest.transform, native.gameObject.layer).AddComponent<UILabel>();
                    PlannerCard.ApplyNativeFont(_tipsPreview, native); _tipsPreview.color = native.color; _tipsPreview.depth = native.depth + 1;
                    _tipsPreview.pivot = UIWidget.Pivot.Center; _tipsPreview.alignment = native.alignment; _tipsPreview.maxLineCount = 1;
                    _tipsPreview.SetAnchor(native.gameObject, 0, 0, 0, 0, 1, 0, 1, 0); _tipsPreview.ResetAndUpdateAnchors();
                    _tipsPreview.text = _lastGamepadTips; _tipsPreview.gameObject.SetActive(true);
                }
                else if (_tipsPreview != null) _tipsPreview.gameObject.SetActive(false);
                if (_button == null)
                {
                    Feedback.Clear();
                    _button = Child("CraftingPlannerTakeNeeded", gui.chest.transform, native.gameObject.layer).AddComponent<UILabel>();
                    _button.pivot = UIWidget.Pivot.Center; _button.depth = native.depth + 3;
                    _button.alignment = NGUIText.Alignment.Center; _button.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
                    _button.gameObject.AddComponent<BoxCollider2D>();
                    _label = _button;
                    var listener = UIEventListener.Get(_button.gameObject);
                    listener.onHover = delegate(GameObject go, bool over) { _label.color = over ? new Color(1, .94f, .72f) : native.color; };
                    listener.onClick = delegate(GameObject go)
                    {
                        if (!PlannerCalibration.IsOpen && GUIElements.me != null && GUIElements.me.chest.is_shown_and_top) Planner.TakeNeeded(GUIElements.me.chest, "mouse_storage_row");
                    };
                }
                PlannerCard.ApplyNativeFont(_label, native);
                _label.overflowMethod = UILabel.Overflow.ResizeFreely;
                _label.text = GameKeyTip.Get(LazyInput.gamepad_active ? GameKey.Option2 : GameKey.Work, PlannerLocalization.TakeNeeded, true, false, false, false);
                if (string.IsNullOrEmpty(_label.text)) _label.text = PlannerLocalization.TakeNeeded;
                int width = Math.Max(100, (int)Math.Ceiling(_label.printedSize.x) + 20);
                int height = Math.Max(28, (int)Math.Ceiling(_label.printedSize.y) + 8);
                _label.overflowMethod = UILabel.Overflow.ClampContent;
                int x = (int)(style.TakeX - style.ChestTipsX), y = (int)style.TakeY;
                _button.SetAnchor(native.gameObject, 0, -width - 8 + x, .5f, -height / 2 + y, 0, -8 + x, .5f, height / 2 + y);
                _button.ResetAndUpdateAnchors();
                BoxCollider2D collider = _button.GetComponent<BoxCollider2D>(); collider.size = new Vector2(_button.width, _button.height);
                _button.gameObject.SetActive(true);
                _label.color = UICamera.hoveredObject == _button.gameObject ? new Color(1, .94f, .72f) : native.color;
                DrawFeedback(native, gui.craft.craft_item_prefab.item_gui.x1.icon);
            }
            catch (Exception ex) { _failed = true; Destroy(); Planner.LogLine("event=storage_controls result=disabled_for_session error=" + ex); }
        }
        internal static void ObserveNativeTips(ChestGUI chest)
        {
            if (!PlannerCalibration.IsOpen && LazyInput.gamepad_active && chest.is_shown_and_top && chest.button_tips != null && chest.button_tips.label != null && !string.IsNullOrEmpty(chest.button_tips.label.text))
                _lastGamepadTips = chest.button_tips.label.text;
        }
        private static void DrawFeedback(UILabel native, UI2DSprite template)
        {
            foreach (GameObject go in Feedback) if (go != null) { go.SetActive(false); UnityEngine.Object.Destroy(go); }
            Feedback.Clear();
            Color green = new Color(.533f, .843f, .486f), red = new Color(.894f, .467f, .400f);
            PlannerStyle style = PlannerStyle.Current;
            int line = Math.Max(22, native.fontSize + 4) + (int)style.FeedbackLineGap;
            int left = -_button.width / 2 + (int)(style.FeedbackX - style.TakeX), x = left, y = -_button.height / 2 + (int)(style.FeedbackY - style.TakeY);
            int itemStep = 48 + (int)style.FeedbackGap;
            if (_moved.Count > 0)
            {
                UILabel prefix = Text(PlannerLocalization.Taken, _button.transform, native, x, y, 100, green);
                x += Math.Min(100, (int)Math.Ceiling(prefix.printedSize.x)) + (int)style.FeedbackGap;
                foreach (var pair in _moved)
                {
                    if (x + itemStep > left + 324) { x = left; y -= line; }
                    ItemDefinition definition = GameBalance.me.GetDataOrNull<ItemDefinition>(pair.Key);
                    Sprite sprite = definition != null ? EasySpritesCollection.GetSprite(definition.GetIcon(), false, "") : null;
                    if (sprite != null && template != null)
                    {
                        UI2DSprite icon = UnityEngine.Object.Instantiate(template, _button.transform, false);
                        icon.SetAnchor((GameObject)null); icon.pivot = UIWidget.Pivot.Center; icon.width = icon.height = 20; icon.fixedAspect = false;
                        icon.transform.localScale = Vector3.one; icon.transform.localPosition = new Vector3(x + 10, y, 0);
                        icon.depth = native.depth + 4; icon.sprite2D = sprite; icon.gameObject.SetActive(true); Feedback.Add(icon.gameObject);
                        Text(pair.Value.ToString(), _button.transform, native, x + 24, y, 30, green); x += itemStep;
                    }
                    else
                    {
                        y -= line; x = left;
                        Text(Planner.GetItemName(pair.Key) + " ×" + pair.Value, _button.transform, native, x, y, 324, green); y -= line;
                    }
                }
            }
            if (_notice != TransferNotice.None)
            {
                if (_moved.Count > 0) y -= line;
                Text(PlannerLocalization.TransferMessage(_notice), _button.transform, native, left, y, 324, _notice == TransferNotice.Complete ? green : red);
            }
        }
        internal static void Show(List<KeyValuePair<string, int>> moved, TransferNotice notice) { _moved = moved; _notice = notice; Refresh(); }
        private static void RestoreTips()
        {
            if (_nativeTips != null) { _nativeTips.leftAnchor.absolute = _tipsLeft; _nativeTips.rightAnchor.absolute = _tipsRight; _nativeTips.ResetAndUpdateAnchors(); }
            _nativeTips = null;
        }
        internal static void Clear() { RestoreTips(); _lastGamepadTips = ""; if (_tipsPreview != null) _tipsPreview.gameObject.SetActive(false); _moved.Clear(); _notice = TransferNotice.None; if (_button != null) _button.gameObject.SetActive(false); }
        internal static void Destroy() { RestoreTips(); if (_tipsPreview != null) UnityEngine.Object.Destroy(_tipsPreview.gameObject); _tipsPreview = null; if (_button != null) { _button.gameObject.SetActive(false); UnityEngine.Object.Destroy(_button.gameObject); } _button = null; Feedback.Clear(); }
    }
    [HarmonyPatch(typeof(BaseGUI), "Update")]
    internal static class PlannerChestKeyboardPatch
    {
        private static void Postfix(BaseGUI __instance)
        {
            ChestGUI chest = __instance as ChestGUI;
            if (chest != null) PlannerChestControls.ObserveNativeTips(chest);
            if (chest == null || !chest.is_shown_and_top || LazyInput.gamepad_active || PlannerCalibration.IsOpen || !LazyInput.GetKeyDown(GameKey.Work)) return;
            if (Planner.TakeNeeded(chest, "keyboard_storage_work")) LazyInput.WaitForRelease(GameKey.Work);
        }
    }
}
