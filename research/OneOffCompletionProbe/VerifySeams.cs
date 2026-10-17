// SPDX-License-Identifier: MPL-2.0
// Checks real host metadata only; no game methods are invoked.
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
internal static class VerifySeams
{
    private const BindingFlags All=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    private static void Method(Type owner,string name,Type result,params Type[] args)
    {
        var method=owner.GetMethod(name,All,null,args,null);
        if(method==null || method.IsStatic || method.ReturnType!=result) throw new Exception(owner.Name+"."+name);
    }
    private static int Main(string[] args)
    {
        try
        {
            AppDomain.CurrentDomain.AssemblyResolve+=(sender,e)=>{var p=Path.Combine(args[0],new AssemblyName(e.Name).Name+".dll"); return File.Exists(p)?Assembly.LoadFrom(p):null;};
            var host=Assembly.LoadFrom(Path.Combine(args[0],"Assembly-CSharp.dll"));
            var craft=host.GetType("CraftDefinition",true);var component=host.GetType("CraftComponent",true);
            var item=host.GetType("Item",true);var inventory=host.GetType("MultiInventory",true);var wgo=host.GetType("WorldGameObject",true);
            var listItem=typeof(List<>).MakeGenericType(item);
            Method(component,"CraftReally",typeof(bool),craft,item,typeof(List<string>),listItem,typeof(bool),typeof(int),typeof(bool),typeof(bool),typeof(bool));
            foreach(var name in new[]{"ProcessFinishedCraft","Cancel","End"}) Method(component,name,typeof(void));
            Method(inventory,"RemoveItems",typeof(bool),listItem,inventory.GetNestedType("DestinationType"),typeof(List<string>),listItem);
            Method(wgo,"ReplaceWithObject",typeof(void),typeof(string),typeof(bool),typeof(int));
            Method(host.GetType("GameSave",true),"OnFinishedCraft",typeof(void),craft);
            Method(host.GetType("BuildModeLogics",true),"CraftBuilding",typeof(void),craft);
            var ownerGetter=component.GetProperty("wgo",BindingFlags.Public|BindingFlags.Instance);
            if(ownerGetter==null || ownerGetter.PropertyType!=wgo) throw new Exception("public component.wgo");
            foreach(var pair in new[]{new KeyValuePair<string,Type>("current_craft",craft),new KeyValuePair<string,Type>("is_crafting",typeof(bool)),new KeyValuePair<string,Type>("destroyed",typeof(bool))})
            { var field=component.GetField(pair.Key,BindingFlags.Public|BindingFlags.Instance); if(field==null || field.FieldType!=pair.Value) throw new Exception(pair.Key); }
            Console.WriteLine("PASS: eight exact one-off observer targets and public state; host MVID="+host.ManifestModule.ModuleVersionId);
            return 0;
        }
        catch(Exception ex) {Console.Error.WriteLine(ex);return 1;}
    }
}
