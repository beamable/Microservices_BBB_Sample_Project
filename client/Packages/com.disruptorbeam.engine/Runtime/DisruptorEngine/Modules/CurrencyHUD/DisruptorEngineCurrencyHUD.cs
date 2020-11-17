using System.Collections;

using Core.Coroutines;

using Core.Service;

using DisruptorBeam.Content;

using TMPro;

using UnityEngine;

using UnityEngine.UI;

using UnityEngine.AddressableAssets;



namespace DisruptorBeam.Modules.CurrencyHUD

{


   [HelpURL(ContentConstants.URL_FEATURE_CURRENCY_HUD)]
   public class DisruptorEngineCurrencyHUD : MonoBehaviour

    {

        public CurrencyRef content;

        public Canvas canvas;

        public RawImage img;

        public TextMeshProUGUI txtAmount;

        private long targetAmount = 0;

        private long currentAmount = 0;



        void Awake()

        {

            canvas.enabled = false;

        }



        async void Start()

        {

            var de = await DisruptorEngine.Instance;

            de.InventoryService.Subscribe(content.Id, view =>

            {

                view.currencies.TryGetValue(content.Id, out targetAmount);

                ServiceManager.Resolve<CoroutineService>().StartCoroutine(DisplayCurrency());

            });

            var currency = await content.Resolve();

            var icon = await currency.Icon.LoadAssetAsync().Task;

            img.texture = icon.texture;

            

            canvas.enabled = true;

        }



        private IEnumerator DisplayCurrency()

        {

            long deltaTotal = targetAmount - currentAmount;

            long deltaStep = deltaTotal / 50;

            if (deltaStep == 0)

            {

                if (deltaTotal < 0)

                    deltaStep = -1;

                else

                    deltaStep = 1;

            }



            while (currentAmount != targetAmount)

            {

                currentAmount += deltaStep;

                if (deltaTotal > 0 && currentAmount > targetAmount)

                {

                    currentAmount = targetAmount;

                }

                else if (deltaTotal < 0 && currentAmount < targetAmount)

                {

                    currentAmount = targetAmount;

                }



                txtAmount.text = currentAmount.ToString();

                yield return new WaitForSeconds(0.02f);

            }

        }

    }

}