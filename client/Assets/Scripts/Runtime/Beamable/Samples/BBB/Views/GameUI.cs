using Beamable.Samples.BBB.Audio;
using Beamable.Samples.BBB.Data;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Beamable.Samples.BBB.Views
{
   /// <summary>
   /// Handles the view concerns for the main in-game UI elements.
   /// </summary>
   public class GameUI : MonoBehaviour
   {
      //  Events ---------------------------------------          

      //  Properties -----------------------------------
      public Button AttackButton { get { return _attackButton; } }
      public TextMeshProUGUI AttackButtonText { get { return _attackButtonText; } }
      public CanvasGroup CanvasGroup { get { return _canvasGroup; } }
      public HealthBarView BossHealthBarView { get { return _bossHealthBarView; } }

      [SerializeField]
      private HealthBarView _bossHealthBarView = null;

      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private CanvasGroup _canvasGroup = null;

      [SerializeField]
      private Button _attackButton = null;

      [SerializeField]
      private TextMeshProUGUI _attackButtonText = null;
      
      [SerializeField]
      private Button _backButton = null;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _backButton.onClick.AddListener(BackButton_OnClicked);
      }

      //  Other Methods --------------------------------

      //  Event Handlers -------------------------------
      private void BackButton_OnClicked()
      {
         _backButton.interactable = false;

         SoundManager.Instance.PlayAudioClip(SoundConstants.Click01);
         StartCoroutine(LoadScene());
      }

      private IEnumerator LoadScene()
      {
         yield return new WaitForSeconds(_configuration.Delay5BeforePointsView);
         SceneManager.LoadSceneAsync(_configuration.IntroSceneName, LoadSceneMode.Single);
      }
   }
}