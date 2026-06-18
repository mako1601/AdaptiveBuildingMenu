using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace AdaptiveBuildingMenu;

internal static class MaterialSelectionPanelLayoutApplier
{
    private static readonly FieldInfo MaterialSelectorsField = AccessTools.Field(typeof(MaterialSelectionPanel), "materialSelectors");

    public static void Apply(MaterialSelectionPanel panel)
    {
        if (panel == null) return;

        var root = panel.transform as RectTransform;
        if (root == null) return;

        var selectors = GetMaterialSelectors(panel);
        var requiredWidth = GetRequiredWidth(selectors);
        if (requiredWidth <= 0f) return;

        LayoutUi.SetRectWidth(root, requiredWidth);
        LayoutUi.SetPreferredWidth(root, requiredWidth);

        if (selectors != null)
        {
            foreach (var selector in selectors)
            {
                MaterialSelectorLayoutApplier.Apply(selector);
            }
        }

        LayoutUi.RebuildLayout(root);
    }

    private static float GetRequiredWidth(List<MaterialSelector> selectors)
    {
        if (selectors == null || selectors.Count == 0) return 0f;

        var maxWidth = 0f;
        foreach (var selector in selectors)
        {
            maxWidth = Math.Max(maxWidth, MaterialSelectorLayoutApplier.MeasureRequiredWidth(selector));
        }

        return maxWidth;
    }

    private static List<MaterialSelector> GetMaterialSelectors(MaterialSelectionPanel panel)
    {
        return LayoutReflection.GetPrivateField<List<MaterialSelector>>(MaterialSelectorsField, panel);
    }
}
