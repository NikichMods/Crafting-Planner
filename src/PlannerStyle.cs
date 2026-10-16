// SPDX-License-Identifier: MPL-2.0
using System;
using System.Globalization;
namespace CraftingPlanner
{
    internal sealed partial class PlannerStyle
    {
        internal float Right = 9, Top = 40, Width = 286, Scale = .65f;
        internal float Font = 24, Icon = 50, Row = 18, Padding = 12;
        internal float Column = 67, ColumnGap = 5, TitleGap = 0, FooterGap = 9;
        internal float Opacity = 0.39f, TextOpacity = 1, ShadowRadius = 34, ShadowFeather = 28;
        internal float GoalsOffset = -7, MaterialsGap = 12, ColumnInset = 20;
        internal bool CraftFont = true, Outline = true;
        internal bool AlternateNumbers = false, AlternateQuantity = true, TitleCraftFont = false, FooterCraftFont = true;
        internal float TitleFont = 16, FooterFont = 20;
        internal float IconX = -3, IconY, GoalIconSpacing, MaterialIconSpacing;
        internal float FeedbackX = 264, FeedbackY = -5, FeedbackGap = 0, FeedbackLineGap = 4;
        internal static PlannerStyle Current = new PlannerStyle();
        internal void Clamp()
        {
            Right = Bound(Right, 0, 400); Top = Bound(Top, 40, 400); Width = Bound(Width, 240, 640);
            Scale = (float)Math.Round(Bound(Scale, 0.6f, 1.5f), 2); Font = Bound(Font, 10, 32); Icon = Bound(Icon, 8, 80);
            Row = Bound(Row, 18, 48); Padding = Bound(Padding, 4, 24); Column = Bound(Column, 32, 110);
            ColumnGap = Bound(ColumnGap, 0, 20); TitleGap = Bound(TitleGap, 0, 28); FooterGap = Bound(FooterGap, 4, 32);
            Opacity = Bound(Opacity, 0, 0.85f); TextOpacity = Bound(TextOpacity, 0.5f, 1);
            ShadowRadius = Bound(ShadowRadius, 6, 90); ShadowFeather = Bound(ShadowFeather, 5, 28);
            GoalsOffset = Bound(GoalsOffset, -12, 80); MaterialsGap = Bound(MaterialsGap, 0, 60); ColumnInset = Bound(ColumnInset, 0, 36);
            TitleFont = Bound(TitleFont, 10, 32); FooterFont = Bound(FooterFont, 10, 32);
            IconX = Bound(IconX, -60, 60); IconY = Bound(IconY, -60, 60);
            GoalIconSpacing = Bound(GoalIconSpacing, -20, 20); MaterialIconSpacing = Bound(MaterialIconSpacing, -20, 20);
            FeedbackX = Bound(FeedbackX, -600, 1000); FeedbackY = Bound(FeedbackY, -300, 150);
            FeedbackGap = Bound(FeedbackGap, 0, 40); FeedbackLineGap = Bound(FeedbackLineGap, 0, 24);
            Right = Pixel(Right); Top = Pixel(Top); Width = Pixel(Width); Font = Pixel(Font); Icon = Pixel(Icon); Row = Pixel(Row);
            Padding = Pixel(Padding); Column = Pixel(Column); ColumnGap = Pixel(ColumnGap); TitleGap = Pixel(TitleGap); FooterGap = Pixel(FooterGap);
            ShadowRadius = Pixel(ShadowRadius); ShadowFeather = Pixel(ShadowFeather); GoalsOffset = Pixel(GoalsOffset); MaterialsGap = Pixel(MaterialsGap); ColumnInset = Pixel(ColumnInset);
            TitleFont = Pixel(TitleFont); FooterFont = Pixel(FooterFont); IconX = Pixel(IconX); IconY = Pixel(IconY);
            GoalIconSpacing = Pixel(GoalIconSpacing); MaterialIconSpacing = Pixel(MaterialIconSpacing);
            FeedbackX = Pixel(FeedbackX); FeedbackY = Pixel(FeedbackY); FeedbackGap = Pixel(FeedbackGap); FeedbackLineGap = Pixel(FeedbackLineGap);
            Opacity = (float)Math.Round(Opacity, 2); TextOpacity = (float)Math.Round(TextOpacity, 2);
            ClampTuning();
        }
        private static float Pixel(float value) { return (float)Math.Round(value, MidpointRounding.AwayFromZero); }
        internal static float BaselineValue(string field)
        {
            float value = (float)typeof(PlannerStyle).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(new PlannerStyle());
            return field == "Opacity" || field == "TextOpacity" || field.EndsWith("ShadowOpacity", StringComparison.Ordinal) ? value * 100 : value;
        }
        private static float Bound(float value, float min, float max)
        { return float.IsNaN(value) || float.IsInfinity(value) ? min : Math.Max(min, Math.Min(max, value)); }
        internal string Record()
        {
            return "right=" + N(Right) + " top=" + N(Top) + " width=" + N(Width) + " scale=" + N(Scale) +
                " font=" + N(Font) + " icon=" + N(Icon) + " row=" + N(Row) + " padding=" + N(Padding) +
                " column=" + N(Column) + " column_gap=" + N(ColumnGap) + " title_gap=" + N(TitleGap) +
                " footer_gap=" + N(FooterGap) + " opacity=" + N(Opacity) + " text_opacity=" + N(TextOpacity) + " shadow_radius=" + N(ShadowRadius) + " shadow_feather=" + N(ShadowFeather) +
                " goals_offset=" + N(GoalsOffset) + " materials_gap=" + N(MaterialsGap) + " column_inset=" + N(ColumnInset) + " font_profile=" + (CraftFont ? "craft" : "hud") + " outline=" + Outline +
                " alternate_numbers=" + AlternateNumbers + " alternate_quantity=" + AlternateQuantity + " title_craft_font=" + TitleCraftFont + " title_font=" + N(TitleFont) +
                " footer_craft_font=" + FooterCraftFont + " footer_font=" + N(FooterFont) + " icon_x=" + N(IconX) + " icon_y=" + N(IconY) +
                " goal_icon_spacing=" + N(GoalIconSpacing) + " material_icon_spacing=" + N(MaterialIconSpacing) +
                " feedback_x=" + N(FeedbackX) + " feedback_y=" + N(FeedbackY) + " feedback_gap=" + N(FeedbackGap) + " feedback_line_gap=" + N(FeedbackLineGap) + TuningRecord();
        }
        private static string N(float value) { return value.ToString("0.###", CultureInfo.InvariantCulture); }
    }
}
