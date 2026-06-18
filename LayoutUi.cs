using UnityEngine;
using UnityEngine.UI;

namespace AdaptiveBuildingMenu;

internal static class LayoutUi
{
    public static int CountActiveChildren(Transform parent)
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

    public static void SetRectSize(RectTransform rectTransform, float width, float height)
    {
        if (rectTransform == null) return;

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public static void SetRectWidth(RectTransform rectTransform, float width)
    {
        if (rectTransform == null) return;

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    public static void SetPreferredWidth(RectTransform rectTransform, float width)
    {
        if (rectTransform == null) return;

        var layoutElement = rectTransform.GetComponent<LayoutElement>() ?? rectTransform.gameObject.AddComponent<LayoutElement>();

        layoutElement.minWidth = width;
        layoutElement.preferredWidth = width;
    }

    public static void SetPreferredHeight(RectTransform rectTransform, float height)
    {
        if (rectTransform == null) return;

        var layoutElement = rectTransform.GetComponent<LayoutElement>() ?? rectTransform.gameObject.AddComponent<LayoutElement>();

        layoutElement.minHeight = height;
        layoutElement.preferredHeight = height;
    }

    public static void SetVerticalScrollbarVisible(ScrollRect scrollRect, bool visible)
    {
        var scrollbar = scrollRect?.verticalScrollbar;

        if (scrollbar == null) return;

        scrollbar.gameObject.SetActive(visible);
    }

    public static void RebuildLayout(RectTransform rectTransform)
    {
        if (rectTransform == null) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
