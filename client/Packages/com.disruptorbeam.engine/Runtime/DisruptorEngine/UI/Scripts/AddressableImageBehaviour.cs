using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace DisruptorBeam.UI.Scripts
{
   public class AddressableImageBehaviour : MonoBehaviour
   {
      public Image Renderer;

      public async void Refresh(string address)
      {
         Renderer.sprite = await Addressables.LoadAssetAsync<Sprite>(address).Task;
      }
   }
}