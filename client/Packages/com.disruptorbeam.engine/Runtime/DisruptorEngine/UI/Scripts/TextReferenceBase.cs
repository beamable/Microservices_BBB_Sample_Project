using TMPro;
using UnityEngine;

namespace DisruptorBeam.UI.Scripts
{
   public abstract class TextReferenceBase : MonoBehaviour
   {
      public abstract string Value { get; set; }

      public void SetValueFromReference(TextReferenceBase reference)
      {
         Value = reference.Value;
      }
   }
}
