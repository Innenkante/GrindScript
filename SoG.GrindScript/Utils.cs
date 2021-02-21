using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{

    public static class Utils
    {
        
        private static Assembly _assembly;
        private static IEnumerable<TypeInfo> _definedTypes;

        public static void Initialize(Assembly assembly)
        {
            _assembly = assembly;
            _definedTypes = _assembly.DefinedTypes;

        }

        public static void WriteToConsole(string message, string who = "Debug")
        {
            Console.WriteLine("[" + who + "] - " + message);
        }

        public static Game1 GetTheGame()
        {
            return (Game1)Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null);
        }

        public static TypeInfo GetGameType(string name)
        {
            return _definedTypes.First(t => t.FullName == name);
        }
    }

    static class TypeExtension
    {
        public static MethodInfo GetPublicInstanceMethod(this TypeInfo t,string name)
        {
            return t.GetMethod(name, BindingFlags.Public|BindingFlags.Instance);
        }

        public static MethodInfo[] GetPublicInstanceOverloadedMethods(this TypeInfo t, string name)
        {
            return t.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == name).ToArray();
        }

        public static MethodInfo GetPrivateInstanceMethod(this TypeInfo t, string name)
        {
            return t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static MethodInfo GetPrivateStaticMethod(this TypeInfo t, string name)
        {
            return t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static T GetPrivateInstanceField<T>(this TypeInfo t, object instance, string field)
        {
            return (T)t.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(instance);
        }

        public static T GetPublicInstanceField<T>(this TypeInfo t, object instance, string field)
        {
            return (T)t.GetField(field, BindingFlags.Instance | BindingFlags.Public)?.GetValue(instance);
        }

        public static T GetPublicStaticField<T>(this TypeInfo t, string field)
        {
            return (T) t.GetField(field, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        }

        public static dynamic GetPublicStaticField(this TypeInfo t, string field)
        {
            return t.GetField(field, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        }
    }
}
