using Beamable.Samples.BBB.Views;
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
      private BeamContext _beamContext;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _introUI.AboutBodyText = "";
         SetupBeamable();
      }

      protected async void OnDestroy()
      {
         _beamContext.Api.ConnectivityService.OnConnectivityChanged -= ConnectivityService_OnConnectivityChanged;
         await _beamContext.ClearPlayerAndStop();
      }

      //  Other Methods --------------------------------

      /// <summary>
      /// Login with Beamable and fetch user/session information
      /// </summary>
      private async void SetupBeamable()
      {
         try
         {
            // Attempt Connection to Beamable
            _beamContext = BeamContext.Default;
            await _beamContext.OnReady;
            // Fetch user information
            _dbid = _beamContext.PlayerId;
            _isBeamableSDKInstalled = true;

            // Handle any changes to the internet connectivity
            _beamContext.Api.ConnectivityService.OnConnectivityChanged += ConnectivityService_OnConnectivityChanged;
            ConnectivityService_OnConnectivityChanged(_beamContext.Api.ConnectivityService.HasConnectivity);
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