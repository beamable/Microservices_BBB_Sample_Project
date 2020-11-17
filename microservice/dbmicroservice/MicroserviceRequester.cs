using System;
using System.Collections.Generic;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Core.Platform.SDK;
using Core.Server.Common;
using Newtonsoft.Json;

namespace Beamable.Server
{
   public class WebsocketRequest
   {
      public long id;
      public string method;
      public string path;
      public object body;
      public long? from;
   }

   public interface IPlatformSubscription
   {
      void Resolve(RequestContext ctx);
   }
   public class PlatformSubscription<T> : IPlatformSubscription
   {
      public string EventName;
      public Action Unsubscribe;
      public Action<T> OnEvent;

      public void Resolve(RequestContext ctx)
      {
         var data = JsonConvert.DeserializeObject<T>(ctx.Body);
         OnEvent?.Invoke(data);
      }
   }

   public class WebsocketErrorResponse
   {
      public int status;
      public string service;
      public string error;
      public string message;

      public override string ToString()
      {
         return $"Websocket Error Response. status=[{status}] service=[{service}] error=[{error}] message=[{message}]";
      }
   }

   public class RequesterException : Exception
   {
      public string Uri { get; }
      public long Id { get; }
      public WebsocketErrorResponse Error;

      public RequesterException(WebsocketErrorResponse err, string uri, long id) : base("Requester error. ")
      {
         Uri = uri;
         Id = id;
         Error = err;
      }
   }


   public interface IWebsocketResponseListener
   {
      long Id { get; set; }
      void Resolve(RequestContext ctx);
   }
   public class WebsocketResponseListener<T> : IWebsocketResponseListener
   {
      public Promise<T> OnDone;
      public long Id { get; set; }
      public string Uri { get; set; }

      public Func<string, T> Parser { get; set; }

      public void Resolve(RequestContext ctx)
      {
         // TODO: handle error response?

         if (ctx.Status != 200)
         {
            Console.Error.WriteLine($"{Uri} yield bad [{ctx.Body}]");
            var errorPayload = JsonConvert.DeserializeObject<WebsocketErrorResponse>(ctx.Body);
            Console.Error.WriteLine(errorPayload);
            OnDone.CompleteError(new RequesterException(errorPayload, Uri, Id));
            return;
         }

         // parse out the data from ctx.
         if (Parser == null)
         {
            Parser = JsonConvert.DeserializeObject<T>;
         }

         var result = Parser(ctx.Body);
         OnDone.CompleteSuccess(result);
      }
   }

   public class SocketRequesterContext
   {
      public EasyWebSocket Socket { get; private set; }

      private Dictionary<long, IWebsocketResponseListener> _pendingMessages = new Dictionary<long, IWebsocketResponseListener>();
      private Dictionary<string, IList<IPlatformSubscription>> _subscriptions = new Dictionary<string, IList<IPlatformSubscription>>();
      private long _lastRequestId = 1;

      public SocketRequesterContext(EasyWebSocket socket)
      {
         Socket = socket;
      }

      public long GetNextRequestId()
      {
         lock (this)
         {
            var curr = _lastRequestId;
            _lastRequestId++;
            return curr;
         }
      }

      public PlatformSubscription<T> Subscribe<T>(string eventName, Action<T> callback)
      {
         var subscription = new PlatformSubscription<T>
         {
            EventName = eventName
         };
         subscription.OnEvent += callback;

         if (!_subscriptions.ContainsKey(eventName))
         {
            _subscriptions.Add(eventName, new List<IPlatformSubscription>());
         }

         var subscriptionList = _subscriptions[eventName];
         subscriptionList.Add(subscription);
         var unsub = new Action(() =>
         {
            subscriptionList.Remove(subscription);
         });
         subscription.Unsubscribe = unsub;

         return subscription;
      }

      private bool TryGetEventSubscriptions(string eventName, out IList<IPlatformSubscription> subscriptions)
      {
         return _subscriptions.TryGetValue(eventName, out subscriptions);
      }

      public bool IsPlatformMessage(RequestContext ctx)
      {
         return string.IsNullOrEmpty(ctx.Path) || ctx.Path.StartsWith("event/");
      }

      public void HandleMessage(RequestContext ctx, string msg)
      {
         var isEvent = ctx.Path?.StartsWith("event/") ?? false;
         if (isEvent)
         {
            // the platform is notifying us of an event...
            lock (this)
            {
               // the microservice also manages a set of request ids, and increments them on event sends.
               //  we need to sync the client request id to match. XXX feels strange.
               _lastRequestId = ctx.Id;
            }

            var eventName = ctx.Path.Substring("event/".Length);
            if (TryGetEventSubscriptions(eventName, out var subscriptions))
            {
               foreach (var subscription in subscriptions)
               {
                  subscription.Resolve(ctx);
               }
            }

            //</color> <color=blue>BaseGet [{"id":1,"method":"post","path":"event/content.manifest","body":{"categories":["tournaments","announcements","listings","items","stores","sagamap","skus","currency","leaderboards","emails","game_types"]}}

         } else if (TryGetListener(ctx.Id, out var listener))
         {
            // this is a response to some pending request...
            try
            {
               listener.Resolve(ctx);
            }
            finally
            {
               Remove(ctx.Id);
            }
         }
         else
         {
            Console.Error.WriteLine($"There was no listener for request id=[{ctx.Id}]");
         }
      }

      public Promise<T> AddListener<T>(WebsocketRequest req, string uri, Func<string, T> parser)
      {
         var requestId = GetNextRequestId();
         if (_pendingMessages.ContainsKey(requestId))
         {
            BeamableSerilogProvider.Instance.Debug("The request {id} was already taken", requestId);
            return AddListener(req, uri, parser); // try again.
         }

         var promise = new Promise<T>();

         req.id = requestId;
         var listener = new WebsocketResponseListener<T>
         {
            Id = requestId,
            OnDone = promise,
            Parser = parser,
            Uri = uri,
         };

         _pendingMessages.Add(requestId, listener);
         return promise;
      }

      public bool TryGetListener(long id, out IWebsocketResponseListener listener)
      {
         return _pendingMessages.TryGetValue(id, out listener);
      }

      public void Remove(long id)
      {
         _pendingMessages.Remove(id);
      }


   }

   public class MicroserviceRequester : IBeamableRequester
   {
      protected RequestContext _requestContext;

      private readonly SocketRequesterContext _socketContext;

      // TODO how do we handle Timeout errors?
      // TODO what does concurrency look like?

      public MicroserviceRequester(RequestContext requestContext, SocketRequesterContext socketContext)
      {
         _requestContext = requestContext;
         _socketContext = socketContext;
      }

      public Promise<T> Request<T>(Method method, string uri, object body = null, bool includeAuthHeader = true, Func<string, T> parser = null,
         bool useCache = false)
      {
         // TODO: What do we do about includeAuthHeader?
         // TODO: What do we do about useCache?

         // peel off the first slash of the uri, because socket paths are not relative, they are absolute. // TODO: xxx gross.
         if (uri.StartsWith('/'))
         {
            uri = uri.Substring(1);
         }

         if (body == null)
         {
            body = new { }; // empty object.
         }

         var req = new WebsocketRequest
         {
            method = method.ToString().ToLower(),
            body = body,
            path = uri,
         };
         if (_requestContext != null && includeAuthHeader)
         {
            req.from = _requestContext.UserId;
         }

         var promise = _socketContext.AddListener(req, uri, parser);
         var msg = JsonConvert.SerializeObject(req);

         BeamableSerilogProvider.Instance.Debug("sending request {msg}", msg);
         _socketContext.Socket.SendMessage(msg);

         return promise;
      }

      /// <summary>
      /// Each socket only needs to set up one subscription to the server.
      /// All events will get piped to the client.
      /// It's the client job to filter the events, and decide what is valuable.
      /// </summary>
      /// <returns></returns>
      public Promise<EmptyResponse> InitializeSubscription()
      {
         var req = new MicroserviceProviderRequest
         {
            type = "event"
         };
         var promise = Request<MicroserviceProviderResponse>(Method.POST, "gateway/provider", req);

         return promise.Map(_ => new EmptyResponse());
      }

      public IBeamableRequester WithAccessToken(TokenResponse tokenResponse)
      {
         throw new NotImplementedException();
      }

      public string EscapeURL(string url)
      {
         return System.Web.HttpUtility.UrlEncode(url);
      }
   }
}