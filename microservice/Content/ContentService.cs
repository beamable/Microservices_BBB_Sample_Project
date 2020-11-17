using System;
using System.Collections.Generic;
using System.Net.Http;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Core.Platform.SDK;
using DisruptorBeam.Content;

namespace Beamable.Server.Content
{
   public class ContentManifest
   {
      public string id;
      public long created;
      public List<ContentReference> references;
   }


   public class ContentReference
   {

      public string id;
      public string version;
      public string uri;
      public string checksum;
      public string type;
   }

   public struct ContentKey
   {
      public string Id, Version;

      public override bool Equals(object obj)
      {
         if (obj is ContentKey other)
         {
            return string.Equals(other.Id, Id) && string.Equals(other.Version, Version);
         }
         else
         {
            return false;
         }
      }

      public override int GetHashCode()
      {
         return 17 + 31 * Id.GetHashCode() + 31 * Version.GetHashCode();
      }
   }

   public class ContentService : IContentService
   {
      private readonly MicroserviceRequester _requester;
      private readonly SocketRequesterContext _socket;

      private Dictionary<string, ContentReference> _idToUri = new Dictionary<string, ContentReference>();

      private Promise<Unit> _waitForManifest = new Promise<Unit>();
      private MicroserviceContentSerializer _serializer = new MicroserviceContentSerializer();

      public ContentService(MicroserviceRequester requester, SocketRequesterContext socket)
      {
         _requester = requester;
         _socket = socket;
      }

      public void Init()
      {

         _socket.Subscribe<ContentManifestEvent>("content.manifest", HandleContentPublish);
         RefreshManifest();
      }

      void HandleContentPublish(ContentManifestEvent manifestEvent)
      {
         // the event data isn't super useful on its own, so just use it to know that our cache is invalid.
         RefreshManifest(); // TODO: Maybe this should get debounced? If a flood of events started pouring in for whatever reason, this could kill us.
      }

      void RefreshManifest()
      {
         // reset the manifest promise, so that if a content request comes while a manifest is being retrieved, we wait for the updated content to be served
         _waitForManifest = _requester.Request<ContentManifest>(Method.GET, "/basic/content/manifest")
            .Map(manifest =>
         {
            _idToUri.Clear();
            foreach (var reference in manifest.references)
            {
               // TODO how do we separate the private from the public?
               if (!_idToUri.ContainsKey(reference.id))
               {
                  _idToUri.Add(reference.id, reference);
               }
            }

            return PromiseBase.Unit;
         });
      }

      public Promise<TContent> Resolve<TContent>(IContentRef<TContent> reference) where TContent : IContentObject, new()
      {
         return _waitForManifest.FlatMap(_ =>
         {
            var id = reference.GetId();
            if (!_idToUri.TryGetValue(id, out var content))
            {
               throw new Exception($"No content exists for {id}");
            };

            var requestPromise = new Promise<string>();
            RequestContent(content.uri, requestPromise);
            return requestPromise.Map(ParseContent<TContent>);
         });

      }

      TContent ParseContent<TContent>(string json) where TContent : IContentObject, new()
      {
         return _serializer.Deserialize<TContent>(json);
      }

//      TContent ParseContent<TContent>(string json) where TContent : IContentObject, new()
//      {
//         var fields = typeof(TContent)
//            .GetFields(BindingFlags.Public | BindingFlags.Instance)
//            .ToImmutableDictionary(f => f.Name);
//
//         var instance = Activator.CreateInstance<TContent>();
//
//         using (JsonDocument document = JsonDocument.Parse(json))
//         {
//            var id = document.RootElement.GetProperty("id").GetString();
//            var version = document.RootElement.GetProperty("version").GetString();
//            var properties = document.RootElement.GetProperty("properties").EnumerateObject();
//
//            foreach (var property in properties)
//            {
//               // find associated field in content object...
//               if (!fields.TryGetValue(property.Name, out var field))
//               {
//                  continue; // there is no field for this property... Don't do anything.
//               }
//
//               // and parse the value into the field...
//               if (property.Value.TryGetProperty("data", out var dataBlock))
//               {
//                  // need to deserialize the value of the data...
//                  var raw = dataBlock.GetRawText();
//                  var value = JsonConvert.DeserializeObject(raw, field.FieldType);
//                  field.SetValue(instance, value);
//               }
//
//            }
//         }
//
//         return instance;
//      }

      async void RequestContent(string uri, Promise<string> promise)
      {
         // TODO: Add some sort of caching layer
         using var client = new HttpClient();
         var result = await client.GetStringAsync(uri);
         Console.WriteLine($"Content json for uri=[{uri}] json=[{result}]");
         promise.CompleteSuccess(result);
      }

   }
}