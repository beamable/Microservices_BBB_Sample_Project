using System.Collections.Generic;
using Core.Platform.SDK;

namespace Beamable.Server.Editor.ManagerClient
{
   public class MicroserviceManager
   {
      public const string SERVICE = "/basic/servicemanager";

      public PlatformRequester Requester { get; }

      public MicroserviceManager(PlatformRequester requester)
      {
         Requester = requester;
      }

      public Promise<ServiceManifest> GetCurrentManifest()
      {
         return Requester.Request<GetManifestResponse>(Method.GET, $"{SERVICE}/manifest/current", "{}")
            .Map(res => res.manifest)
            .Recover(ex =>
            {
               if (ex is PlatformRequesterException platformEx && platformEx.Status == 404)
               {
                  return new ServiceManifest();
               }

               throw ex;
            });
      }

      public Promise<ServiceManifest> GetManifest(long id)
      {
         return Requester.Request<GetManifestResponse>(Method.GET, $"{SERVICE}/manifest?id={id}")
         .Map(res => res.manifest);
      }

      public Promise<List<ServiceManifest>> GetManifests()
      {
         return Requester.Request<GetManifestsResponse>(Method.GET, $"{SERVICE}/manifests")
            .Map(res => res.manifests);
      }

      public Promise<Unit> Deploy(ServiceManifest manifest)
      {
         return Requester.Request<EmptyResponse>(Method.POST, $"{SERVICE}/manifest", new PostManifestRequest
         {
            manifest = manifest.manifest
         }).ToUnit();
      }

      public Promise<GetStatusResponse> GetStatus()
      {
         return Requester.Request<GetStatusResponse>(Method.GET, $"{SERVICE}/status");
      }

   }

   [System.Serializable]
   public class GetManifestResponse
   {
      public ServiceManifest manifest;
   }
   [System.Serializable]
   public class GetManifestsResponse
   {
      public List<ServiceManifest> manifests;
   }
   [System.Serializable]
   public class PostManifestRequest
   {
      public List<ServiceReference> manifest;
   }

   [System.Serializable]
   public class ServiceManifest
   {
      public long id;
      public long created;
      public List<ServiceReference> manifest = new List<ServiceReference>();
      public long createdByAccountId;
      public string comments;
   }

   [System.Serializable]
   public class ServiceReference
   {
      public string serviceName;
      public string checksum;
      public bool enabled;
      public string imageId;
      public string templateId;
      public string comments;
   }

   [System.Serializable]
   public class GetStatusResponse
   {
      public bool isCurrent;
      public List<ServiceStatus> services;
   }

   [System.Serializable]
   public class ServiceStatus
   {
      public string serviceName;
      public string imageId;
      public bool running;
      public bool isCurrent;
   }

//   case class GetStatusResponse(
//      services: Seq[ServiceStatus],
//   isCurrent: Boolean // this is the magic field that says, "Are all the services where they're supposed to be as declared in the manifest?"
//   ) extends NetworkSerializable
//
//   case class ServiceStatus(
//      serviceName: String,
//      running: Boolean, // this probably needs to be an enum of STARTING, STOPPING, RUNNING
//      imageId: String, // this is the image of the task def
//      isCurrent: Boolean // this is the magic field that says, "Is this service state what its supposed to be in the manifest?"
//   )

}