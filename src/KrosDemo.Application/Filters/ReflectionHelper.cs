using System.Reflection;

namespace KrosDemo.Application.Filters;

public static class ReflectionHelper
{
    public static Type? GetPropertyType<T>(string propertyName)
    {
        return GetPropertyInfo<T>(propertyName)?.PropertyType;
    }

    public static object? GetPropertyValue<T>(this T source, string propertyName)
    {
        return GetPropertyInfo<T>(propertyName)?.GetValue(source);
    }

    public static PropertyInfo? GetPropertyInfo<T>(string propertyName)
    {
        return typeof(T).GetProperty(propertyName);
    }
}