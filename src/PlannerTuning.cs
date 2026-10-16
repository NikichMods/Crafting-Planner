// SPDX-License-Identifier: MPL-2.0
namespace CraftingPlanner
{
    internal sealed partial class PlannerStyle
    {
        internal float MenuX = 0, MenuY = 0, MenuMinusX = 0, MenuMinusY = 0, MenuQuantityX = 0, MenuQuantityY = 0, MenuPlusX = 0, MenuPlusY = 0;
        internal float IngredientsX = 0, IngredientsY = 0, MenuFont = 16, MenuShadowSize = 1, MenuShadowOpacity = .8f;
        internal bool MenuHudFont = false, MenuOutline = false, MenuHideBorder = true;
        internal float TitleY = 0, FooterX = 0, FooterY = 0, FooterIconSize = 20, FooterShadowSize = 1, FooterShadowOpacity = .8f;
        internal bool FooterOutline = true;
        internal float TakeX = 0, TakeY = 0, ChestTipsX = 0;
        private static float Offset(float value) { return Pixel(Bound(value, -300, 300)); }
        private void ClampTuning()
        {
            MenuX = Offset(MenuX); MenuY = Offset(MenuY); MenuMinusX = Offset(MenuMinusX); MenuMinusY = Offset(MenuMinusY);
            MenuQuantityX = Offset(MenuQuantityX); MenuQuantityY = Offset(MenuQuantityY); MenuPlusX = Offset(MenuPlusX); MenuPlusY = Offset(MenuPlusY);
            IngredientsX = Offset(IngredientsX); IngredientsY = Offset(IngredientsY); TitleY = Offset(TitleY); FooterX = Offset(FooterX); FooterY = Offset(FooterY);
            MenuFont = Pixel(Bound(MenuFont, 10, 32)); FooterIconSize = Pixel(Bound(FooterIconSize, 10, 48));
            MenuShadowSize = Pixel(Bound(MenuShadowSize, 0, 4)); FooterShadowSize = Pixel(Bound(FooterShadowSize, 0, 4));
            MenuShadowOpacity = (float)System.Math.Round(Bound(MenuShadowOpacity, 0, 1), 2);
            FooterShadowOpacity = (float)System.Math.Round(Bound(FooterShadowOpacity, 0, 1), 2);
            TakeX = Pixel(Bound(TakeX, -600, 1000)); TakeY = Offset(TakeY); ChestTipsX = Pixel(Bound(ChestTipsX, -600, 600));
        }
        private string TuningRecord()
        {
            return " menu_x=" + N(MenuX) + " menu_y=" + N(MenuY) + " menu_minus_x=" + N(MenuMinusX) + " menu_minus_y=" + N(MenuMinusY) +
                " menu_quantity_x=" + N(MenuQuantityX) + " menu_quantity_y=" + N(MenuQuantityY) + " menu_plus_x=" + N(MenuPlusX) + " menu_plus_y=" + N(MenuPlusY) +
                " ingredients_x=" + N(IngredientsX) + " ingredients_y=" + N(IngredientsY) + " menu_font=" + N(MenuFont) + " menu_hud_font=" + MenuHudFont +
                " menu_outline=" + MenuOutline + " menu_shadow_size=" + N(MenuShadowSize) + " menu_shadow_opacity=" + N(MenuShadowOpacity) + " menu_hide_border=" + MenuHideBorder +
                " title_y=" + N(TitleY) + " footer_x=" + N(FooterX) + " footer_y=" + N(FooterY) + " footer_icon_size=" + N(FooterIconSize) +
                " footer_outline=" + FooterOutline + " footer_shadow_size=" + N(FooterShadowSize) + " footer_shadow_opacity=" + N(FooterShadowOpacity) +
                " take_x=" + N(TakeX) + " take_y=" + N(TakeY) + " chest_tips_x=" + N(ChestTipsX);
        }
    }
}
