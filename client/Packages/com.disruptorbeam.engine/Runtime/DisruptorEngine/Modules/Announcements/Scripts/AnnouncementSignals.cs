using System;

using System.Collections;

using System.Collections.Generic;

using System.Linq;

using System.Linq.Expressions;

using System.Security.Authentication;

using Core.Coroutines;

using Core.Platform.SDK;

using Core.Platform.SDK.Auth;

using DisruptorBeam.Signals;

using DisruptorBeam.UI;

using DisruptorBeam.UI.Scripts;

using TMPro;

using UnityEngine;

using UnityEngine.Events;



namespace DisruptorBeam.Modules.Announcements

{

   [System.Serializable]

   public class ToggleEvent : DeSignal<bool>

   {



   }

   [HelpURL(ContentConstants.URL_FEATURE_ANNOUNCEMENTS_FLOW)]
   public class AnnouncementSignals : DeSignalTower

   {

      [Header("Flow Events")]

      public ToggleEvent OnToggleAnnouncement;


      private static bool _toggleState;


      public static bool ToggleState => _toggleState;


      private void Broadcast<TArg>(TArg arg, Func<AnnouncementSignals, DeSignal<TArg>> getter)

      {

         this.BroadcastSignal(arg, getter);

      }


      public void ToggleAnnouncement()

      {

         _toggleState = !_toggleState;

         Broadcast(_toggleState, s => s.OnToggleAnnouncement);
     
      }

      public void ToggleAnnouncement(bool desiredState)

      {

         if (desiredState == ToggleState) return;



         _toggleState = desiredState;

         Broadcast(_toggleState, s => s.OnToggleAnnouncement);

      }
   }
}

