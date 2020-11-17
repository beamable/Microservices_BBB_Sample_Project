
using Beamable.Common.Content;
using Beamable.Common.Inventory.Api;
using Beamable.Server.Api;

namespace Beamable.Server
{
   public interface IBeamableServices
   {
      IMicroserviceAuthApi Auth { get; }
      IMicroserviceStatsApi Stats { get; }
      IContentService Content { get; }
      IInventoryApi Inventory { get; }
   }
}