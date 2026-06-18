using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdaptiveBuildingMenu;

internal static class LayoutGroups
{
    public static List<GridInfo> GetVisibleGroups(Transform groupsRoot)
    {
        var groups = new List<GridInfo>();
        if (groupsRoot == null) return groups;

        for (var index = 0; index < groupsRoot.childCount; index++)
        {
            var groupTransform = groupsRoot.GetChild(index);
            var grid = groupTransform.GetComponentInChildren<GridLayoutGroup>(true);

            if (grid == null) continue;

            groups.Add(new GridInfo(grid, LayoutUi.CountActiveChildren(grid.transform)));
        }

        return groups;
    }

    public static void ChooseColumnCounts(List<GridInfo> groups, int baseColumns, int maxColumns, int maxRows)
    {
        foreach (var group in groups)
        {
            group.Columns = baseColumns;
        }

        while (GetTotalRows(groups) > maxRows)
        {
            GridInfo bestGroup = null;
            var bestSavedRows = 0;

            foreach (var group in groups)
            {
                if (group.ActiveChildren <= 0 || group.Columns >= maxColumns) continue;

                var currentRows = GetRows(group.ActiveChildren, group.Columns);
                var nextRows = GetRows(group.ActiveChildren, group.Columns + 1);
                var savedRows = currentRows - nextRows;

                if (savedRows > bestSavedRows)
                {
                    bestGroup = group;
                    bestSavedRows = savedRows;
                }
            }

            if (bestGroup == null) break;

            bestGroup.Columns++;
        }
    }

    public static void ApplyGridSettings(List<GridInfo> groups, bool usesSubcategoryLayout, float panelWidth)
    {
        foreach (var group in groups)
        {
            group.Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            group.Grid.constraintCount = group.Columns;

            if (usesSubcategoryLayout)
            {
                var totalSpacing = group.Grid.spacing.x * Math.Max(0, group.Columns - 1);
                var cellWidth = Math.Max(1f, (panelWidth - 24f - totalSpacing) / group.Columns);
                group.Grid.cellSize = new Vector2(cellWidth, group.Grid.cellSize.y);
            }
        }
    }

    public static void RebuildGridLayouts(List<GridInfo> groups)
    {
        foreach (var group in groups)
        {
            if (group.Grid.transform is RectTransform rectTransform)
            {
                LayoutUi.RebuildLayout(rectTransform);
            }
        }
    }

    public static float GetReliableCellWidth(GridLayoutGroup grid, float baseWidth, int baseColumns, float maxBuildButtonWidth)
    {
        var cellWidth = grid.cellSize.x;

        if (cellWidth > 0f && cellWidth <= maxBuildButtonWidth) return cellWidth;

        var spacing = grid.spacing.x * Math.Max(0, baseColumns - 1);
        var padding = grid.padding.left + grid.padding.right;
        var derivedWidth = (baseWidth - padding - spacing) / Math.Max(1, baseColumns);

        if (derivedWidth > 0f && derivedWidth <= maxBuildButtonWidth) return derivedWidth;

        return 78f;
    }

    public static int GetTotalRows(List<GridInfo> groups)
    {
        var rows = 0;

        foreach (var group in groups)
        {
            rows += GetRows(group.ActiveChildren, group.Columns);
        }

        return rows;
    }

    public static int CountNonEmptyGroups(List<GridInfo> groups)
    {
        var count = 0;

        foreach (var group in groups)
        {
            if (group.ActiveChildren > 0)
            {
                count++;
            }
        }

        return count;
    }

    private static int GetRows(int itemCount, int columns)
    {
        if (itemCount <= 0) return 0;

        columns = Math.Max(1, columns);
        return (itemCount + columns - 1) / columns;
    }
}
