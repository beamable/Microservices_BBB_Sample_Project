using System.Collections;
using System.Collections.Generic;
using DisruptorBeam.UI.Scripts;
using UnityEngine;

namespace DisruptorBeam.UI.Scripts
{
   public class BackButtonBehaviour : MonoBehaviour
   {
      public MenuManagementBehaviour MenuManager;
      
      public void GoBack()
      {
         MenuManager.GoBack();
      }
   }
}