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
        private static UITexture _button, _fill;
        private static UILabel _label;
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
            label.fontSize = 13; label.pivot = UIWidget.Pivot.Left; label.alignment = NGUIText.Alignment.Left;
            label.width = width; label.height = 22; label.maxLineCount = 1; label.overflowMethod = UILabel.Overflow.ClampContent;
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
                { if (_button != null) _button.gameObject.SetActive(false); return; }
                UILabel native = gui.chest.button_tips != null ? gui.chest.button_tips.label : null;
                if (native == null) return;
                if (_button == null)
                {
                    Feedback.Clear();
                    _button = Child("CraftingPlannerTakeNeeded", gui.chest.transform, native.gameObject.layer).AddComponent<UITexture>();
                    _button.mainTexture = Texture2D.whiteTexture; _button.pivot = UIWidget.Pivot.Center; _button.depth = native.depth + 1;
                    _button.color = new Color(.50f, .43f, .30f, .95f); _button.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
                    _button.gameObject.AddComponent<BoxCollider2D>();
                    _fill = Child("button-fill", _button.transform, native.gameObject.layer).AddComponent<UITexture>();
                    _fill.mainTexture = Texture2D.whiteTexture; _fill.depth = native.depth + 2;
                    _fill.SetAnchor(_button.gameObject, 0, 1, 0, 1, 1, -1, 1, -1); _fill.color = new Color(.13f, .12f, .10f, .9f);
                    _label = Child("button-label", _button.transform, native.gameObject.layer).AddComponent<UILabel>();
                    _label.pivot = UIWidget.Pivot.Center; _label.alignment = NGUIText.Alignment.Center; _label.depth = native.depth + 3;
                    var listener = UIEventListener.Get(_button.gameObject);
                    listener.onHover = delegate(GameObject go, bool over) { _fill.color = over ? new Color(.28f, .23f, .15f, .95f) : new Color(.13f, .12f, .10f, .9f); };
                    listener.onClick = delegate(GameObject go)
                    {
                        if (!PlannerCalibration.IsOpen && GUIElements.me != null && GUIElements.me.chest.is_shown_and_top) Planner.TakeNeeded(GUIElements.me.chest, "mouse_storage_row");
                    };
                }
                PlannerCard.ApplyNativeFont(_label, native);
                _label.overflowMethod = UILabel.Overflow.ResizeFreely;
                _label.text = GameKeyTip.Get(LazyInput.gamepad_active ? GameKey.Option2 : GameKey.Work, PlannerLocalization.TakeNeeded, true, false, false, false);
                int width = Math.Max(100, (int)Math.Ceiling(_label.printedSize.x) + 20);
                int height = Math.Max(28, (int)Math.Ceiling(_label.printedSize.y) + 8);
                _label.overflowMethod = UILabel.Overflow.ClampContent;
                _label.SetAnchor(_button.gameObject, 0, 8, 0, 4, 1, -8, 1, -4);
                _button.SetAnchor(native.gameObject, 0, -width - 8, .5f, -height / 2, 0, -8, .5f, height / 2);
                _button.ResetAndUpdateAnchors();
                BoxCollider2D collider = _button.GetComponent<BoxCollider2D>(); collider.size = new Vector2(_button.width, _button.height);
                _button.gameObject.SetActive(true);
                DrawFeedback(native, gui.craft.craft_item_prefab.item_gui.x1.icon);
            }
            catch (Exception ex) { _failed = true; Destroy(); Planner.LogLine("event=storage_controls result=disabled_for_session error=" + ex); }
        }
        private static void DrawFeedback(UILabel native, UI2DSprite template)
        {
            foreach (GameObject go in Feedback) if (go != null) { go.SetActive(false); UnityEngine.Object.Destroy(go); }
            Feedback.Clear();
            Color green = new Color(.533f, .843f, .486f), red = new Color(.894f, .467f, .400f);
            int x = 0, y = 30;
            if (_moved.Count > 0)
            {
                UILabel prefix = Text(PlannerLocalization.Taken, _button.transform, native, x, y, 100, green);
                x += Math.Min(100, (int)Math.Ceiling(prefix.printedSize.x)) + 8;
                foreach (var pair in _moved)
                {
                    if (x + 54 > 324) { x = 0; y += 25; }
                    ItemDefinition definition = GameBalance.me.GetDataOrNull<ItemDefinition>(pair.Key);
                    Sprite sprite = definition != null ? EasySpritesCollection.GetSprite(definition.GetIcon(), false, "") : null;
                    if (sprite != null && template != null)
                    {
                        UI2DSprite icon = UnityEngine.Object.Instantiate(template, _button.transform, false);
                        icon.SetAnchor((GameObject)null); icon.pivot = UIWidget.Pivot.Center; icon.width = icon.height = 20; icon.fixedAspect = false;
                        icon.transform.localScale = Vector3.one; icon.transform.localPosition = new Vector3(x + 10, y, 0);
                        icon.depth = native.depth + 4; icon.sprite2D = sprite; icon.gameObject.SetActive(true); Feedback.Add(icon.gameObject);
                        Text(pair.Value.ToString(), _button.transform, native, x + 24, y, 30, green); x += 54;
                    }
                    else
                    {
                        y += 25; x = 0;
                        Text(Planner.GetItemName(pair.Key) + " ×" + pair.Value, _button.transform, native, x, y, 324, green); y += 25;
                    }
                }
            }
            if (_notice != TransferNotice.None)
            {
                if (_moved.Count > 0) y += 25;
                Text(PlannerLocalization.TransferMessage(_notice), _button.transform, native, 0, y, 324, red);
            }
        }
        internal static void Show(List<KeyValuePair<string, int>> moved, TransferNotice notice) { _moved = moved; _notice = notice; Refresh(); }
        internal static void Clear() { _moved.Clear(); _notice = TransferNotice.None; if (_button != null) _button.gameObject.SetActive(false); }
        internal static void Destroy() { if (_button != null) { _button.gameObject.SetActive(false); UnityEngine.Object.Destroy(_button.gameObject); } _button = null; Feedback.Clear(); }
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
