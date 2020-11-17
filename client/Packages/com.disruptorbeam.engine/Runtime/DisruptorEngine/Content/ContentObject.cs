using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beamable.Common.Content;
using Core.Serialization.SmallerJSON;
using DisruptorBeam.Content.Serialization;
using DisruptorBeam.Content.Validation;
using UnityEngine;

namespace DisruptorBeam.Content
{
   public struct ContentObjectField
   {
      public string Name;
      public Type Type;
      public object Value;
      public Action<object> Setter;
   }

   public delegate void ContentDelegate(ContentObject content);

   [System.Serializable]
   public class ContentObject : ScriptableObject, IContentObject, IRawJsonProvider
   {
      public event ContentDelegate OnChanged;

      [Obsolete]
      public string ContentVersion => Version;

      public string ContentName { get; private set; }

      public string ContentType => GetContentTypeName(GetType());
      public string Id => $"{ContentType}.{ContentName}";
      public string Version { get; private set; }

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

      public ContentObject SetContentName(string name)
      {
         if (ContentName != name || this.name != name)
         {
            ContentName = name;
            this.name = name;
         }
         return this;
      }
      public ContentObject SetContentMetadata(string name, string version)
      {
         ContentName = name;
         Version = version;
         return this;
      }


      public void BroadcastUpdate()
      {
         OnChanged?.Invoke(this);
      }


      /// <summary>
      /// Serialize this content into a json block, containing *ONLY* the properties object.
      /// Ex:
      /// {
      ///   "sample": { "data": 4 },
      ///   "sample2": { "data": { "nested": 5 } }
      /// }
      /// </summary>
      /// <param name="s"></param>
      //      public void Serialize(JsonSerializable.IStreamSerializer s)
      //      {
      //         var props = ContentSerialization.ConvertToDictionary(this);
      //         props.Serialize(s);
      //      }

      /// <summary>
      /// Set all the values of this content object from the given ArrayDict.
      /// This is a modifying action.
      /// </summary>
      /// <param name="dict">An array dict that should only contain the properties json object</param>
      //      public void ApplyProperties(ArrayDict dict)
      //      {
      //         ContentSerialization.ConvertFromDictionary(this, dict);
      //      }

      /// <summary>
      /// Create a new piece of content
      /// </summary>
      /// <param name="name">The name of the content, should be unique</param>
      /// <param name="dict">The property object to set the content values</param>
      /// <typeparam name="TContent">Some type of content</typeparam>
      /// <returns></returns>
      //      public static TContent FromDictionary<TContent>(string name, ArrayDict dict)
      //         where TContent : ContentObject, new()
      //      {
      //         var content = new TContent {ContentName = name};
      //         if (dict == null)
      //            return content;
      //
      //         content.ApplyProperties(dict);
      //         return content;
      //      }


      public static string GetContentTypeName(Type contentType)
      {
         return ContentRegistry.GetContentTypeName(contentType);
      }

      public static string GetContentType<TContent>()
         where TContent : ContentObject
      {
         return GetContentTypeName(typeof(TContent));
      }

      public static TContent Make<TContent>(string name)
         where TContent : ContentObject, new()
      {
         var instance = CreateInstance<TContent>();
         instance.SetContentName(name);
         return instance;
      }


      /// <summary>
      /// Validate this `ContentObject`.
      /// </summary>
      /// <exception cref="AggregateContentValidationException">Should throw if the content is semantically invalid.</exception>
      public virtual void Validate()
      {
         var errors = GetMemberValidationErrors();
         if (errors.Count > 0)
         {
            throw new AggregateContentValidationException(errors);
         }
      }

      public bool HasValidationErrors(out List<string> errors)
      {
         errors = new List<string>();

         if (ContentNameValidationException.HasNameValidationErrors(ContentName, out var nameErrors))
         {
            errors.AddRange(nameErrors.Select(e => e.Message));
         }

         errors.AddRange(GetMemberValidationErrors()
            .Select(e => e.Message));

         return errors.Count > 0;
      }



      public List<ContentValidationException> GetMemberValidationErrors()
      {
         var errors = new List<ContentValidationException>();

         foreach (var field in GetType().GetFields())
         {
            foreach (var attribute in field.GetCustomAttributes<ValidationAttribute>())
            {
               try
               {
                  attribute.Validate(field, this);
               }
               catch (ContentValidationException e)
               {
                  errors.Add(e);
               }
            }
         }

         return errors;
      }

      public string ToJson()
      {
         return ClientContentSerializer.SerializeContent(this);
      }
   }
}