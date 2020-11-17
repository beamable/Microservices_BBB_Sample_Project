using System;
using System.Collections.Generic;
using System.Reflection;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Server;
using Core.Platform.SDK;
using UnityEngine;

namespace DisruptorBeam.Content
{
   public class ContentRef<TContent> : AbsContentRef<TContent>
      where TContent : IContentObject, new()
   {
      public override Promise<TContent> Resolve()
      {
         return BeamableMicroService._contentService.Resolve(this);
      }
   }

   public class ContentLink<TContent> : AbsContentLink<TContent>
      where TContent : IContentObject, new()
   {
      public override Promise<TContent> Resolve()
      {
         return BeamableMicroService._contentService.Resolve(this);
      }

      public override void OnCreated()
      {
         Resolve();
      }
   }

   public class ContentObject : ScriptableObject, IContentObject
   {
      private static readonly Dictionary<Type, string> _contentTypeNames = new Dictionary<Type, string>();

      public string ContentName { get; private set; }

      public string ContentType => GetContentType(GetType());
      public string Id => $"{ContentType}.{ContentName}";
      public string Version { get; set; }
      public void SetIdAndVersion(string id, string version)
      {
         // validate id.
         var typeName = ContentType;
         if (!id.StartsWith(typeName))
         {
            throw new Exception($"Content type of [{typeName}] cannot use id=[{id}]");
         }

         ContentName = id.Substring(typeName.Length + 1); // +1 for the dot.
         Version = version;
      }


      public static string GetContentType(Type contentType)
      {
         if (_contentTypeNames.ContainsKey(contentType))
         {
            return _contentTypeNames[contentType];
         }
         var attribute = contentType.GetCustomAttribute<ContentTypeAttribute>();
         var typeName = attribute == null ? contentType.Name : attribute.TypeName;
         _contentTypeNames.Add(contentType, typeName);
         return typeName;
      }

      public static string GetContentType<TContent>()
         where TContent : ContentObject
      {
         return GetContentType(typeof(TContent));
      }
   }

}