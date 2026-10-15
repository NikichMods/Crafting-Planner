// SPDX-License-Identifier: MPL-2.0
using System.Collections.Generic;
namespace CraftingPlanner
{
    internal static class PlannerGoalState
    {
        internal static bool AdjustPinned(IList<ProjectGoal> goals, ProjectGoal pinned, int delta, out ProjectGoal updated)
        {
            updated = null;
            if (pinned == null || !goals.Contains(pinned) || pinned.Definition == null) return false;
            return Adjust(goals, pinned.Definition, pinned.Key, pinned.Repeatable, delta, out updated);
        }
        internal static bool Adjust(IList<ProjectGoal> goals, CraftDefinition definition, string key, bool repeatable, int delta, out ProjectGoal updated)
        {
            updated = null;
            if (definition == null || delta == 0) return false;
            foreach (ProjectGoal goal in goals) if (goal.Key == key) { updated = goal; break; }
            if (delta > 0)
            {
                if (updated == null)
                {
                    updated = new ProjectGoal { Key = key, Definition = definition, Quantity = 1, Repeatable = repeatable };
                    goals.Add(updated);
                }
                else if (updated.Repeatable && updated.Quantity < int.MaxValue) updated.Quantity++;
            }
            else
            {
                if (updated == null) return false;
                updated.Quantity--;
                if (updated.Quantity <= 0) goals.Remove(updated);
            }
            return true;
        }
    }
}
