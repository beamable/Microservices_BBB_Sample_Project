using Core.Platform.SDK.Payments;
using UnityEngine;

namespace DisruptorBeam.Modules.Shop
{
   public abstract class ObtainRenderer : MonoBehaviour
   {
      public abstract void RenderObtain(PlayerListingView data);
   }
}