namespace EZData
{
    using System;
    using System.Reflection;

    public static class NDataReflectionUtils
    {
#if !UNITY_EDITOR && UNITY_WINRT
        public static bool IsValueType(Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static PropertyInfo GetProperty(Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }

        public static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }

        public static bool IsEnum(Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static FieldInfo GetField(Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredField(name);
        }

        public static Delegate CreateDelegate(Type type, object target, MethodInfo method)
        {
            return method.CreateDelegate(type, target);
        }
#else
        public static bool IsValueType(Type type)
        {
            return type.IsValueType;
        }

        public static PropertyInfo GetProperty(Type type, string name)
        {
            return type.GetProperty(name);
        }

        public static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetMethod(name);
        }

        public static bool IsEnum(Type type)
        {
            return type.IsEnum;
        }

        public static FieldInfo GetField(Type type, string name)
        {
            return type.GetField(name);
        }

        public static Delegate CreateDelegate(Type type, object target, MethodInfo method)
        {
            return Delegate.CreateDelegate(type, target, method);
        }
#endif
    }
}