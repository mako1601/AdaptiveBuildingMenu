namespace AdaptiveBuildingMenu;

internal static class ModAssets
{
    public static ModOptions Options { get; set; } = new ModOptions();

    public static void LoadOptions()
    {
        var loadedOptions = PeterHan.PLib.Options.POptions.ReadSettings<ModOptions>();
        if (loadedOptions != null)
        {
            Options = loadedOptions;
        }
    }
}
