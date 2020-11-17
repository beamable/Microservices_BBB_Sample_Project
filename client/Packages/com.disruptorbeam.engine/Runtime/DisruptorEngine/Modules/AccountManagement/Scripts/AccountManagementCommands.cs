using System.Linq;
using Core.ConsoleCommands;
using Core.Platform.SDK.Auth;
using DisruptorBeam.Signals;
using UnityEngine;
using UnityEngine.Scripting;

namespace DisruptorBeam.Modules.AccountManagement
{
   [DBeamConsoleCommandProvider]
   public class AccountManagementCommands
   {
      [Preserve]
      public AccountManagementCommands()
      {
         
      }

      [DBeamConsoleCommand("account_toggle", "emit an account management toggle event", "account_toggle")]
      public string ToggleAccount(string[] args)
      {
         DeSignalTower.ForAll<AccountManagementSignals>(s => s.OnToggleAccountManagement?.Invoke(!AccountManagementSignals.ToggleState));
         return "okay";
      }

      [DBeamConsoleCommand("account_list", "list user data", "account_list")]
      public string ListCredentials(string[] args)
      {
         DisruptorEngine.Instance.Then(de =>
         {
            de.GetDeviceUsers().Then(all =>
            {
               all.ToList().ForEach(bundle =>
               {
                  var user = bundle.User;
                  var userType = user.id == de.User.id ? "CURRENT" : "DEVICE";
                  Debug.Log($"{userType} : EMAIL: [{user.email}] ID: [{user.id}] 3RD PARTIES: [{string.Join(",", user.thirdPartyAppAssociations)}]");
               });
            });
         });
         return "";
      }
   }
}