using System;
using System.Linq;
using System.Reflection;

namespace SoG.Modding.Extensions
{
    // I've seen that Harmony has some reflection utils. Maybe use those instead?

    public static class TypeExtension
    {
        // Quick, fast, dirty and easy "just give me the private stuff"

        public static MethodInfo GetPrivateMethod(this Type t, string name)
        {
            return t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        // Instance methods

        public static MethodInfo GetPublicInstanceMethod(this Type t, string name)
        {
            return t.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        }

        public static MethodInfo[] GetPublicInstanceOverloadedMethods(this Type t, string name)
        {
            return t.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == name).ToArray();
        }

        public static MethodInfo GetPrivateInstanceMethod(this Type t, string name)
        {
            return t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        // Static methods

        public static MethodInfo GetPublicStaticMethod(this Type t, string name)
        {
            return t.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        }

        public static MethodInfo GetPrivateStaticMethod(this Type t, string name)
        {
            return t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        }

        // Instance fields

        public static T GetPublicInstanceField<T>(this Type t, object instance, string field)
        {
            return (T)t.GetField(field, BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance);
        }

        public static T GetPrivateInstanceField<T>(this Type t, object instance, string field)
        {
            return (T)t.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(instance);
        }

        // Static fields

        public static T GetPublicStaticField<T>(this Type t, string field)
        {
            return (T)t.GetField(field, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        }

        public static T GetPrivateStaticField<T>(this Type t, string field)
        {
            return (T)t.GetField(field, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        }
    }
}
