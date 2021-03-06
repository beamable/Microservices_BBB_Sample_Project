﻿using Beamable.Samples.BBB.Views;
using System;
using UnityEngine;

namespace Beamable.Samples.BBB
{
   /// <summary>
   /// Handles the intro menu scene logic.
   /// </summary>
   public class BBBIntroManager : MonoBehaviour
   {

      //  Fields ---------------------------------------
      [SerializeField]
      private IntroUI _introUI = null;

      private long _dbid = 0;
      private bool _isConnected = false;
      private bool _isBeamableSDKInstalled = false;
      private string _isBeamableSDKInstalledErrorMessage = "";

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _introUI.AboutBodyText = "";
         SetupBeamable();
      }

      protected void OnDestroy()
      {
         Beamable.API.Instance.Then(beamableAPI =>
         {
            beamableAPI.ConnectivityService.OnConnectivityChanged -= ConnectivityService_OnConnectivityChanged;
         });
      }

      //  Other Methods --------------------------------

      /// <summary>
      /// Login with Beamable and fetch user/session information
      /// </summary>
      private void SetupBeamable()
      {
         try
         {
            // Attempt Connection to Beamable
            Beamable.API.Instance.Then(beamableAPI =>
            {
               // Fetch user information
               _dbid = beamableAPI.User.id;
               _isBeamableSDKInstalled = true;

               // Handle any changes to the internet connectivity
               beamableAPI.ConnectivityService.OnConnectivityChanged += ConnectivityService_OnConnectivityChanged;
               ConnectivityService_OnConnectivityChanged(beamableAPI.ConnectivityService.HasConnectivity);

            });
         }
         catch (Exception e)
         {
            _isBeamableSDKInstalledErrorMessage = e.Message;
            ConnectivityService_OnConnectivityChanged(false);
         }
      }

      /// <summary>
      /// Render the user-facing text with success or helpful errors.
      /// </summary>
      private void RenderUI()
      {
         string aboutBodyText = BBBHelper.GetIntroAboutBodyText(
            _isConnected, 
            _dbid, 
            _isBeamableSDKInstalled, 
            _isBeamableSDKInstalledErrorMessage);

         _introUI.AboutBodyText = aboutBodyText;
         _introUI.MenuCanvasGroup.interactable = _isConnected;
      }

      //  Event Handlers -------------------------------
      private void ConnectivityService_OnConnectivityChanged(bool isConnected)
      {
         _isConnected = isConnected;

         // Show some helpful debugging info
         //Debug.Log($"ConnectivityService_OnConnectivityChanged()...");
         //Debug.Log($"_dbid = {_dbid}. ");
         //Debug.Log($"_isConnected = {_isConnected}. ");
         //Debug.Log($"_isBeamableSDKInstalled = {_isBeamableSDKInstalled}. ");

         //if (!string.IsNullOrEmpty(_isBeamableSDKInstalledErrorMessage))
         //{
         //   Debug.Log($"_isBeamableSDKInstalledErrorMessage = {_isBeamableSDKInstalledErrorMessage}. ");
         //}

         RenderUI();
      }
   }
}