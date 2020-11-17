using System;
using System.IO;
using UnityEngine;

namespace DisruptorBeam.Modules
{
   public interface IConfigurationConstants
   {

      string GetSourcePath(Type type);
   }

   public class BeamableConfigurationConstants : IConfigurationConstants
   {
      private const string PACKAGE_EDITOR_DIR = "Packages/com.disruptorbeam.engine/Editor/Modules";
      private const string MODULE_CONFIG_DIR = "Config";

      public string GetSourcePath(Type type)
      {
         var name = type.Name;
         var moduleName = ModuleConfigurationUtil.GetModuleName(type);
         var sourcePath = $"{PACKAGE_EDITOR_DIR}/{moduleName}/{MODULE_CONFIG_DIR}/{name}.asset";
         return sourcePath;
      }
   }

   public static class ModuleConfigurationUtil
   {
      private const string CONFIGURATION = "Configuration";

      public static string GetModuleName(Type configurationType)
      {

#if UNITY_EDITOR

         if (!configurationType.Name.EndsWith(CONFIGURATION))
         {
            throw new Exception($"A module configuration object class name must always end with the literal string, \"{CONFIGURATION}\"");
         }

         var moduleName = configurationType.Name.Substring(0, configurationType.Name.Length - CONFIGURATION.Length);
         return moduleName;
#else
         throw new NotImplementedException();
#endif

      }

   }

   public abstract class BaseModuleConfigurationObject : ScriptableObject
   {

   }

   public abstract class AbsModuleConfigurationObject<TConstants> : BaseModuleConfigurationObject
      where TConstants : IConfigurationConstants, new()
   {
      private const string CONFIG_RESOURCES_DIR = "Assets/DisruptorEngine/Resources";

      public static TConfig Get<TConfig>() where TConfig : BaseModuleConfigurationObject
      {
         var constants = new TConstants();
         var type = typeof(TConfig);
         var name = type.Name;

         var data = Resources.Load<TConfig>(name);
#if UNITY_EDITOR
         if (data == null)
         {

            var sourcePath = constants.GetSourcePath(type);

            if (!File.Exists(sourcePath))
            {
               throw new Exception($"No module configuration exists at {sourcePath}. Please create it.");
            }

            Directory.CreateDirectory(CONFIG_RESOURCES_DIR);
            var sourceData = File.ReadAllText(sourcePath);
            File.WriteAllText($"{CONFIG_RESOURCES_DIR}/{name}.asset", sourceData);
            UnityEditor.AssetDatabase.Refresh();
            data = Resources.Load<TConfig>(name);
         }
#endif
         return data;
      }

   }


   public class ModuleConfigurationObject : AbsModuleConfigurationObject<BeamableConfigurationConstants>
   {

   }

}