// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CraftingPlanner
{
    // Both surfaces own only renderers. No cell controllers, colliders or navigation items.
    internal sealed class PlannerCard : IDisposable
    {
        internal const int Width = 284;
        private const int Padding = 10;
        private const int Line = 20;
        private const int Gap = 12;
        private readonly UIWidget _root;
        private readonly UITexture _back;
        private readonly UILabel _fontTemplate;
        private readonly UI2DSprite _iconTemplate;
        private readonly UILabel _anchor;
        private readonly List<UILabel> _labels = new List<UILabel>();
        private readonly List<UI2DSprite> _icons = new List<UI2DSprite>();
        private int _labelCursor;
        private int _iconCursor;
        private bool _iconMissingLogged;
        private bool _geometryLogged;

        internal bool IsAlive { get { return _root != null; } }

        internal PlannerCard(Transform parent, int layer, UILabel anchor, UILabel fontTemplate, UI2DSprite iconTemplate, string name)
        {
            _anchor = anchor;
            _fontTemplate = fontTemplate;
            _iconTemplate = iconTemplate;
            GameObject go = new GameObject(name);
            go.layer = layer;
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            _root = go.AddComponent<UIWidget>();
            _root.pivot = UIWidget.Pivot.TopRight;
            _root.width = Width;
            _root.height = 40;
            _root.alpha = 1f;
            _root.updateAnchors = UIRect.AnchorUpdate.OnUpdate;

            GameObject backGo = new GameObject("soft-backing");
            backGo.layer = go.layer;
            backGo.transform.SetParent(go.transform, false);
            _back = backGo.AddComponent<UITexture>();
            _back.mainTexture = Texture2D.whiteTexture;
            _back.pivot = UIWidget.Pivot.TopRight;
            _back.color = new Color(0.10f, 0.095f, 0.085f, 0.52f);
            _back.depth = 5000;
            _back.onPostFill = FadeBacking;
            Anchor(40);
        }

        internal void Render()
        {
            if (_root == null || _fontTemplate == null || _anchor == null) return;
            if (!Planner.HasGoals) { Hide(); return; }
            _labelCursor = 0;
            _iconCursor = 0;
            int y = Padding;
            Label(PlannerLocalization.Plan, Padding, y, Width - 2 * Padding, false, 0.85f);
            y += Line;
            foreach (ProjectGoal goal in Planner.GetGoals())
            {
                Label(Planner.GetProjectName(goal.Definition), Padding, y, Width - 80, false, 1f);
                Label("×" + goal.Quantity, Width - 68, y, 58, true, 1f);
                y += Line;
            }
            y += 5;
            // Numeric columns have independent right edges; no space-based alignment.
            Label(PlannerLocalization.Have, 112, y, 50, true, 0.74f);
            Label(PlannerLocalization.Required, 166, y, 50, true, 0.74f);
            Label(PlannerLocalization.Missing, 220, y, 54, true, 0.74f);
            y += Line;
            var requirements = Planner.AggregateRequirements().ToList();
            requirements.Sort((a, b) => string.Compare(Planner.GetItemName(a.Key), Planner.GetItemName(b.Key), StringComparison.CurrentCultureIgnoreCase));
            foreach (var pair in requirements)
            {
                int have = Planner.GetPlayerHave(pair.Key);
                int missing = Math.Max(pair.Value - have, 0);
                bool hasIcon = Icon(pair.Key, y);
                Label(Planner.GetItemName(pair.Key), hasIcon ? 35 : Padding, y, hasIcon ? 74 : 99, false, 0.90f);
                Label(have.ToString(), 112, y, 50, true, 0.90f);
                Label(pair.Value.ToString(), 166, y, 50, true, 0.90f);
                UILabel remaining = Label(missing == 0 ? "—" : missing.ToString(), 220, y, 54, true, 1f);
                remaining.color = missing == 0 ? new Color(0.72f, 0.80f, 0.62f, 0.85f) : new Color(0.96f, 0.82f, 0.57f, 1f);
                y += Line;
            }
            for (int i = _labelCursor; i < _labels.Count; i++) _labels[i].gameObject.SetActive(false);
            for (int i = _iconCursor; i < _icons.Count; i++) _icons[i].gameObject.SetActive(false);
            Anchor(y + Padding);
            _root.gameObject.SetActive(true);
            if (!_geometryLogged && _root.gameObject.activeInHierarchy)
            {
                Planner.LogLine("event=card_created surface=" + _root.gameObject.name +
                    " size=" + Width + "x" + _root.height + " layer=" + _root.gameObject.layer +
                    " language=" + PlannerLocalization.CurrentLanguage +
                    " native_font=" + (_fontTemplate.bitmapFont != null ? _fontTemplate.bitmapFont.name : "dynamic") +
                    " position=" + _root.transform.position);
                _geometryLogged = true;
            }
        }

        private void Anchor(int height)
        {
            _root.width = Width;
            _root.height = height;
            // All four sides target the native zone label's bottom-right corner.
            _root.SetAnchor(_anchor.gameObject, 1f, -Width, 0f, -Gap - height, 1f, 0, 0f, -Gap);
            _back.width = Width;
            _back.height = height;
        }

        private UILabel Label(string text, int x, int y, int width, bool right, float alpha)
        {
            if (_labelCursor == _labels.Count)
            {
                GameObject go = new GameObject("text-" + _labelCursor);
                go.layer = _root.gameObject.layer;
                go.transform.SetParent(_root.transform, false);
                _labels.Add(go.AddComponent<UILabel>());
            }
            UILabel label = _labels[_labelCursor++];
            ApplyNativeFont(label, _fontTemplate);
            label.pivot = UIWidget.Pivot.TopLeft;
            label.alignment = right ? NGUIText.Alignment.Right : NGUIText.Alignment.Left;
            label.multiLine = false;
            label.maxLineCount = 1;
            label.overflowMethod = UILabel.Overflow.ShrinkContent;
            label.width = width;
            label.height = Line;
            label.color = new Color(0.94f, 0.90f, 0.80f, alpha);
            label.depth = 5002;
            label.transform.localPosition = new Vector3(-Width + x, -y, 0f);
            label.text = text;
            label.gameObject.SetActive(true);
            return label;
        }

        internal static void ApplyNativeFont(UILabel label, UILabel template)
        {
            // The live host label owns the current locale font. Copy it afresh so
            // switching CJK -> Latin does not retain a previously applied font.
            if (template.bitmapFont != null) label.bitmapFont = template.bitmapFont;
            else label.trueTypeFont = template.trueTypeFont;
            label.fontSize = template.fontSize;
            label.fontStyle = template.fontStyle;
            label.symbolStyle = template.symbolStyle;
            label.spacingX = template.spacingX;
            label.spacingY = template.spacingY;
            label.effectStyle = template.effectStyle;
            label.effectColor = template.effectColor;
            label.effectDistance = template.effectDistance;
        }

        private bool Icon(string itemId, int y)
        {
            ItemDefinition item = GameBalance.me.GetDataOrNull<ItemDefinition>(itemId);
            Sprite sprite = item != null ? EasySpritesCollection.GetSprite(item.GetIcon(), false, "") : null;
            if (_iconTemplate == null || sprite == null)
            {
                if (!_iconMissingLogged)
                {
                    Planner.LogLine("event=card_icon result=text_fallback reason=no_native_template_or_sprite");
                    _iconMissingLogged = true;
                }
                return false;
            }
            if (_iconCursor == _icons.Count)
            {
                UI2DSprite icon = UnityEngine.Object.Instantiate(_iconTemplate, _root.transform, false);
                icon.gameObject.name = "item-icon-" + _iconCursor;
                icon.gameObject.layer = _root.gameObject.layer;
                icon.SetAnchor((GameObject)null);
                icon.transform.localRotation = Quaternion.identity;
                icon.transform.localScale = Vector3.one;
                icon.fixedAspect = false;
                icon.pivot = UIWidget.Pivot.TopLeft;
                icon.width = 20;
                icon.height = 20;
                icon.depth = 5001;
                // Preserve native shader and tint; only the image/geometry changes.
                _icons.Add(icon);
            }
            UI2DSprite current = _icons[_iconCursor++];
            current.sprite2D = sprite;
            current.transform.localPosition = new Vector3(-Width + Padding, -y, 0f);
            current.gameObject.SetActive(true);
            return true;
        }

        internal static float BackingStrength(float x, float y)
        {
            return Math.Max(0f, Math.Min(1f, (x + y) * 0.5f));
        }

        private void FadeBacking(UIWidget widget, int start, List<Vector3> vertices, List<Vector2> uvs, List<Color> colors)
        {
            for (int i = start; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];
                Color color = colors[i];
                color.a *= BackingStrength((vertex.x + Width) / Width, (vertex.y + _back.height) / _back.height);
                colors[i] = color;
            }
        }

        internal void Hide() { if (_root != null) _root.gameObject.SetActive(false); }

        public void Dispose()
        {
            if (_back != null) _back.onPostFill = null;
            if (_root != null) UnityEngine.Object.Destroy(_root.gameObject);
        }
    }

    internal static class PlannerCardFactory
    {
        internal static PlannerCard Create(Transform parent, int layer, string name)
        {
            GUIElements gui = GUIElements.me;
            if (gui == null || gui.hud == null || gui.hud.zone_name == null || gui.craft == null || gui.craft.craft_item_prefab == null) return null;
            BaseItemCellGUI cell = gui.craft.craft_item_prefab.item_gui;
            UI2DSprite icon = cell != null && cell.x1 != null ? cell.x1.icon : null;
            return new PlannerCard(parent, layer, gui.hud.zone_name, gui.hud.zone_name, icon, name);
        }
    }

    internal static class PlannerHud
    {
        private static PlannerCard _card;
        private static HUD _hud;
        private static bool _failed;
        internal static void Ensure(HUD hud)
        {
            if (_failed || hud == null || (_card != null && _card.IsAlive && _hud == hud)) return;
            Destroy();
            if (hud.zone_name == null || hud.zone_name.transform.parent == null) return;
            _hud = hud;
            try { _card = PlannerCardFactory.Create(hud.zone_name.transform.parent, hud.zone_name.gameObject.layer, "CraftingPlannerHUD"); }
            catch (Exception ex) { Fail(ex); }
        }
        internal static void Refresh()
        {
            if (_failed) return;
            if (GUIElements.me != null) Ensure(GUIElements.me.hud);
            try { if (_card != null) _card.Render(); }
            catch (Exception ex) { Fail(ex); }
        }
        internal static void RefreshIfVisible()
        {
            if (_hud != null && _hud.gameObject.activeInHierarchy) Refresh();
        }
        internal static void OnLanguageChanged() { Refresh(); }
        internal static void Destroy() { if (_card != null) _card.Dispose(); _card = null; _hud = null; }
        private static void Fail(Exception ex)
        {
            _failed = true;
            Destroy();
            Planner.LogLine("event=hud_card result=failed action=disabled_for_session error=" + ex);
        }
    }

    internal static class PlannerCraftOverlay
    {
        private static PlannerCard _card;
        private static CraftGUI _craft;
        private static bool _failed;
        internal static void Open(CraftGUI gui)
        {
            if (_failed || gui == null) return;
            if (_craft != gui || _card == null || !_card.IsAlive)
            {
                Destroy();
                _craft = gui;
                UIPanel panel = gui.GetComponentInParent<UIPanel>();
                try { if (panel != null) _card = PlannerCardFactory.Create(panel.transform, panel.gameObject.layer, "CraftingPlannerCraftOverlay"); }
                catch (Exception ex) { Fail(ex); }
            }
            Refresh();
        }
        internal static void Refresh()
        {
            if (_failed) return;
            try
            {
                if (_card != null)
                {
                    if (_craft != null && _craft.gameObject.activeInHierarchy && Planner.ShouldShowCraftOverlay) _card.Render();
                    else _card.Hide();
                }
                PlannerInputHints.Refresh();
            }
            catch (Exception ex) { Fail(ex); }
        }
        internal static void OnLanguageChanged() { Refresh(); }
        internal static void Hide() { if (_card != null) _card.Hide(); PlannerInputHints.HideCraft(); }
        internal static void Destroy() { if (_card != null) _card.Dispose(); _card = null; _craft = null; }
        private static void Fail(Exception ex)
        {
            _failed = true;
            Destroy();
            Planner.LogLine("event=craft_card result=failed action=disabled_for_session error=" + ex);
        }
    }

    internal static class PlannerInputHints
    {
        private static UILabel _craftHint;
        private static UILabel _chestHint;
        private static bool _failed;
        internal static void Refresh()
        {
            if (_failed || GUIElements.me == null) return;
            try
            {
                CraftGUI craft = GUIElements.me.craft;
                ChestGUI chest = GUIElements.me.chest;
                Show(ref _craftHint, craft, Planner.CanAdjustFocusedProject,
                    GameKeyTip.Get(GameKey.PrevSubTab, "−1", true, true, false, false) + "   " +
                    GameKeyTip.Get(GameKey.NextSubTub, "+1", true, true, false, false));
                Show(ref _chestHint, chest, Planner.HasGoals,
                    GameKeyTip.Get(GameKey.Option2, PlannerLocalization.TakeNeeded, true, true, false, false));
            }
            catch (Exception ex)
            {
                _failed = true;
                Destroy();
                Planner.LogLine("event=input_hints result=failed action=disabled_for_session error=" + ex);
            }
        }
        private static void Show(ref UILabel label, BaseGUI gui, bool relevant, string text)
        {
            bool visible = relevant && gui != null && gui.is_shown_and_top && LazyInput.gamepad_active;
            if (!visible) { if (label != null) label.gameObject.SetActive(false); return; }
            ButtonTipsStr tips = gui.button_tips;
            if (tips == null || tips.label == null) return;
            UILabel template = tips.label;
            if (label == null)
            {
                GameObject go = new GameObject("CraftingPlannerInputHint");
                go.layer = template.gameObject.layer;
                go.transform.SetParent(template.transform.parent, false);
                label = go.AddComponent<UILabel>();
            }
            PlannerCard.ApplyNativeFont(label, template);
            label.pivot = UIWidget.Pivot.TopLeft;
            label.alignment = NGUIText.Alignment.Center;
            label.width = template.width;
            label.height = 22;
            label.multiLine = false;
            label.maxLineCount = 1;
            label.overflowMethod = UILabel.Overflow.ShrinkContent;
            label.depth = template.depth + 1;
            label.color = template.color;
            label.SetAnchor(template.gameObject, 0f, 0, 1f, 6, 1f, 0, 1f, 28);
            label.text = text;
            label.gameObject.SetActive(true);
        }
        internal static void OnWindowChanged(BaseGUI gui) { Refresh(); }
        internal static void OnInputChanged() { Refresh(); }
        internal static void HideCraft() { if (_craftHint != null) _craftHint.gameObject.SetActive(false); }
        internal static void HideChest() { if (_chestHint != null) _chestHint.gameObject.SetActive(false); }
        internal static void Destroy()
        {
            if (_craftHint != null) UnityEngine.Object.Destroy(_craftHint.gameObject);
            if (_chestHint != null) UnityEngine.Object.Destroy(_chestHint.gameObject);
            _craftHint = null;
            _chestHint = null;
        }
    }
}
