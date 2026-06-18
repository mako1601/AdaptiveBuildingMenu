using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace AdaptiveBuildingMenu;

[JsonObject]
[ModInfo("https://github.com/mako1601/AdaptiveBuildingMenu")]
public class ModOptions : IOptions
{
    [Option("Build Menu Columns", "Maximum number of columns in the build menu grid. Higher values make the menu wider instead of taller. Range: 3..12")]
    public int MaxColumns { get; set; } = 8;

    [Option("Build Menu Rows", "Maximum number of visible rows in the build menu before scrolling starts. Lower values keep the menu shorter. Minimum: 1")]
    public int MaxRows { get; set; } = 8;

    [Option("Material Columns", "Number of material columns shown in the material selector. Minimum: 5")]
    public int MaterialColumns { get; set; } = 5;

    [Option("Material Rows", "Maximum number of visible material rows in the material selector before scrolling starts. Minimum: 2")]
    public int MaterialRows { get; set; } = 2;

    public IEnumerable<IOptionsEntry> CreateOptions()
    {
        return null;
    }

    public void OnOptionsChanged()
    {
        ModAssets.Options = this;
    }
}
