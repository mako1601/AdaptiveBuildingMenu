using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace AdaptiveBuildingMenu;

public sealed class Plugin : KMod.UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        PUtil.InitLibrary();
        var options = new POptions();
        options.RegisterOptions(this, typeof(ModOptions));
        ModAssets.LoadOptions();
    }
}
