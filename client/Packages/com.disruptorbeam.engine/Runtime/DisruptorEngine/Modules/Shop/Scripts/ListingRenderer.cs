using Core.Platform.SDK.Payments;
using UnityEngine;

namespace DisruptorBeam.Modules.Shop
{
   public abstract class ListingRenderer : MonoBehaviour
   {
      public abstract void RenderListing(PlayerStoreView store, PlayerListingView listing);
   }
}