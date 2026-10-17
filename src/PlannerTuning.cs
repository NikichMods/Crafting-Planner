// SPDX-License-Identifier: MPL-2.0
namespace CraftingPlanner
{
    internal sealed partial class PlannerStyle
    {
        internal float MenuX = 17, MenuY = 1, MenuMinusX = 8, MenuMinusY = 0, MenuQuantityX = 0, MenuQuantityY = 0, MenuPlusX = -9, MenuPlusY = 0;
        internal float IngredientsX = PlannerLayout.MenuIngredientsOrigin, IngredientsY = 0, CostIngredientsX = 46, CostIngredientsY = 0;
        internal float MenuSpendX = 24, MenuSpendY = -1, QuantityArrowGap = 13;
        internal float MenuFont = 16, MenuShadowSize = 1, MenuShadowOpacity = .8f;
        internal bool MenuHudFont = true, MenuOutline = false, MenuHideBorder = false;
        internal float TitleY = 0, FooterX = -22, FooterY = 3, FooterIconSize = 16, FooterShadowSize = 1, FooterShadowOpacity = .8f;
        internal bool FooterOutline = true;
        internal float ChestRowX = 0, ChestRowY = 0, ChestActionGap = 10;
        internal float MenuCaptionX = 0, MenuCaptionY = 0, MenuCaptionFont = 16, MenuActionGap = 10;
        private static float Offset(float value) { return Pixel(Bound(value, -300, 300)); }
        internal float IngredientX(bool hasCosts) { return hasCosts ? CostIngredientsX : IngredientsX; }
        internal float IngredientY(bool hasCosts) { return hasCosts ? CostIngredientsY : IngredientsY; }
        private void ClampTuning()
        {
            MenuX = Offset(MenuX); MenuY = Offset(MenuY); MenuMinusX = Offset(MenuMinusX); MenuMinusY = Offset(MenuMinusY);
            MenuQuantityX = Offset(MenuQuantityX); MenuQuantityY = Offset(MenuQuantityY); MenuPlusX = Offset(MenuPlusX); MenuPlusY = Offset(MenuPlusY);
            IngredientsX = Offset(IngredientsX); IngredientsY = Offset(IngredientsY); TitleY = Offset(TitleY); FooterX = Offset(FooterX); FooterY = Offset(FooterY);
            CostIngredientsX = Offset(CostIngredientsX); CostIngredientsY = Offset(CostIngredientsY);
            MenuSpendX = Offset(MenuSpendX); MenuSpendY = Offset(MenuSpendY); QuantityArrowGap = Pixel(Bound(QuantityArrowGap, 0, 40));
            MenuFont = Pixel(Bound(MenuFont, 10, 32)); FooterIconSize = Pixel(Bound(FooterIconSize, 10, 48));
            MenuShadowSize = Pixel(Bound(MenuShadowSize, 0, 4)); FooterShadowSize = Pixel(Bound(FooterShadowSize, 0, 4));
            MenuShadowOpacity = (float)System.Math.Round(Bound(MenuShadowOpacity, 0, 1), 2);
            FooterShadowOpacity = (float)System.Math.Round(Bound(FooterShadowOpacity, 0, 1), 2);
            ChestRowX = Pixel(Bound(ChestRowX, -600, 600)); ChestRowY = Offset(ChestRowY); ChestActionGap = Pixel(Bound(ChestActionGap, 0, 40));
            MenuCaptionX = Offset(MenuCaptionX); MenuCaptionY = Offset(MenuCaptionY); MenuCaptionFont = Pixel(Bound(MenuCaptionFont, 10, 32)); MenuActionGap = Pixel(Bound(MenuActionGap, 0, 40));
        }
        private string TuningRecord()
        {
            return " menu_x=" + N(MenuX) + " menu_y=" + N(MenuY) + " menu_minus_x=" + N(MenuMinusX) + " menu_minus_y=" + N(MenuMinusY) +
                " menu_quantity_x=" + N(MenuQuantityX) + " menu_quantity_y=" + N(MenuQuantityY) + " menu_plus_x=" + N(MenuPlusX) + " menu_plus_y=" + N(MenuPlusY) +
                " ingredients_x=" + N(IngredientsX) + " ingredients_y=" + N(IngredientsY) + " cost_ingredients_x=" + N(CostIngredientsX) + " cost_ingredients_y=" + N(CostIngredientsY) +
                " menu_spend_x=" + N(MenuSpendX) + " menu_spend_y=" + N(MenuSpendY) +
                " quantity_arrow_gap=" + N(QuantityArrowGap) + " menu_font=" + N(MenuFont) + " menu_hud_font=" + MenuHudFont +
                " menu_outline=" + MenuOutline + " menu_shadow_size=" + N(MenuShadowSize) + " menu_shadow_opacity=" + N(MenuShadowOpacity) + " menu_hide_border=" + MenuHideBorder +
                " title_y=" + N(TitleY) + " footer_x=" + N(FooterX) + " footer_y=" + N(FooterY) + " footer_icon_size=" + N(FooterIconSize) +
                " footer_outline=" + FooterOutline + " footer_shadow_size=" + N(FooterShadowSize) + " footer_shadow_opacity=" + N(FooterShadowOpacity) +
                " chest_row_x=" + N(ChestRowX) + " chest_row_y=" + N(ChestRowY) + " chest_action_gap=" + N(ChestActionGap) +
                " menu_caption_x=" + N(MenuCaptionX) + " menu_caption_y=" + N(MenuCaptionY) + " menu_caption_font=" + N(MenuCaptionFont) + " menu_action_gap=" + N(MenuActionGap);
        }
    }
}
