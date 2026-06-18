using System.Reflection;

namespace AdaptiveBuildingMenu;

internal static class LayoutReflection
{
    public static T GetPrivateField<T>(FieldInfo field, object instance)
    {
        if (field == null) return default;

        return (T)field.GetValue(instance);
    }

    public static void SetPrivateField(FieldInfo field, object instance, object value)
    {
        field?.SetValue(instance, value);
    }
}
