using DisruptorBeam.Content;
using UnityEngine;

namespace DisruptorBeam.Editor.Content
{
   [System.Serializable]
   public class SerializedContentObject<TContent> : ScriptableObject
      where TContent : ContentObject, new()
   {
      public TContent Content;
   }
}