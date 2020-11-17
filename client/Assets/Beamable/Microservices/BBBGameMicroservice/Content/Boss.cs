using DisruptorBeam.Content;

namespace Beamable.Server.BBBGameMicroservice.Content
{
   /// <summary>
   /// The <see cref="ContentObject"/> which defines the values that can be set
   /// on the client side for ease-of-use or on the server side via portal for live-ops.
   /// </summary>
   [ContentType("boss")]
   public class Boss : ContentObject
   {
      //  Fields ---------------------------------------
      public int MaxHealth = 100;
   }

   /// <summary>
   /// Unity-friendly serialization for content in the Unity Inspector
   /// </summary>
   [System.Serializable]
   public class BossContentRef : ContentRef<Boss> { }
}