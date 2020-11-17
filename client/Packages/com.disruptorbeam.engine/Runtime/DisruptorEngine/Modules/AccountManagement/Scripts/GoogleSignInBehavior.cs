using Core.Config;
using Core.Platform.SDK.Auth;
using UnityEngine;

namespace DisruptorBeam.Modules.AccountManagement
{
   public class GoogleSignInBehavior : MonoBehaviour
   {
      private GoogleSignIn _google;
      private ThirdPartyLoginPromise _promise;

      /// <summary>
      /// Begin the Google Sign-In process.
      /// </summary>
      /// <param name="promise">Promise to be completed when sign-in succeeds or fails</param>
      public void StartGoogleLogin(ThirdPartyLoginPromise promise)
      {
         if (promise.ThirdParty != AuthThirdParty.Google)
         {
            return;
         }

         var clientId = ConfigDatabase.GetString("google_client_id");
         _google = new GoogleSignIn(gameObject, "GoogleAuthResponse", clientId);
         _promise = promise;
         _google.Login();
      }

      /// <summary>
      /// Callback to be invoked via UnitySendMessage when the plugin either
      /// receives a valid ID token or indicates an error.
      /// </summary>
      /// <param name="message">Response message from the Google Sign-In plugin</param>
      private void GoogleAuthResponse(string message)
      {
         if (_promise == null)
         {
            return;
         }

         GoogleSignIn.HandleResponse(
            message,
            token =>
            {
               _promise.CompleteSuccess(new ThirdPartyLoginResponse(token));
            },
            exc =>
            {
               _promise.CompleteError(exc);
            });
      }
   }
}
