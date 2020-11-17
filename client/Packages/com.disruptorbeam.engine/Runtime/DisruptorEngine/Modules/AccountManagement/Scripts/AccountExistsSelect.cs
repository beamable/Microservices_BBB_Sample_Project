using Core.Platform.SDK.Auth;
using DisruptorBeam.UI.Scripts;

public class AccountExistsSelect : MenuBase
{
   public AccountDisplayItem AccountDisplayItem;

   public void SetForUser(User user)
   {
      AccountDisplayItem.gameObject.SetActive(false);
      AccountDisplayItem.StartLoading(user, false, null).Then(_ => AccountDisplayItem.Apply());
   }

   public void SetForUserImmediate(AccountDisplayItem other)
   {
      AccountDisplayItem.StartLoading(other).Then(_ => AccountDisplayItem.Apply());
   }
}
