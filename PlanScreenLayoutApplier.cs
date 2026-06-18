using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace AdaptiveBuildingMenu;

internal static class PlanScreenLayoutApplier
{
    private static readonly FieldInfo BuildGridWidthField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_width");
    private static readonly FieldInfo BuildGridRowHeightField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_rowHeight");
    private static readonly FieldInfo BuildGridBorderHeightField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_borderHeight");
    private static readonly FieldInfo UseSubCategoryLayoutField = AccessTools.Field(typeof(PlanScreen), "useSubCategoryLayout");

    public static void Apply(PlanScreen screen)
    {
        if (screen == null) return;

        var context = new AdaptiveLayoutContext
        {
            GroupsRoot = screen.GroupsTransform,
            PanelRoot = screen.buildingGroupsRoot,
            BuildButtonBackgroundPanel = screen.BuildButtonBGPanel,
            ContentsRect = screen.BuildingGroupContentsRect,
            ProductInfoScreen = screen.ProductInfoScreen?.GetComponent<RectTransform>(),
            ContentsScrollRect = screen.BuildingGroupContentsRect?.GetComponent<ScrollRect>(),
            UsesSubcategoryLayout = LayoutReflection.GetPrivateField<bool>(UseSubCategoryLayoutField, screen),
            BuildGridRowHeight = LayoutReflection.GetPrivateField<float>(BuildGridRowHeightField, screen),
            BuildGridBorderHeight = LayoutReflection.GetPrivateField<float>(BuildGridBorderHeightField, screen),
            MaxColumns = ModAssets.Options?.MaxColumns ?? 8,
            MaxRows = ModAssets.Options?.MaxRows ?? 8,
            SetBuildGridWidth = width => LayoutReflection.SetPrivateField(BuildGridWidthField, screen, width)
        };

        AdaptiveMenuLayout.Apply(context);
    }
}
