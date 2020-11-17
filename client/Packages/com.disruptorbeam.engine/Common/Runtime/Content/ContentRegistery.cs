using System;
using System.Collections.Generic;
using System.Reflection;
using DisruptorBeam.Content;
using UnityEngine;

namespace Beamable.Common.Content
{
   public static class ContentRegistry
   {
      private static readonly Dictionary<string, Type> contentTypeToClass = new Dictionary<string, Type>();
      private static readonly Dictionary<Type, string> classToContentType = new Dictionary<Type, string>();

      static ContentRegistry()
      {
         ScanAssemblies();
      }


      private static void ScanAssemblies()
      {
         contentTypeToClass.Clear();
         classToContentType.Clear();
         var assemblies = AppDomain.CurrentDomain.GetAssemblies();
         foreach (var assembly in assemblies)
         {
            var asmName = assembly.GetName().Name;
            if ("Tests".Equals(asmName)) continue; // TODO think harder about this.
            try
            {
               foreach (var type in assembly.GetTypes())
               {

                  bool hasContentAttribute = type.GetCustomAttribute<ContentTypeAttribute>(false) != null;
                  bool isAssignableFromIContentObject = typeof(IContentObject).IsAssignableFrom(type);

                  #if !DB_MICROSERVICE
                  bool isAssignableFromScriptableObject = typeof(ScriptableObject).IsAssignableFrom(type);
                  #else
                  bool isAssignableFromScriptableObject = true;
                  #endif

                  if (hasContentAttribute && isAssignableFromIContentObject && isAssignableFromScriptableObject)
                  {
                     string typeName = GetContentTypeName(type);
                     contentTypeToClass[typeName] = type;
                     classToContentType[type] = typeName;
                  }
               }
            }
            catch (Exception ex)
            {
               Debug.LogError(ex);
            }
         }
      }

      public static string GetContentTypeName(Type contentType)
      {

         #if !DB_MICROSERVICE
         if (contentType == typeof(ScriptableObject))
            return null;
         #endif

         var contentTypeAttribute = contentType.GetCustomAttribute<ContentTypeAttribute>(false);
         if (contentTypeAttribute == null)
            return GetContentTypeName(contentType.BaseType);

         var startType = contentTypeAttribute.TypeName;
         var endType = GetContentTypeName(contentType.BaseType);
         if (startType != null && endType != null)
         {
            return string.Join(".", endType, startType);
         }
         else
         {
            return endType ?? startType;
         }
      }

      public static IEnumerable<Type> GetContentTypes()
      {
         return classToContentType.Keys;
      }

      public static IEnumerable<string> GetContentClassIds()
      {
         return contentTypeToClass.Keys;
      }

      public static Type GetTypeFromId(string id)
      {
         return contentTypeToClass[id.Substring(0, id.LastIndexOf("."))];
      }

      public static string TypeToName(Type type)
      {
         if (classToContentType.TryGetValue(type, out var name))
         {
            return name;
         }
         else
         {
            throw new Exception($"No content name found for type=[{type.Name}]");
         }
      }
   }
}