
using Beamable.Common.Content;
using Beamable.Common.Inventory.Api;
using Beamable.Server.Api;

namespace Beamable.Server
{
   public class BeamableServices : IBeamableServices
   {
      public IMicroserviceAuthApi Auth { get; set; }
      public IMicroserviceStatsApi Stats { get; set; }
      public IContentService Content { get; set; }
      public IInventoryApi Inventory { get; set; }
   }
}