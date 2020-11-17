/*
 *  Simple CustomMicroservice example
 */
//#define DB_MICROSERVICE  // I sometimes enable this to see code better in rider


using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Server.Api;
using Core.Platform.SDK;
using Core.Server.Common;
using ContentService = Beamable.Server.Content.ContentService;
#if DB_MICROSERVICE
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Text.Json;
using DisruptorBeam.Content;
using Newtonsoft.Json;
using Serilog;

namespace Beamable.Server
{

   public class MicroserviceNonceResponse
   {
      public string nonce;
   }

   public class MicroserviceAuthRequest
   {
      public string cid, pid, signature;
   }

   public class MicroserviceAuthResponse
   {
      public string result;
   }

   public class MicroserviceProviderRequest
   {
      public string name, type;
   }

   public class MicroserviceProviderResponse
   {

   }

   public class BeamableMicroService
   {
      public string MicroserviceName => _serviceAttribute.MicroserviceName;
      public string QualifiedName => $"micro_{MicroserviceName}"; // scope everything behind a feature name "micro"

      private bool isStarted = false;

      private Promise<Unit> _serviceInitialized = new Promise<Unit>();
      private EasyWebSocket _webSocket;
      private MicroserviceRequester _requester;
      private SocketRequesterContext _socketRequesterContext;
      private ServiceMethodCollection _serviceMethods;
      private MicroserviceAttribute _serviceAttribute;

      // TODO: XXX This is gross. Eventually, I'd like to have an IContentService get injected into a service provider, that the ContentRef can use
      public static ContentService _contentService;

      private IMicroserviceArgs _args;
      private ServiceFactory<Microservice> _serviceFactory;
      private string CustomerID => _args.CustomerID;
      private string ProjectName => _args.ProjectName;
      private string Secret => _args.Secret;
      private string Host => _args.Host;

      public void Start<TMicroService>(ServiceFactory<TMicroService> factory, IMicroserviceArgs args)
         where TMicroService : Microservice
      {
         _serviceFactory = factory;

         var t = typeof(TMicroService);
         _serviceAttribute = t.GetCustomAttribute<MicroserviceAttribute>();
         if (_serviceAttribute == null)
         {
            throw new Exception($"Cannot create service. Missing [{typeof(MicroserviceAttribute).Name}].");
         }

         _args = args.Copy();
         Log.Debug("Starting... {args}", args);

         if (isStarted)
            return;
         isStarted = true; // todo: this needs to be smarter

         _serviceMethods = ServiceMethodHelper.Scan<TMicroService>();
         _webSocket = EasyWebSocket.Create(Host);
         _socketRequesterContext = new SocketRequesterContext(_webSocket);
         _requester = new MicroserviceRequester(null, _socketRequesterContext);

         _contentService = new ContentService(_requester, _socketRequesterContext);


         // Call backs
         _webSocket.OnConnect(AuthorizeConnection);
         _webSocket.OnDisconnect(CloseConnection);
         _webSocket.OnMessage(HandleWebsocketMessage);

         _serviceInitialized.Then(_ =>
         {
            _contentService.Init();
         });

         // Connect and Run
         _webSocket.Connect();

         CancellationTokenSource cancelSource = new CancellationTokenSource();
         cancelSource.Token.WaitHandle.WaitOne();
      }

      async void HandleWebsocketMessage(EasyWebSocket ws, string msg)
      {

         var ctx = msg.BuildWebRequest(_args);

         var reqLog = Log.ForContext("requestContext", ctx, true);
         BeamableSerilogProvider.LogContext.Value = reqLog;

         reqLog.Debug("Started Handling WS");


         if (_socketRequesterContext.IsPlatformMessage(ctx))
         {
            // the request is a platform request.
            try
            {
               _socketRequesterContext.HandleMessage(ctx, msg);
            }
            catch(Exception ex)
            {

               Console.WriteLine(
                  $"Exception {ex.GetType().Name}: {ex.Message} - {ex.Source} \n {ex.StackTrace}");
            }

            return;
         }

         // this is a client request. Handle the service method.
         try
         {
            var argStrings = new List<string>();
            using (var bodyDoc = JsonDocument.Parse(ctx.Body))
            {
               // extract the payload object.
               if (bodyDoc.RootElement.TryGetProperty("payload", out var payloadString))
               {
                  using (var payloadDoc = JsonDocument.Parse(payloadString.ToString()))
                  {
                     foreach (var argJson in payloadDoc.RootElement.EnumerateArray())
                     {
                        argStrings.Add(argJson.GetRawText());
                     }
                  }
               }
            }

            var route = ctx.Path.Substring(QualifiedName.Length + 1);

            var service = _serviceFactory.Invoke();
            service.ProvideContext(ctx);
            var requester = new MicroserviceRequester(ctx, _socketRequesterContext);
            service.ProvideRequester(requester);
            service.ProvideServices(GenerateSdks(requester, ctx));

            var result = await _serviceMethods.Handle(service, route, argStrings.ToArray());

            var response = new GatewayResponse
            {
               id = ctx.Id,
               status = 200,
               body = new ClientResponse
               {
                  payload = result
               }
            };
            var responseJson = JsonConvert.SerializeObject(response);
            reqLog.Debug("Responding with {json}", responseJson);
            ws.SendMessage(responseJson);
         }
         catch (TargetInvocationException ex)
         {
            var inner = ex.InnerException;
            Console.WriteLine(
               $"Exception {inner.GetType().Name}: {inner.Message} - {inner.Source} \n {inner.StackTrace}");
            var failResponse = new GatewayResponse
            {
               id = ctx.Id,
               status = 500,
               body = new ClientResponse
               {
                  payload = ""
               }
            };
            var failResponseJson = JsonConvert.SerializeObject(failResponse);
            ws.SendMessage(failResponseJson);
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Exception {ex.GetType().Name}: {ex.Message} - {ex.Source} \n {ex.StackTrace}");
            var failResponse = new GatewayResponse
            {
               id = ctx.Id,
               status = 500,
               body = new ClientResponse
               {
                  payload = ""
               }
            };
            var failResponseJson = JsonConvert.SerializeObject(failResponse);
            ws.SendMessage(failResponseJson);
         }

      }

      private IBeamableServices GenerateSdks(MicroserviceRequester requester, RequestContext ctx)
      {
         var services = new BeamableServices
         {
            Auth = new ServerAuthApi(requester, ctx),
            Stats = new MicroserviceStatsApi(requester, ctx),
            Content = _contentService,
            Inventory = new MicroserviceInventoryApi(requester, ctx)
         };

         return services;
      }


      private void CloseConnection(EasyWebSocket ws)
      {
         Log.Debug("Closing socket connection...");
      }

      private void AuthorizeConnection(EasyWebSocket ws)
      {
         Log.Debug("Authorizing WS connection");
         _requester.Request<MicroserviceNonceResponse>(Method.GET, "gateway/nonce").Then(res =>
         {
            Log.Debug("Got nonce");
            var sig = CalculateSignature(Secret + res.nonce);
            var req = new MicroserviceAuthRequest
            {
               cid = CustomerID,
               pid = ProjectName,
               signature = sig
            };
            _requester.Request<MicroserviceAuthResponse>(Method.POST, "gateway/auth", req).Then(res =>
            {
               if (!string.Equals("ok", res.result)) return;

               Log.Debug("Authorization complete");
               ProvideService(QualifiedName).Then(_ =>
               {
                  Log.Information("Service ready");
                  _serviceInitialized.CompleteSuccess(PromiseBase.Unit);
               });
            });
         });

      }

      private string CalculateSignature(string text)
      {
         System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
         byte[] data = Encoding.UTF8.GetBytes(text);
         byte[] hash = md5.ComputeHash(data);
         return Convert.ToBase64String(hash);
      }

      private Promise<Unit> ProvideService(string name)
      {
         var req = new MicroserviceProviderRequest
         {
            type = "basic",
            name = name
         };
         var serviceProvider = _requester.Request<MicroserviceProviderResponse>(Method.POST, "gateway/provider", req).Then(res =>
         {
            Log.Debug("Service provider initialized");
         }).ToUnit();
         var eventProvider = _requester.InitializeSubscription().Then(res =>
         {
            Log.Debug("Event provider initialized");
         }).ToUnit();
         return Promise.Sequence(serviceProvider, eventProvider).ToUnit();
      }


   }
}
#endif