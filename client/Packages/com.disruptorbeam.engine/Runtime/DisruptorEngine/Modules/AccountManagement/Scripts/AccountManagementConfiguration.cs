using System;
using System.Collections.Generic;
using Core.Platform.SDK;
using Core.Platform.SDK.Auth;
using DisruptorBeam.Stats;
using UnityEngine;

namespace DisruptorBeam.Modules.AccountManagement
{
   [CreateAssetMenu(
      fileName="Account Management Configuration",
      menuName= ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
      "Account Management Configuration",
      order= ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
   public class AccountManagementConfiguration : ModuleConfigurationObject
   {
      public struct UserThirdPartyAssociation
      {
         public AuthThirdParty ThirdParty;
         public bool HasAssociation;
         public bool ThirdPartyEnabled;

         public bool ShouldShowIcon => ThirdPartyEnabled && HasAssociation;
         public bool ShouldShowButton => ThirdPartyEnabled && !HasAssociation;
      }

      public static AccountManagementConfiguration Instance => Get<AccountManagementConfiguration>();

      public bool Facebook, Apple, Google;

      [Tooltip("The stat to use to show account names")]
      public StatObject DisplayNameStat;

      [Tooltip("The label to use next to the sub text")]
      public string SubtextLabel = "Progress";

      [Tooltip("The stat to use to show account sub text")]
      public StatObject SubtextStat;

      [Tooltip("The stat to use to hold an avatar addressable sprite asset")]
      public StatObject AvatarStat;

      [Tooltip("Allows you to override specific account management functionality")]
      [SerializeField]
      private AccountManagementAdapter _overrides;

      [Tooltip("The max character limit of a player's alias")]
      public int AliasCharacterLimit = 18;

      [Tooltip("Controls the presence of the promotional banner on the main menu of account management.")]
      public bool ShowPromotionalSlider = false;

      public AccountManagementAdapter Overrides
      {
         get
         {
            if (_overrides == null)
            {
               if (_overrides == null)
               {
                  var gob = new GameObject();
                  _overrides = gob.AddComponent<AccountManagementAdapter>();
               }
            }
            return _overrides;
         }
      }

      public bool Has(AuthThirdParty thirdParty)
      {
         switch (thirdParty)
         {
            case AuthThirdParty.Facebook:
               return Facebook;
            case AuthThirdParty.Apple:
               return Apple && (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS);
            case AuthThirdParty.Google:
               return Google && Application.platform == RuntimePlatform.Android;
            default:
               return false;
         }
      }

      public Promise<List<UserThirdPartyAssociation>> GetAllEnabledThirdPartiesForUser(User user)
      {
         // for each user, we need to run a promise out
         var promises = new List<Promise<UserThirdPartyAssociation>>();

         var thirdParties = (AuthThirdParty[])Enum.GetValues(typeof(AuthThirdParty));
         foreach (var thirdParty in thirdParties)
         {
            if (!Has(thirdParty))
            {
               promises.Add(Promise<UserThirdPartyAssociation>.Successful(new UserThirdPartyAssociation
               {
                  HasAssociation = false,
                  ThirdParty = thirdParty,
                  ThirdPartyEnabled = false
               }));
            }
            else
            {
               // TODO, somehow we should be able to cache this fact, so we don't keep on pinging apis.
               promises.Add(Overrides.DoesUserHaveThirdParty(user, thirdParty).Map(hasThirdParty =>
               new UserThirdPartyAssociation
               {
                  HasAssociation = hasThirdParty,
                  ThirdParty = thirdParty,
                  ThirdPartyEnabled = true
               }));
            }
         }

         return Promise.Sequence(promises);
      }

   }
}
