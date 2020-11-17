using DisruptorBeam.Editor;

namespace Beamable.Server.Editor.ManagerClient
{
   public static class BeamableExtensions
   {
      private static MicroserviceManager _manager;

      public static MicroserviceManager GetMicroserviceManager(this EditorDisruptorEngine de)
      {
         return (_manager ?? (_manager = new MicroserviceManager(de.Requester)));
      }
   }
}