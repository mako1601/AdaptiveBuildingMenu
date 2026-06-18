using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace AdaptiveBuildingMenu;

internal static class MaterialSelectorLayoutApplier
{
    private const int DefaultTargetColumns = 5;
    private const int DefaultTargetRows = 2;
    private const float FallbackCellWidth = 78f;
    private const float FallbackCellHeight = 28f;
    private const float WidthPadding = 14f;
    private static readonly FieldInfo ScrollRectField = AccessTools.Field(typeof(MaterialSelector), "ScrollRect");

    public static void Apply(MaterialSelector selector)
    {
        if (selector == null) return;

        var layoutContainer = selector.LayoutContainer?.GetComponent<RectTransform>();
        if (layoutContainer == null) return;

        var grid = selector.LayoutContainer.GetComponentInChildren<GridLayoutGroup>(true);
        if (grid == null) return;

        var scrollRect = LayoutReflection.GetPrivateField<ScrollRect>(ScrollRectField, selector);
        var visibleRows = GetVisibleRows(grid.transform);

        ApplyGridSettings(grid);
        ResizeContainer(layoutContainer, scrollRect, grid, visibleRows);
        LayoutUi.RebuildLayout(layoutContainer);
    }

    public static float MeasureRequiredWidth(MaterialSelector selector)
    {
        if (selector == null) return 0f;

        var grid = selector.LayoutContainer?.GetComponentInChildren<GridLayoutGroup>(true);
        if (grid == null) return 0f;

        var cellWidth = grid.cellSize.x > 0f ? grid.cellSize.x : FallbackCellWidth;
        var targetColumns = GetTargetColumns();
        return grid.padding.left + grid.padding.right + (targetColumns * cellWidth) + ((targetColumns - 1) * grid.spacing.x) + WidthPadding;
    }

    private static void ApplyGridSettings(GridLayoutGroup grid)
    {
        var targetColumns = GetTargetColumns();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = targetColumns;

        var cellWidth = grid.cellSize.x > 0f ? grid.cellSize.x : FallbackCellWidth;
        var cellHeight = grid.cellSize.y > 0f ? grid.cellSize.y : FallbackCellHeight;
        grid.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private static void ResizeContainer(RectTransform layoutContainer, ScrollRect scrollRect, GridLayoutGroup grid, int visibleRows)
    {
        var targetColumns = GetTargetColumns();
        var targetRows = GetTargetRows();
        var cellWidth = grid.cellSize.x;
        var width = grid.padding.left + grid.padding.right + (targetColumns * cellWidth) + ((targetColumns - 1) * grid.spacing.x) + WidthPadding;

        LayoutUi.SetRectWidth(layoutContainer, width);
        LayoutUi.SetPreferredWidth(layoutContainer, width);

        var rowsToShow = Mathf.Clamp(visibleRows, 1, targetRows);
        var visibleHeight = grid.padding.top + grid.padding.bottom + (rowsToShow * grid.cellSize.y) + ((rowsToShow - 1) * grid.spacing.y);
        var viewport = scrollRect?.viewport;
        if (viewport != null)
        {
            LayoutUi.SetRectSize(viewport, width, visibleHeight);
            LayoutUi.SetPreferredWidth(viewport, width);
            LayoutUi.SetPreferredHeight(viewport, visibleHeight);
        }
    }

    private static int GetVisibleRows(Transform gridParent)
    {
        var targetColumns = GetTargetColumns();
        var activeChildren = LayoutUi.CountActiveChildren(gridParent);
        if (activeChildren <= 0) return 1;

        return (activeChildren + targetColumns - 1) / targetColumns;
    }

    private static int GetTargetColumns()
    {
        var configuredColumns = ModAssets.Options?.MaterialColumns ?? DefaultTargetColumns;
        return Mathf.Max(DefaultTargetColumns, configuredColumns);
    }

    private static int GetTargetRows()
    {
        var configuredRows = ModAssets.Options?.MaterialRows ?? DefaultTargetRows;
        return Mathf.Max(DefaultTargetRows, configuredRows);
    }
}
