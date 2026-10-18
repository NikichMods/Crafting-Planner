// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
namespace CraftingPlanner
{
    // Project-owned geometry; independent of NGUI and the current input device.
    internal static class PlannerLayout
    {
        internal const int IconSlotWidth = 30;
        internal static bool NumericCraftFont(bool bodyCraft, bool allNumbers, bool quantityOnly, bool quantity)
        { return bodyCraft ^ (allNumbers || quantity && quantityOnly); }
        internal static int TextRowHeight(int row, int font, int wrappedHeight)
        { return Math.Max(Math.Max(row, font + 4), wrappedHeight); }
        internal static int MinimumWidth(int padding, int column, int gap)
        { return 2 * padding + IconSlotWidth + 8 + 3 * column + 2 * gap; }
        internal static int QuantityCenter(int width, int padding, int inset)
        { return width - padding - QuantitySlotWidth + ArrowWidth + QuantityWidth / 2 - inset; }
        internal const int ArrowWidth = 20, QuantityWidth = 30;
        internal const int QuantitySlotWidth = ArrowWidth * 2 + QuantityWidth;
        internal const int MenuIngredientsOrigin = 29;
        internal static float QuantityArrowOffset(float numberWidth, float arrowWidth, float gap)
        { return numberWidth / 2 + Math.Max(0, gap) + arrowWidth / 2; }
        internal static int GoalNameRight(int centre, float numberWidth, float arrowWidth, float gap)
        {
            float offset = QuantityArrowOffset(numberWidth, arrowWidth, gap);
            // Separate text from both visible ink and the larger clickable widget.
            return (int)Math.Floor(centre - offset - Math.Max(ArrowWidth, arrowWidth) / 2f - 4);
        }
        internal static int ClampSelection(int index, int count) { return Math.Max(0, Math.Min(index, count - 1)); }

        internal static int[] RightAlignedStarts(int[] widths, int right, int gap)
        {
            int[] starts = new int[widths.Length];
            for (int i = widths.Length - 1; i >= 0; i--) { starts[i] = right - widths[i]; right = starts[i] - gap; }
            return starts;
        }

        internal static int[] ResultRowStarts(int[] widths, int limit, int gap)
        {
            var rows = new List<int>(); int occupied = 0;
            for (int i = 0; i < widths.Length; i++)
            {
                if (i == 0 || occupied + gap + widths[i] > limit) { rows.Add(i); occupied = widths[i]; }
                else occupied += gap + widths[i];
            }
            return rows.ToArray();
        }

        internal static float BackingAlpha(float x, float y, float width, float height, float radius, float feather)
        {
            if (width <= 0 || height <= 0 || feather <= 0) return 0;
            radius = Math.Max(0, Math.Min(radius, Math.Min(width, height) / 2));
            float qx = Math.Abs(x - width / 2) - (width / 2 - radius);
            float qy = Math.Abs(y - height / 2) - (height / 2 - radius);
            double outside = Math.Sqrt(Math.Pow(Math.Max(qx, 0), 2) + Math.Pow(Math.Max(qy, 0), 2));
            double distance = outside + Math.Min(Math.Max(qx, qy), 0) - radius;
            double blend = Math.Max(0, Math.Min(1, .5 - distance / (2 * feather)));
            return (float)(blend * blend * (3 - 2 * blend));
        }
    }
}
