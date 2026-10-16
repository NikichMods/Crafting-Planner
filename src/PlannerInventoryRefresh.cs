// SPDX-License-Identifier: MPL-2.0
using HarmonyLib;
namespace CraftingPlanner
{
    // Transfer events are batched; LateUpdate only consumes a flag, never polls inventory.
    internal static class PlannerInventoryRefresh
    {
        private static bool _pending;
        internal static void Mark() { _pending = true; }
        internal static bool Consume() { bool pending = _pending; _pending = false; return pending; }
        internal static void Reset() { _pending = false; }
        internal static void Flush()
        {
            if (Consume() && Planner.HasGoals && PlannerPresentation.GetView() != PlannerView.Hidden)
            {
                PlannerHud.RefreshIfVisible();
                Planner.LogLine("event=inventory_refresh trigger=native_transfer");
            }
        }
        internal static bool IncludesPlayer(MultiInventory inventory)
        {
            if (inventory == null || inventory.all == null || MainGame.me == null || MainGame.me.player == null) return false;
            Item player = MainGame.me.player.data;
            if (player == null) return false;
            foreach (Inventory owner in inventory.all) if (owner != null && ReferenceEquals(owner.data, player)) return true;
            return false;
        }
    }
    [HarmonyPatch(typeof(MultiInventory), "MoveItemTo", new[] { typeof(MultiInventory), typeof(Item), typeof(int), typeof(bool), typeof(bool) })]
    internal static class PlannerTransferRefreshPatch
    {
        private static void Postfix(MultiInventory __instance, MultiInventory __0, bool __result)
        {
            if (__result && Planner.HasGoals && (PlannerInventoryRefresh.IncludesPlayer(__instance) || PlannerInventoryRefresh.IncludesPlayer(__0))) PlannerInventoryRefresh.Mark();
        }
    }
}
