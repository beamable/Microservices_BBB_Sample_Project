using DisruptorBeam.Content;

namespace Beamable.Server.BBBGameMicroservice.Content
{
   /// <summary>
   /// The <see cref="ContentObject"/> which defines the values that can be set
   /// on the client side for ease-of-use or on the server side via portal for live-ops.
   /// </summary>
   [ContentType("weapon")]
   public class Weapon : ContentObject
   {
      //  Fields ---------------------------------------
      public float HitChance = 0.5f;
      public int MinDamage = 25;
      public int MaxDamage = 50;
   }

   /// <summary>
   /// Unity-friendly serialization for content in the Unity Inspector
   /// </summary>
   [System.Serializable]
   public class WeaponContentRef : ContentRef<Weapon> { }
}