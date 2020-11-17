using Core.Platform.SDK.Auth;
using DisruptorBeam.UI.Scripts;

public class AccountThirdPartyWaitingMenu : MenuBase
{

    public TextReference ThirdPartyMessage;

    public void GoBackToMainPage()
    {
        Manager.GoBackToPage<AccountMainMenu>();
    }

    public void SetFor(AuthThirdParty argThirdParty)
    {
        ThirdPartyMessage.Value = $"Signing into {argThirdParty}";
    }

}
