using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace AdaptiveBuildingMenu;

[JsonObject]
[ModInfo("https://github.com/mako1601/AdaptiveBuildingMenu")]
public class ModOptions : IOptions
{
    [Option("Max Columns", "The maximum width of the build menu. When a category has too many buildings, the grid will expand sideways up to this number of columns to save space. Range 3..12")]
    public int MaxColumns { get; set; } = 8;

    [Option("Max Rows", "The maximum height of the build menu before a scrollbar appears. Limits how tall the menu can grow on your screen. Minimum: 1")]
    public int MaxRows { get; set; } = 8;

    public IEnumerable<IOptionsEntry> CreateOptions()
    {
        return null;
    }

    public void OnOptionsChanged()
    {
        ModAssets.Options = this;
    }
}
