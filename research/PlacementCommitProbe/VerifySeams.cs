// SPDX-License-Identifier: MPL-2.0
// Metadata-only check against real host references, without launching Unity.
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

internal static class VerifySeams
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static MethodInfo Method(Type type, string name, Type result, params Type[] arguments)
    {
        var method = type.GetMethod(name, All, null, arguments, null);
        Require(method != null && method.ReturnType == result, type.Name + "." + name);
        return method;
    }
    private static void Field(Type owner, string name, Type type, bool isStatic)
    {
        var field = owner.GetField(name, All);
        Require(field != null && field.FieldType == type && field.IsStatic == isStatic, owner.Name + "." + name);
    }
    private static int Main(string[] args)
    {
        try
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
                var file = Path.Combine(args[0], new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(file) ? Assembly.LoadFrom(file) : null;
            };
            var host = Assembly.LoadFrom(Path.Combine(args[0], "Assembly-CSharp.dll"));
            var build = host.GetType("BuildModeLogics", true);
            var craft = host.GetType("ObjectCraftDefinition", true);
            var inventory = host.GetType("MultiInventory", true);
            var wgo = host.GetType("WorldGameObject", true);
            var floating = host.GetType("FloatingWorldGameObject", true);
            var item = host.GetType("Item", true);
            var listItem = typeof(List<>).MakeGenericType(item);
            var destination = inventory.GetNestedType("DestinationType");
            Require(Method(build, "DoPlace", typeof(void)).IsPrivate, "DoPlace visibility");
            Method(build, "CancelPlacing", typeof(void));
            Field(build, "_cd", craft, false);
            Field(build, "_multi_inventory", inventory, false);
            Field(build, "last_build_desk", wgo, true);
            Field(wgo, "unique_id", typeof(long), false);
            Field(wgo, "obj_id", typeof(string), false);
            Field(wgo, "just_built", typeof(bool), false);
            Field(floating, "can_be_built", typeof(bool), true);
            Require(Method(floating, "get_cur_floating", floating).IsStatic, "floating getter");
            Method(floating, "get_wobj", wgo);
            Method(floating, "StopCurrentFloating", typeof(void), typeof(bool));
            Method(wgo, "Redraw", typeof(void), typeof(bool), typeof(bool), typeof(bool));
            Method(inventory, "RemoveItems", typeof(bool), listItem, destination, typeof(List<string>), listItem);
            Method(inventory, "GetTotalCount", typeof(int), typeof(string), destination, typeof(bool));
            Console.WriteLine("PASS: exact placement probe host signatures; MVID=" + host.ManifestModule.ModuleVersionId);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
