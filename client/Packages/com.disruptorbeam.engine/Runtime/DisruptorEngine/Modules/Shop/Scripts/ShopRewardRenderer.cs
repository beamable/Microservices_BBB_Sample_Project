using Core.Platform.SDK.Payments;
using DisruptorBeam.Signals;
using DisruptorBeam.UI.Scripts;
using UnityEngine;

namespace DisruptorBeam.Modules.Shop
{
   public class ShopRewardRenderer : MenuBase
   {
      public ObtainRenderer ObtainRenderer;
      public GameObject Frame;

      public PlayerListingView Listing;

      void Start()
      {
         Frame.SetActive(false);
      }

      public override void OnOpened()
      {
         Frame.SetActive(true);

         ObtainRenderer.RenderObtain(Listing);
      }
   }
}