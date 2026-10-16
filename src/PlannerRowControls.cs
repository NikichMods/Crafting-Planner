// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace CraftingPlanner
{
    // Added to supported live rows, never to the shared prefab or the native focus tree.
    internal sealed class PlannerRowControls : MonoBehaviour
    {
        private static readonly List<PlannerRowControls> Rows = new List<PlannerRowControls>();
        private const int Reserve = 80;
        private CraftItemGUI _row;
        private UIWidget _ingredients;
        private int _left, _right;
        private bool _shifted;
        private UIWidget _column;
        private UILabel _minus, _plus;
        private UILabel _quantity, _caption;
        private readonly List<UILabel> _texts = new List<UILabel>();
        private bool _failed;
        internal static void Ensure(CraftItemGUI row)
        {
            if (row == null) return;
            PlannerRowControls controls = row.GetComponent<PlannerRowControls>();
            if (controls == null)
            {
                if (!Eligible(row)) return;
                controls = row.gameObject.AddComponent<PlannerRowControls>();
                controls._row = row; Rows.Add(controls);
            }
            controls.Render();
        }
        private static bool Eligible(CraftItemGUI row)
        {
            return Planner.IsSupportedProject(row.current_craft) && row.selection_frame != null && row.ingredients_table != null && row.label_name != null &&
                (row.amount_buttons == null || !row.amount_buttons.activeSelf);
        }
        internal void Restore()
        {
            if (!_shifted || _ingredients == null) return;
            _ingredients.leftAnchor.absolute = _left; _ingredients.rightAnchor.absolute = _right;
            _ingredients.ResetAndUpdateAnchors();
            if (_row != null && _row.label_name != null) _row.label_name.ResetAndUpdateAnchors();
            _shifted = false;
        }
        private GameObject Child(string name, Transform parent)
        {
            GameObject go = new GameObject(name); go.layer = gameObject.layer; go.transform.SetParent(parent, false); return go;
        }
        private UILabel Text(string text, Transform parent, int x, int y, int width, int height)
        {
            UILabel label = Child("text", parent).AddComponent<UILabel>();
            PlannerCard.ApplyNativeFont(label, _row.label_name);
            label.fontSize = 13; label.overflowMethod = UILabel.Overflow.ClampContent; label.maxLineCount = 1;
            label.pivot = UIWidget.Pivot.Center; label.alignment = NGUIText.Alignment.Center;
            label.width = width; label.height = height; label.depth = _row.selection_frame.depth + 3;
            label.color = new Color(.75f, .74f, .68f); label.effectStyle = UILabel.Effect.Shadow;
            label.effectColor = new Color(.06f, .06f, .05f); label.effectDistance = new Vector2(1, 1);
            label.transform.localPosition = new Vector3(x, y, 0); label.text = text; _texts.Add(label); return label;
        }
        private UILabel Arrow(int delta, int x)
        {
            UILabel arrow = Text(delta < 0 ? "<" : ">", _column.transform, x, -28, PlannerLayout.ArrowWidth, 26);
            BoxCollider2D collider = arrow.gameObject.AddComponent<BoxCollider2D>(); collider.size = new Vector2(arrow.width, arrow.height);
            UIEventListener events = UIEventListener.Get(arrow.gameObject);
            events.onHover = delegate(GameObject go, bool over) { arrow.color = over ? new Color(1, .94f, .72f) : new Color(.75f, .74f, .68f); };
            events.onClick = delegate(GameObject go)
            {
                if (PlannerCalibration.IsOpen || GUIElements.me == null || !GUIElements.me.craft.is_shown_and_top || !Eligible(_row)) return;
                Planner.AdjustProject(_row.current_craft, delta);
            };
            return arrow;
        }
        private void Render()
        {
            if (_failed || _row == null) return;
            try
            {
                if (!Eligible(_row)) { Restore(); Hide(); return; }
                if (_ingredients == null)
                {
                    _ingredients = _row.ingredients_table.GetComponent<UIWidget>();
                    if (_ingredients == null) throw new InvalidOperationException("Native ingredient anchor widget unavailable");
                    _left = _ingredients.leftAnchor.absolute; _right = _ingredients.rightAnchor.absolute;
                }
                if (!_shifted)
                {
                    _ingredients.leftAnchor.absolute = _left - Reserve;
                    _ingredients.rightAnchor.absolute = _right - Reserve;
                    _shifted = true; _ingredients.ResetAndUpdateAnchors();
                    _row.label_name.ResetAndUpdateAnchors();
                }
                if (_column == null)
                {
                    _column = Child("CraftingPlannerColumn", transform).AddComponent<UIWidget>();
                    _column.pivot = UIWidget.Pivot.TopRight;
                    _column.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
                    _column.SetAnchor(_row.selection_frame.gameObject, 1, -Reserve, 0, 4, 1, -4, 1, -5);
                    _quantity = Text("", _column.transform, -38, -28, PlannerLayout.QuantityWidth, 22);
                    _quantity.overflowMethod = UILabel.Overflow.ShrinkContent;
                    _minus = Arrow(-1, -63); _plus = Arrow(1, -13);
                }
                _column.ResetAndUpdateAnchors();
                AlignIngredients();
                int quantity = Planner.QuantityFor(_row.current_craft);
                foreach (UILabel label in _texts) { PlannerCard.ApplyNativeFont(label, _row.label_name); label.fontSize = 13; }
                var items = GUIElements.me.craft.GetItemsList();
                UILabel title = GUIElements.me.craft.object_info != null ? GUIElements.me.craft.object_info.text_header : null;
                bool first = items != null && items.Count > 0 && items[0] == _row;
                if (title != null && first)
                {
                    if (_caption == null) _caption = Text(PlannerLocalization.PlanColumn, title.transform.parent, 0, 0, 72, 22);
                    PlannerCard.ApplyNativeFont(_caption, title); _caption.color = title.color; _caption.depth = title.depth + 1;
                    _caption.text = PlannerLocalization.PlanColumn;
                    Vector3[] corners = title.worldCorners;
                    Vector3 position = _quantity.transform.position; position.y = (corners[0].y + corners[2].y) / 2;
                    _caption.transform.position = position; _caption.gameObject.SetActive(true);
                }
                else if (_caption != null) _caption.gameObject.SetActive(false);
                _quantity.text = "×" + quantity;
                _minus.transform.localPosition = new Vector3(-63, -28, 0);
                _plus.transform.localPosition = new Vector3(-13, -28, 0);
                _plus.gameObject.SetActive(quantity == 0 || Planner.IsRepeatable(_row.current_craft));
                _column.gameObject.SetActive(true);
                UIEventListener.Get(_minus.gameObject).onHover(_minus.gameObject, UICamera.hoveredObject == _minus.gameObject);
                UIEventListener.Get(_plus.gameObject).onHover(_plus.gameObject, UICamera.hoveredObject == _plus.gameObject);
            }
            catch (Exception ex)
            {
                Restore(); Hide(); _failed = true;
                Planner.LogLine("event=plan_column result=disabled_for_row error=" + ex);
            }
        }
        private void AlignIngredients()
        {
            float right = float.MinValue;
            foreach (UIWidget widget in _row.ingredients_table.GetComponentsInChildren<UIWidget>())
            {
                if (widget == _ingredients || !widget.gameObject.activeInHierarchy) continue;
                foreach (Vector3 corner in widget.worldCorners) right = Math.Max(right, transform.InverseTransformPoint(corner).x);
            }
            if (right == float.MinValue) return;
            float target = transform.InverseTransformPoint(_column.worldCorners[0]).x - 7;
            int delta = (int)Math.Round(target - right);
            _ingredients.leftAnchor.absolute += delta; _ingredients.rightAnchor.absolute += delta;
            _ingredients.ResetAndUpdateAnchors(); _row.label_name.ResetAndUpdateAnchors();
        }
        private void Hide() { if (_column != null) _column.gameObject.SetActive(false); if (_caption != null) _caption.gameObject.SetActive(false); }
        internal static void RefreshAll()
        {
            for (int i = Rows.Count - 1; i >= 0; i--) { if (Rows[i] == null) Rows.RemoveAt(i); else Rows[i].Render(); }
        }
        internal static void RestoreAll()
        {
            foreach (var row in Rows) if (row != null) { row.Restore(); row.Hide(); }
        }
        internal static void DestroyAll()
        {
            foreach (var row in Rows.ToArray()) if (row != null) { row.Restore(); row.Hide(); UnityEngine.Object.Destroy(row); }
            Rows.Clear();
        }
        private void OnDestroy() { Restore(); if (_column != null) UnityEngine.Object.Destroy(_column.gameObject); if (_caption != null) UnityEngine.Object.Destroy(_caption.gameObject); Rows.Remove(this); }
    }
    [HarmonyPatch(typeof(CraftItemGUI), "Redraw")]
    internal static class PlannerRowRedrawPatch
    {
        private static void Prefix(CraftItemGUI __instance)
        {
            var controls = __instance.GetComponent<PlannerRowControls>(); if (controls != null) controls.Restore();
        }
        private static void Postfix(CraftItemGUI __instance) { PlannerRowControls.Ensure(__instance); }
    }
}
