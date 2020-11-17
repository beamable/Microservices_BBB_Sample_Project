using DisruptorBeam.UI.Buss;
using UnityEngine;

namespace DisruptorBeam.Editor.UI.Buss.Extensions
{
   public class StyleBehaviourExtensions
   {
      public static void Refresh()
      {

         foreach (var behaviour in GameObject.FindObjectsOfType<StyleBehaviour>())
         {
            if (!behaviour || !behaviour.isActiveAndEnabled && !behaviour.gameObject.activeInHierarchy)
            {
               continue;
            }

            behaviour.ApplyStyleTree();
         }


      }
   }
}