using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdaptiveBuildingMenu;

internal static class AdaptiveMenuLayout
{
    private const int DefaultBuildColumns = 3;
    private const int HeaderHeight = 24;
    private const int MaxHeaderHeight = 72;
    private const float VanillaPanelWidth = 264f;
    private const float WideVanillaPanelWidth = 320f;
    private const float SearchBarHeight = 36f;
    private const float ProductInfoGap = 8f;
    private const float PanelRightPadding = 18f;
    private const float MaxBuildButtonWidth = 120f;

    public static void Apply(AdaptiveLayoutContext context)
    {
        if (context == null) return;

        var groups = LayoutGroups.GetVisibleGroups(context.GroupsRoot);
        if (groups.Count == 0) return;

        var baseColumns = context.UsesSubcategoryLayout ? 1 : DefaultBuildColumns;
        var maxColumns = Mathf.Clamp(context.MaxColumns, baseColumns, 12);
        var targetMaxRows = GetMaxVisibleRows(context);

        LayoutGroups.ChooseColumnCounts(groups, baseColumns, maxColumns, targetMaxRows);

        var totalActualRows = LayoutGroups.GetTotalRows(groups);
        var visibleRowsForPanel = Math.Min(totalActualRows, targetMaxRows);

        var width = CalculatePanelWidth(groups, context.UsesSubcategoryLayout, baseColumns);
        LayoutGroups.ApplyGridSettings(groups, context.UsesSubcategoryLayout, width);
        LayoutGroups.RebuildGridLayouts(groups);
        width = CalculateMeasuredPanelWidth(context, groups, width);
        LayoutGroups.ApplyGridSettings(groups, context.UsesSubcategoryLayout, width);
        context.SetBuildGridWidth?.Invoke(width);
        ResizePanel(context, groups, width, visibleRowsForPanel);
        MoveProductInfoScreen(context, width);
    }

    private static float CalculatePanelWidth(List<GridInfo> groups, bool usesSubcategoryLayout, int baseColumns)
    {
        var maxColumns = baseColumns;

        foreach (var group in groups)
        {
            maxColumns = Math.Max(maxColumns, group.Columns);
        }

        var baseWidth = usesSubcategoryLayout ? WideVanillaPanelWidth : VanillaPanelWidth;
        if (maxColumns <= baseColumns) return baseWidth;

        var sampleGrid = groups[0].Grid;
        var cellWidth = LayoutGroups.GetReliableCellWidth(sampleGrid, baseWidth, baseColumns, MaxBuildButtonWidth);
        var spacing = sampleGrid.spacing.x;

        if (usesSubcategoryLayout)
        {
            cellWidth = Math.Max(1f, baseWidth - 24f);
        }

        return baseWidth + (maxColumns - baseColumns) * (cellWidth + spacing);
    }

    private static float CalculateMeasuredPanelWidth(AdaptiveLayoutContext context, List<GridInfo> groups, float fallbackWidth)
    {
        if (context.PanelRoot == null) return fallbackWidth;

        var childCorners = new Vector3[4];
        var root = context.PanelRoot;
        var rootLeft = root.rect.xMin;
        var right = rootLeft;

        foreach (var group in groups)
        {
            for (var index = 0; index < group.Grid.transform.childCount; index++)
            {
                var child = group.Grid.transform.GetChild(index);
                if (!child.gameObject.activeSelf || child is not RectTransform childRect) continue;

                childRect.GetWorldCorners(childCorners);

                for (var corner = 0; corner < childCorners.Length; corner++)
                {
                    var localCorner = root.InverseTransformPoint(childCorners[corner]);
                    right = Math.Max(right, localCorner.x);
                }
            }
        }

        var measuredWidth = right - rootLeft + PanelRightPadding;
        if (measuredWidth <= 0f) return fallbackWidth;

        var baseWidth = Math.Min(fallbackWidth, WideVanillaPanelWidth);
        return Math.Max(baseWidth, Math.Min(measuredWidth, fallbackWidth));
    }

    private static void ResizePanel(AdaptiveLayoutContext context, List<GridInfo> groups, float width, int maxRows)
    {
        if (context.PanelRoot == null) return;

        var totalRows = LayoutGroups.GetTotalRows(groups);
        var visibleRows = Math.Min(totalRows, maxRows);
        var headerHeight = Math.Min(MaxHeaderHeight, LayoutGroups.CountNonEmptyGroups(groups) * HeaderHeight);
        var height = context.BuildGridBorderHeight + headerHeight + SearchBarHeight + visibleRows * context.BuildGridRowHeight;

        LayoutUi.SetRectSize(context.PanelRoot, width, height);
        LayoutUi.SetRectWidth(context.BuildButtonBackgroundPanel, width);
        LayoutUi.SetRectWidth(context.ContentsRect, width);
        LayoutUi.SetVerticalScrollbarVisible(context.ContentsScrollRect, totalRows > maxRows);
        LayoutUi.RebuildLayout(context.PanelRoot);
    }

    private static void MoveProductInfoScreen(AdaptiveLayoutContext context, float menuWidth)
    {
        if (context.ProductInfoScreen == null) return;

        context.ProductInfoScreen.anchoredPosition = new Vector2(menuWidth + ProductInfoGap, context.ProductInfoScreen.anchoredPosition.y);
    }

    private static int GetMaxVisibleRows(AdaptiveLayoutContext context)
    {
        var rowHeight = Math.Max(1f, context.BuildGridRowHeight);
        var rowsThatFitOnScreen = Math.Max(1, (int)(Screen.height / rowHeight) - 3);
        var configMaxRows = Mathf.Max(1, context.MaxRows);

        return Math.Min(configMaxRows, rowsThatFitOnScreen);
    }
}
