// SPDX-License-Identifier: MPL-2.0
using System.Collections.Generic;
namespace CraftingPlanner
{
    internal static class PlannerGoalState
    {
        internal const int MaxQuantity = 20;
        internal static string Key(ProjectContextKind context, string owner, string definition)
        { return ((int)context).ToString() + "|" + (owner ?? "") + "|" + definition; }
        internal static ProjectGoal FindPlacement(IList<ProjectGoal> goals, ObjectCraftDefinition definition, string builder)
        {
            if (definition == null || definition.build_type != ObjectCraftDefinition.BuildType.Put ||
                string.IsNullOrEmpty(builder) || string.IsNullOrEmpty(definition.id)) return null;
            string key = Key(ProjectContextKind.Builder, builder, definition.id);
            foreach (ProjectGoal goal in goals) if (goal.Repeatable == !definition.one_time_craft && goal.Key == key) return goal;
            return null;
        }
        internal static ProjectGoal FindWorldCompletion(IList<ProjectGoal> goals, CraftDefinition definition, string owner)
        {
            if (definition is ObjectCraftDefinition || !Planner.HasPlainNeeds(definition) || string.IsNullOrEmpty(definition.change_wgo) ||
                string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(definition.id)) return null;
            string key = Key(ProjectContextKind.WorldChange, owner, definition.id);
            foreach (ProjectGoal goal in goals) if (!goal.Repeatable && goal.Key == key) return goal;
            return null;
        }
        internal static ProjectGoal FindBuilderCompletion(IList<ProjectGoal> goals, CraftDefinition definition, string owner)
        {
            var build = definition as ObjectCraftDefinition;
            if (build == null || !build.one_time_craft || build.build_type != ObjectCraftDefinition.BuildType.None ||
                build.wait_script_callback || !build.is_auto || string.IsNullOrEmpty(build.end_script) || !Planner.HasPlainNeeds(build) ||
                string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(build.id)) return null;
            string key = Key(ProjectContextKind.Builder, owner, build.id);
            foreach (ProjectGoal goal in goals) if (!goal.Repeatable && goal.Key == key) return goal;
            return null;
        }
        internal static bool CanIncrease(int quantity, bool repeatable)
        { return quantity < (repeatable ? MaxQuantity : 1); }
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
                else if (CanIncrease(updated.Quantity, updated.Repeatable)) updated.Quantity++;
                else return false;
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
