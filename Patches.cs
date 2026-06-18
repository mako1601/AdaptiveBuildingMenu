using HarmonyLib;

namespace AdaptiveBuildingMenu;

internal static class Patches
{
    [HarmonyPatch(typeof(PlanScreen), "ConfigurePanelSize")]
    private static class PlanScreenConfigurePanelSizePatch
    {
        private static void Postfix(PlanScreen __instance)
        {
            PlanScreenLayoutApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(PlanScreen), "RefreshScale")]
    private static class PlanScreenRefreshScalePatch
    {
        private static void Postfix(PlanScreen __instance)
        {
            PlanScreenLayoutApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(MaterialSelector), "OnPrefabInit")]
    private static class MaterialSelectorOnPrefabInitPatch
    {
        private static void Postfix(MaterialSelector __instance)
        {
            MaterialSelectorLayoutApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(MaterialSelector), "ConfigureScreen")]
    private static class MaterialSelectorConfigureScreenPatch
    {
        private static void Postfix(MaterialSelector __instance)
        {
            MaterialSelectorLayoutApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(MaterialSelectionPanel), "OnPrefabInit")]
    private static class MaterialSelectionPanelOnPrefabInitPatch
    {
        private static void Postfix(MaterialSelectionPanel __instance)
        {
            MaterialSelectionPanelLayoutApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(MaterialSelectionPanel), "ConfigureScreen")]
    private static class MaterialSelectionPanelConfigureScreenPatch
    {
        private static void Postfix(MaterialSelectionPanel __instance)
        {
            MaterialSelectionPanelLayoutApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(MaterialSelectionPanel), "RefreshSelectors")]
    private static class MaterialSelectionPanelRefreshSelectorsPatch
    {
        private static void Postfix(MaterialSelectionPanel __instance)
        {
            MaterialSelectionPanelLayoutApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(MaterialSelectionPanel), "UpdateResourceToggleValues")]
    private static class MaterialSelectionPanelUpdateResourceToggleValuesPatch
    {
        private static void Postfix(MaterialSelectionPanel __instance)
        {
            MaterialSelectionPanelLayoutApplier.Apply(__instance);
        }
    }
}
