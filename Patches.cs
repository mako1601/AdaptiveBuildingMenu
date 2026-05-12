using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace AdaptiveBuildingMenu
{
    internal static class Patches
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

        private static readonly FieldInfo BuildGridWidthField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_width");
        private static readonly FieldInfo BuildGridRowHeightField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_rowHeight");
        private static readonly FieldInfo BuildGridBorderHeightField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_borderHeight");
        private static readonly FieldInfo UseSubCategoryLayoutField = AccessTools.Field(typeof(PlanScreen), "useSubCategoryLayout");

        [HarmonyPatch(typeof(PlanScreen), "ConfigurePanelSize")]
        private static class PlanScreenConfigurePanelSizePatch
        {
            private static void Postfix(PlanScreen __instance)
            {
                ApplyAdaptiveLayout(__instance);
            }
        }

        [HarmonyPatch(typeof(PlanScreen), "RefreshScale")]
        private static class PlanScreenRefreshScalePatch
        {
            private static void Postfix(PlanScreen __instance)
            {
                ApplyAdaptiveLayout(__instance);
            }
        }

        private static void ApplyAdaptiveLayout(PlanScreen screen)
        {
            if (screen == null) return;

            var groups = GetVisibleGroups(screen);
            if (groups.Count == 0) return;

            var usesSubcategoryLayout = GetPrivateField<bool>(UseSubCategoryLayoutField, screen);
            var baseColumns = usesSubcategoryLayout ? 1 : DefaultBuildColumns;
            var maxColumns = Math.Max(baseColumns, 8);
            var maxRows = GetMaxVisibleRows(screen);

            ChooseColumnCounts(groups, baseColumns, maxColumns, maxRows);

            var width = CalculatePanelWidth(groups, usesSubcategoryLayout, baseColumns);
            ApplyGridSettings(groups, usesSubcategoryLayout, width);
            RebuildGridLayouts(groups);
            width = CalculateMeasuredPanelWidth(screen, groups, width);
            ApplyGridSettings(groups, usesSubcategoryLayout, width);
            SetPrivateField(BuildGridWidthField, screen, width);
            ResizePanel(screen, groups, width, maxRows);
            MoveProductInfoScreen(screen, width);
        }

        private static List<GridInfo> GetVisibleGroups(PlanScreen screen)
        {
            var groups = new List<GridInfo>();
            if (screen.GroupsTransform == null) return groups;

            for (var index = 0; index < screen.GroupsTransform.childCount; index++)
            {
                var groupTransform = screen.GroupsTransform.GetChild(index);
                var grid = groupTransform.GetComponentInChildren<GridLayoutGroup>(true);

                if (grid == null) continue;

                var activeChildren = CountActiveChildren(grid.transform);
                groups.Add(new GridInfo(grid, activeChildren));
            }

            return groups;
        }

        private static int CountActiveChildren(Transform parent)
        {
            var count = 0;

            for (var index = 0; index < parent.childCount; index++)
            {
                if (parent.GetChild(index).gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ChooseColumnCounts(List<GridInfo> groups, int baseColumns, int maxColumns, int maxRows)
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

        private static void ApplyGridSettings(List<GridInfo> groups, bool usesSubcategoryLayout, float panelWidth)
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
            var cellWidth = GetReliableCellWidth(sampleGrid, baseWidth, baseColumns);
            var spacing = sampleGrid.spacing.x;

            if (usesSubcategoryLayout)
            {
                cellWidth = Math.Max(1f, baseWidth - 24f);
            }

            return baseWidth + (maxColumns - baseColumns) * (cellWidth + spacing);
        }

        private static float GetReliableCellWidth(GridLayoutGroup grid, float baseWidth, int baseColumns)
        {
            var cellWidth = grid.cellSize.x;

            if (cellWidth > 0f && cellWidth <= MaxBuildButtonWidth) return cellWidth;

            var spacing = grid.spacing.x * Math.Max(0, baseColumns - 1);
            var padding = grid.padding.left + grid.padding.right;
            var derivedWidth = (baseWidth - padding - spacing) / Math.Max(1, baseColumns);

            if (derivedWidth > 0f && derivedWidth <= MaxBuildButtonWidth) return derivedWidth;

            return 78f;
        }

        private static void RebuildGridLayouts(List<GridInfo> groups)
        {
            foreach (var group in groups)
            {
                if (group.Grid.transform is RectTransform rectTransform)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }
            }
        }

        private static float CalculateMeasuredPanelWidth(PlanScreen screen, List<GridInfo> groups, float fallbackWidth)
        {
            if (screen.buildingGroupsRoot == null) return fallbackWidth;

            var childCorners = new Vector3[4];
            var root = screen.buildingGroupsRoot;
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

        private static void ResizePanel(PlanScreen screen, List<GridInfo> groups, float width, int maxRows)
        {
            if (screen.buildingGroupsRoot == null) return;

            var totalRows = GetTotalRows(groups);
            var visibleRows = Math.Min(totalRows, maxRows);
            var rowHeight = GetPrivateField<float>(BuildGridRowHeightField, screen);
            var borderHeight = GetPrivateField<float>(BuildGridBorderHeightField, screen);
            var headerHeight = Math.Min(MaxHeaderHeight, CountNonEmptyGroups(groups) * HeaderHeight);
            var height = borderHeight + headerHeight + SearchBarHeight + visibleRows * rowHeight;

            SetRectSize(screen.buildingGroupsRoot, width, height);
            SetRectWidth(screen.BuildButtonBGPanel, width);
            SetRectWidth(screen.BuildingGroupContentsRect, width);
            SetScrollbar(screen, totalRows > maxRows);
            LayoutRebuilder.ForceRebuildLayoutImmediate(screen.buildingGroupsRoot);
        }

        private static void SetRectSize(RectTransform rectTransform, float width, float height)
        {
            if (rectTransform == null) return;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private static void SetRectWidth(RectTransform rectTransform, float width)
        {
            if (rectTransform == null) return;

            if (!Mathf.Approximately(rectTransform.anchorMin.x, rectTransform.anchorMax.x))
            {
                rectTransform.offsetMin = new Vector2(0f, rectTransform.offsetMin.y);
                rectTransform.offsetMax = new Vector2(0f, rectTransform.offsetMax.y);
                return;
            }

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private static void SetScrollbar(PlanScreen screen, bool needsScroll)
        {
            var scrollRect = screen.BuildingGroupContentsRect?.GetComponent<ScrollRect>();
            var scrollbar = scrollRect?.verticalScrollbar;

            if (scrollbar == null) return;

            scrollbar.gameObject.SetActive(needsScroll);
        }

        private static void MoveProductInfoScreen(PlanScreen screen, float menuWidth)
        {
            if (screen.ProductInfoScreen == null) return;

            var rectTransform = screen.ProductInfoScreen.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            rectTransform.anchoredPosition = new Vector2(menuWidth + ProductInfoGap, rectTransform.anchoredPosition.y);
        }

        private static int GetMaxVisibleRows(PlanScreen screen)
        {
            var rowHeight = Math.Max(1f, GetPrivateField<float>(BuildGridRowHeightField, screen));
            var rowsThatFitOnScreen = Math.Max(1, (int)(Screen.height / rowHeight) - 3);
            return Math.Max(1, Math.Min(8, rowsThatFitOnScreen));
        }

        private static int GetTotalRows(List<GridInfo> groups)
        {
            var rows = 0;

            foreach (var group in groups)
            {
                rows += GetRows(group.ActiveChildren, group.Columns);
            }

            return rows;
        }

        private static int CountNonEmptyGroups(List<GridInfo> groups)
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

        private static T GetPrivateField<T>(FieldInfo field, object instance)
        {
            if (field == null) return default;

            return (T)field.GetValue(instance);
        }

        private static void SetPrivateField(FieldInfo field, object instance, object value)
        {
            field?.SetValue(instance, value);
        }

        private sealed class GridInfo
        {
            public GridInfo(GridLayoutGroup grid, int activeChildren)
            {
                Grid = grid;
                ActiveChildren = activeChildren;
            }

            public GridLayoutGroup Grid { get; }
            public int ActiveChildren { get; }
            public int Columns { get; set; }
        }
    }
}
