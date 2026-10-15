// SPDX-License-Identifier: MPL-2.0
using System;
namespace CraftingPlanner
{
    // Project-owned geometry; independent of NGUI and the current input device.
    internal static class PlannerLayout
    {
        internal const int ArrowWidth = 20, QuantityWidth = 30;
        internal const int QuantitySlotWidth = ArrowWidth * 2 + QuantityWidth;
        internal static int QuantityX(int slotLeft) { return slotLeft + ArrowWidth; }

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
