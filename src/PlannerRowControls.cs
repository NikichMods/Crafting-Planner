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
        private static UITable _nativeList;
        private const int Reserve = 80;
        private CraftItemGUI _row;
        private UIWidget _ingredients;
        private UITable _ingredientLayout;
        private int _left, _right, _top, _bottom;
        private UIBasicSprite _background;
        private UIBasicSprite.AdvancedType _rightBorder;
        private bool _borderHidden;
        private bool _shifted;
        private UIWidget _column;
        private UILabel _minus, _plus;
        private UILabel _quantity;
        private UILabel _spend;
        private int _spendBottom, _spendTop;
        private Transform _spendLeftTarget, _spendRightTarget;
        private float _spendLeftRelative, _spendRightRelative;
        private int _spendLeft, _spendRight;
        private bool _spendSeparated;
        private readonly List<UILabel> _texts = new List<UILabel>();
        private bool _failed, _rendering;
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
        internal static bool HasSupportedRows(CraftGUI gui)
        {
            var items = gui.GetItemsList();
            return items != null && items.Exists(row => row != null && row.gameObject.activeInHierarchy && Eligible(row));
        }
        internal void Restore()
        {
            RestoreBorder(); RestoreSpend();
            if (!_shifted || _ingredients == null) return;
            _ingredients.leftAnchor.absolute = _left; _ingredients.rightAnchor.absolute = _right;
            _ingredients.topAnchor.absolute = _top; _ingredients.bottomAnchor.absolute = _bottom;
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
            label.overflowMethod = UILabel.Overflow.ClampContent; label.maxLineCount = 1;
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
            if (_failed || _row == null || _rendering) return;
            _rendering = true;
            try
            {
                if (!_row.gameObject.activeInHierarchy) { Restore(); Hide(); return; }
                if (!Eligible(_row)) { Restore(); Hide(); return; }
                if (_ingredients == null)
                {
                    _ingredients = _row.ingredients_table.GetComponent<UIWidget>();
                    if (_ingredients == null) throw new InvalidOperationException("Native ingredient anchor widget unavailable");
                    _left = _ingredients.leftAnchor.absolute; _right = _ingredients.rightAnchor.absolute;
                    _top = _ingredients.topAnchor.absolute; _bottom = _ingredients.bottomAnchor.absolute;
                    _ingredientLayout = _row.ingredients_table;
                    _ingredientLayout.onReposition += AfterIngredientLayout;
                    UITable list = GUIElements.me.craft.craft_list_grid;
                    if (_nativeList != list)
                    {
                        UnsubscribeList(); _nativeList = list;
                        if (_nativeList != null) _nativeList.onReposition += RefreshAll;
                    }
                }
                _row.GetComponent<UIWidget>().ResetAndUpdateAnchors();
                _row.selection_frame.ResetAndUpdateAnchors();
                if (!_shifted)
                {
                    _ingredients.leftAnchor.absolute = _left - Reserve;
                    _ingredients.rightAnchor.absolute = _right - Reserve;
                    _shifted = true; _ingredients.ResetAndUpdateAnchors();
                    _row.label_name.ResetAndUpdateAnchors();
                }
                if (_column == null)
                {
                    UIPanel panel = _row.GetComponentInParent<UIPanel>();
                    if (panel == null) throw new InvalidOperationException("Native clipped row panel unavailable");
                    _column = Child("CraftingPlannerColumn", panel.transform).AddComponent<UIWidget>();
                    _column.pivot = UIWidget.Pivot.TopRight;
                    _column.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
                    _quantity = Text("", _column.transform, -38, -28, PlannerLayout.QuantityWidth, 22);
                    _quantity.overflowMethod = UILabel.Overflow.ShrinkContent;
                    _minus = Arrow(-1, -63); _plus = Arrow(1, -13);
                }
                PlannerStyle style = PlannerStyle.Current;
                _column.SetAnchor(_row.selection_frame.gameObject, 1, -Reserve + (int)style.MenuX, 0, 4 + (int)style.MenuY, 1, -4 + (int)style.MenuX, 1, -5 + (int)style.MenuY);
                _column.ResetAndUpdateAnchors();
                SeparateSpend();
                _ingredients.ResetAndUpdateAnchors();
                _row.ingredients_table.Reposition();
                AlignIngredients();
                _ingredients.leftAnchor.absolute += (int)style.IngredientsX; _ingredients.rightAnchor.absolute += (int)style.IngredientsX;
                _ingredients.topAnchor.absolute = _top + (int)style.IngredientsY; _ingredients.bottomAnchor.absolute = _bottom + (int)style.IngredientsY;
                _ingredients.ResetAndUpdateAnchors(); _row.label_name.ResetAndUpdateAnchors();
                int quantity = Planner.QuantityFor(_row.current_craft);
                foreach (UILabel label in _texts) PlannerCard.ApplyNativeFont(label, _row.label_name);
                foreach (UILabel label in new[] { _quantity, _minus, _plus })
                {
                    PlannerCard.ApplyNativeFont(label, style.MenuHudFont ? GUIElements.me.hud.zone_name : _row.label_name);
                    label.fontSize = (int)style.MenuFont; label.effectStyle = style.MenuOutline ? UILabel.Effect.Outline : UILabel.Effect.Shadow;
                    label.effectColor = new Color(.06f, .06f, .05f, style.MenuShadowOpacity); label.effectDistance = new Vector2(style.MenuShadowSize, style.MenuShadowSize);
                }
                float quantityX = -_column.width / 2f + style.MenuQuantityX, quantityY = -_column.height / 2f + style.MenuQuantityY;
                _quantity.transform.localPosition = new Vector3(quantityX, quantityY, 0);
                _quantity.text = quantity.ToString();
                float numberWidth = Math.Min(_quantity.width, _quantity.printedSize.x);
                _minus.transform.localPosition = new Vector3(quantityX - PlannerLayout.QuantityArrowOffset(numberWidth, _minus.printedSize.x, style.QuantityArrowGap) + style.MenuMinusX, quantityY + style.MenuMinusY, 0);
                _plus.transform.localPosition = new Vector3(quantityX + PlannerLayout.QuantityArrowOffset(numberWidth, _plus.printedSize.x, style.QuantityArrowGap) + style.MenuPlusX, quantityY + style.MenuPlusY, 0);
                if (_background == null && _row.selection_frame.leftAnchor.target != null) _background = _row.selection_frame.leftAnchor.target.GetComponent<UIBasicSprite>();
                if (style.MenuHideBorder && _background != null && _background.type == UIBasicSprite.Type.Advanced)
                {
                    if (!_borderHidden) { _rightBorder = _background.rightType; _borderHidden = true; }
                    _background.rightType = UIBasicSprite.AdvancedType.Invisible; _background.MarkAsChanged();
                }
                else RestoreBorder();
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
            finally { _rendering = false; }
        }
        private void RestoreSpend()
        {
            if (_spend != null)
            {
                _spend.bottomAnchor.absolute = _spendBottom;
                _spend.topAnchor.absolute = _spendTop;
                _spend.leftAnchor.target = _spendLeftTarget; _spend.leftAnchor.relative = _spendLeftRelative; _spend.leftAnchor.absolute = _spendLeft;
                _spend.rightAnchor.target = _spendRightTarget; _spend.rightAnchor.relative = _spendRightRelative; _spend.rightAnchor.absolute = _spendRight;
                _spend.ResetAndUpdateAnchors();
            }
            _spend = null; _spendSeparated = false;
        }
        private void CaptureSpend(UILabel spend)
        {
            _spend = spend; _spendBottom = spend.bottomAnchor.absolute; _spendTop = spend.topAnchor.absolute;
            _spendLeftTarget = spend.leftAnchor.target; _spendLeftRelative = spend.leftAnchor.relative; _spendLeft = spend.leftAnchor.absolute;
            _spendRightTarget = spend.rightAnchor.target; _spendRightRelative = spend.rightAnchor.relative; _spendRight = spend.rightAnchor.absolute;
        }
        private void SeparateSpend()
        {
            RestoreSpend();
            UILabel spend = _row.txt_spend;
            if (spend == null || !spend.gameObject.activeInHierarchy || string.IsNullOrWhiteSpace(spend.text)) return;
            spend.ResetAndUpdateAnchors();
            int width = spend.width;
            CaptureSpend(spend);
            // Every native cost shape uses the same adjustable block; its text/size stay host-owned.
            spend.leftAnchor.target = _column.transform; spend.leftAnchor.relative = 0; spend.leftAnchor.absolute = -7 - width + (int)PlannerStyle.Current.MenuSpendX;
            spend.rightAnchor.target = _column.transform; spend.rightAnchor.relative = 0; spend.rightAnchor.absolute = -7 + (int)PlannerStyle.Current.MenuSpendX;
            spend.topAnchor.absolute = _spendTop + (int)PlannerStyle.Current.MenuSpendY;
            spend.bottomAnchor.absolute = _spendBottom + (int)PlannerStyle.Current.MenuSpendY;
            spend.ResetAndUpdateAnchors(); _spendSeparated = true;
        }
        private void RestoreBorder()
        {
            if (_borderHidden && _background != null) { _background.rightType = _rightBorder; _background.MarkAsChanged(); }
            _borderHidden = false;
        }
        private void AfterIngredientLayout() { Render(); }
        private static void UnsubscribeList() { if (_nativeList != null) _nativeList.onReposition -= RefreshAll; _nativeList = null; }
        private void AlignIngredients()
        {
            float right = float.MinValue;
            foreach (UIWidget widget in _row.ingredients_table.GetComponentsInChildren<UIWidget>())
            {
                if (widget == _ingredients || !widget.gameObject.activeInHierarchy) continue;
                foreach (Vector3 corner in widget.worldCorners) right = Math.Max(right, transform.InverseTransformPoint(corner).x);
            }
            if (right == float.MinValue) return;
            UIWidget boundary = _spendSeparated ? _spend : _column;
            float target = transform.InverseTransformPoint(boundary.worldCorners[0]).x - 7;
            // Retain the accepted origin while allowing real signed movement relative to native costs.
            if (_spendSeparated) target -= PlannerLayout.MenuIngredientsOrigin;
            int delta = (int)Math.Round(target - right);
            _ingredients.leftAnchor.absolute += delta; _ingredients.rightAnchor.absolute += delta;
            _ingredients.ResetAndUpdateAnchors(); _row.label_name.ResetAndUpdateAnchors();
        }
        private void Hide() { if (_column != null) _column.gameObject.SetActive(false); }
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
            UnsubscribeList();
        }
        private void OnDisable() { Restore(); Hide(); }
        private void OnEnable() { if (_row != null) Render(); }
        private void OnDestroy()
        {
            Restore(); if (_ingredientLayout != null) _ingredientLayout.onReposition -= AfterIngredientLayout;
            if (_column != null) UnityEngine.Object.Destroy(_column.gameObject);
            Rows.Remove(this); if (Rows.Count == 0) UnsubscribeList();
        }
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
