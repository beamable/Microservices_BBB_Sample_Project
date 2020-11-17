using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Core.Platform.SDK;
using Core.Serialization.SmallerJSON;
using UnityEngine;

namespace DisruptorBeam.Content
{
   /// <summary>
   /// This is the runtime content stuff.
   /// It will DOWNLOAD content...
   /// </summary>
   public class ContentService : PlatformSubscribable<ClientManifest, ClientManifest>
   {
      private ClientManifest _latestManifiest;

      private Promise<Unit> _manifestPromise = new Promise<Unit>();
      private Dictionary<string, ClientContentInfo> _contentIdTable = new Dictionary<string, ClientContentInfo>();
      private Dictionary<Type, ContentCache> _contentCaches = new Dictionary<Type, ContentCache>();

      public ContentService(PlatformService platform, PlatformRequester requester) : base(platform, requester, "content")
      {
         Subscribe(cb =>
         {
            // pay attention, server...
         });
      }

      public Promise<TContent> GetContent<TContent>(IContentRef<TContent> reference)
         where TContent : ContentObject, new()
      {
         return GetContent<TContent>(reference as IContentRef);
      }

      public Promise<IContentObject> GetContent(string contentId, Type contentType)
      {

#if UNITY_EDITOR
         if (!UnityEditor.EditorApplication.isPlaying)
         {
            Debug.LogError("Cannot resolve content at edit time!");
            throw new Exception("Cannot resolve content at edit time!");
         }
#endif
         ContentCache rawCache;

         if (!_contentCaches.TryGetValue(contentType, out rawCache))
         {
            var cacheType = typeof(ContentCache<>).MakeGenericType(contentType);
            var constructor = cacheType.GetConstructor(new[] { typeof(IPlatformRequester) });
            rawCache = (ContentCache)constructor.Invoke(new[] {requester});

            _contentCaches.Add(contentType, rawCache);
         }


         return WaitForManifest().FlatMap(manifest =>
         {
            if (!_contentIdTable.ContainsKey(contentId))
            {
               return Promise<IContentObject>.Failed(new Exception($"Content not found, currently have: '{contentId}' of type {contentType.Name}"));
            }
            return rawCache.GetContentObject(_contentIdTable[contentId]);
         });

      }

      public Promise<IContentObject> GetContent(string contentId)
      {
         var referencedType = ContentRegistry.GetTypeFromId(contentId);
         return GetContent(contentId, referencedType);
      }

      public Promise<TContent> GetContent<TContent>(IContentRef reference)
         where TContent : ContentObject, new()
      {
         var referencedType = reference.GetReferencedType();
         return GetContent(reference.GetId(), referencedType).Map( c => (TContent)c);
      }

      Promise<ClientManifest> WaitForManifest()
      {
         return _manifestPromise.Map(x => _latestManifiest);
      }

      protected override string CreateRefreshUrl(string scope)
      {
         return "/basic/content/manifest/public?id=global";
      }

      protected override Promise<ClientManifest> ExecuteRequest(PlatformRequester requester, string url)
      {
         return requester.Request(Method.GET, url, null, true, ClientManifest.ParseCSV, useCache:true);
      }

      protected override void OnRefresh(ClientManifest data)
      {
         Notify(data);
         _latestManifiest = data;

         // TODO work on better refresh strategy. A total wipe isn't very performant.
         _contentIdTable.Clear();
         foreach (var entry in _latestManifiest.entries)
         {
            _contentIdTable.Add(entry.contentId, entry);
         }

         _manifestPromise.CompleteSuccess(new Unit());
      }


   }
}