using Beamable.Server;

namespace DisruptorBeam.Content
{
   public class ContentService : Beamable.Server.Content.ContentService
   {
      public ContentService(MicroserviceRequester requester, SocketRequesterContext socket) : base(requester, socket)
      {
      }
   }
}