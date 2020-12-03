using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

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

        public static dynamic GetTheGame()
        {
            return Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null);
        }

        public static dynamic GetTheContent()
        {
            return GetTheGame().Content;
        }

        public static dynamic GetNullTex()
        {
            return Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
        }

        public static dynamic GetLocalPlayer()
        {
            return Utils.GetTheGame().xLocalPlayer;
        }

        public static TypeInfo GetGameType(string name)
        {
            return _definedTypes.First(t => t.FullName == name);
        }

        public static dynamic GetEnumObject(string enumFullName, int value)
        {
            return Enum.ToObject(GetGameType(enumFullName), value);
        }

        public static dynamic DefaultConstructObject(string typeFullName)
        {
            return Activator.CreateInstance(GetGameType(typeFullName));
        }

        public static dynamic ConstructObject(string typeFullName, params object[] args)
        {
            return Activator.CreateInstance(GetGameType(typeFullName), args);
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
