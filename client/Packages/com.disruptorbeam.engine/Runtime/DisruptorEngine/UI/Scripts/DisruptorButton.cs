using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DisruptorBeam;


namespace DisruptorBeam.UI.Scripts
{
   public class DisruptorButton : UIBehaviour
   {
      public Button Button;
      public TextMeshProUGUI Text;
      public Gradient Gradient;
      public EventSoundBehaviour SoundBehaviour;
      public bool RequiresConnectivity;
      private DisruptorBeam.IDisruptorEngine _engineInstance;


      protected override async void Start()
      {
            base.Start();
            _engineInstance = await DisruptorBeam.DisruptorEngine.Instance;
            _engineInstance.ConnectivityService.OnConnectivityChanged += toggleButton;
      }

      public void toggleButton(bool offlineStatus)
      {
            if(RequiresConnectivity && Button != null)
            {
                Button.interactable = offlineStatus;
            }
      }
   }
}