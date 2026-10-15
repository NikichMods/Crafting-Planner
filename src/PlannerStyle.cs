// SPDX-License-Identifier: MPL-2.0
using System;
using System.Globalization;
namespace CraftingPlanner
{
    internal sealed class PlannerStyle
    {
        internal float Right = 4, Top = 73, Width = 240, Scale = .8f;
        internal float Font = 16, Icon = 30, Row = 30, Padding = 4;
        internal float Column = 44, ColumnGap = 2, TitleGap = 0, FooterGap = 8;
        internal float Opacity = 0.40f, TextOpacity = 0.95f, ShadowRadius = 24, ShadowFeather = 13;
        internal static PlannerStyle Current = new PlannerStyle();
        internal void Clamp()
        {
            Right = Bound(Right, 0, 400); Top = Bound(Top, 40, 400); Width = Bound(Width, 240, 640);
            Scale = Bound(Scale, 0.6f, 1.5f); Font = Bound(Font, 10, 24); Icon = Bound(Icon, 16, 40);
            Row = Bound(Row, 18, 48); Padding = Bound(Padding, 4, 24); Column = Bound(Column, 32, 110);
            ColumnGap = Bound(ColumnGap, 0, 20); TitleGap = Bound(TitleGap, 0, 28); FooterGap = Bound(FooterGap, 4, 32);
            Opacity = Bound(Opacity, 0, 0.85f); TextOpacity = Bound(TextOpacity, 0.5f, 1);
            ShadowRadius = Bound(ShadowRadius, 6, 90); ShadowFeather = Bound(ShadowFeather, 5, 28);
        }
        private static float Bound(float value, float min, float max)
        { return float.IsNaN(value) || float.IsInfinity(value) ? min : Math.Max(min, Math.Min(max, value)); }
        internal string Record()
        {
            return "right=" + N(Right) + " top=" + N(Top) + " width=" + N(Width) + " scale=" + N(Scale) +
                " font=" + N(Font) + " icon=" + N(Icon) + " row=" + N(Row) + " padding=" + N(Padding) +
                " column=" + N(Column) + " column_gap=" + N(ColumnGap) + " title_gap=" + N(TitleGap) +
                " footer_gap=" + N(FooterGap) + " opacity=" + N(Opacity) + " text_opacity=" + N(TextOpacity) + " shadow_radius=" + N(ShadowRadius) + " shadow_feather=" + N(ShadowFeather);
        }
        private static string N(float value) { return value.ToString("0.###", CultureInfo.InvariantCulture); }
    }
}
