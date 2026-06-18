using System;
using UnityEngine;
using UnityEngine.UI;

namespace AdaptiveBuildingMenu;

internal sealed class AdaptiveLayoutContext
{
    public Transform GroupsRoot { get; set; }
    public RectTransform PanelRoot { get; set; }
    public RectTransform BuildButtonBackgroundPanel { get; set; }
    public RectTransform ContentsRect { get; set; }
    public RectTransform ProductInfoScreen { get; set; }
    public ScrollRect ContentsScrollRect { get; set; }
    public bool UsesSubcategoryLayout { get; set; }
    public float BuildGridRowHeight { get; set; }
    public float BuildGridBorderHeight { get; set; }
    public int MaxColumns { get; set; }
    public int MaxRows { get; set; }
    public Action<float> SetBuildGridWidth { get; set; }
}
