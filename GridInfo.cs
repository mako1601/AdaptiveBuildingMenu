using UnityEngine.UI;

namespace AdaptiveBuildingMenu;

internal sealed class GridInfo
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
