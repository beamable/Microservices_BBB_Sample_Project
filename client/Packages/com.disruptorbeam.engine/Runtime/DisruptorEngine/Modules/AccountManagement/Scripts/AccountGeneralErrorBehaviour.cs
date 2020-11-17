using DisruptorBeam.UI.Scripts;

namespace DisruptorBeam.Modules.AccountManagement
{
   public class AccountGeneralErrorBehaviour : MenuBase
   {
      public TextReference ErrorText;

      public void Show(string error)
      {
         var menu = Manager.Show<AccountGeneralErrorBehaviour>();
         ErrorText.Value = error;
      }
   }
}