using Beamable.Samples.BBB.Data;
using Beamable.Samples.BBB.Views;
using DG.Tweening;
using System.Collections;
using Beamable.Samples.Core.Audio;
using UnityEngine;

namespace Beamable.Samples.BBB
{
   /// <summary>
   /// The main entry point for in-game logic for the primary game scene.
   /// 
   /// NOTE: This is the CLIENT-AUTHORITATIVE (CA) implementation.
   /// 
   /// </summary>
   public class BBBGameManagerCA : MonoBehaviour
   {
      //  Fields ---------------------------------------
      [SerializeField]
      private Configuration _configuration = null;

      [SerializeField]
      private GameUI _gameUI = null;

      [SerializeField]
      private HeroView _heroView = null;

      [SerializeField]
      private BossView _bossView = null;

      [SerializeField]
      private PointsView _pointsViewPrefab = null;

      private PointsView _pointsViewInstance = null;

      private int _bossHealth = 0;


      //  Unity Methods   ------------------------------
      protected void Start ()
      {
         _gameUI.AttackButton.onClick.AddListener(AttackButton_OnClicked);
         _gameUI.AttackButtonText.text = BBBHelper.GetAttackButtonText(-1);

         BBBHelper.RenderersDoFade(_bossView.Renderers, 0, 1, 0, 3);
         BBBHelper.RenderersDoFade(_heroView.Renderers, 0, 1, 1, 3);

         //
         _gameUI.CanvasGroup.DOFade(0, 0);
         _gameUI.CanvasGroup.DOFade(1, 1).SetDelay(0.50f);

         //
         _bossHealth = 100;
         _gameUI.BossHealthBarView.Health = _bossHealth;
      }

      //  Other Methods --------------------------------
      private IEnumerator Attack()
      {
         _gameUI.AttackButton.interactable = false;

         // Wait - Click
         yield return new WaitForSeconds(_configuration.Delay1BeforeAttack);
         SoundManager.Instance.PlayAudioClip(SoundConstants.Click02);

         // Wait - Backswing
         yield return new WaitForSeconds(_configuration.Delay2BeforeBackswing);
         SoundManager.Instance.PlayAudioClip(SoundConstants.Unsheath01);
         _heroView.PrepareAttack();

         // Wait - Swing
         yield return new WaitForSeconds(_configuration.Delay3BeforeForeswing);
         SoundManager.Instance.PlayAudioClip(SoundConstants.Swing01);
         _heroView.Attack();

         // Wait - Damage
         yield return new WaitForSeconds(_configuration.Delay4BeforeTakeDamage);
         SoundManager.Instance.PlayAudioClip(SoundConstants.TakeDamage01);
         BBBHelper.RenderersDoColorFlicker(_bossView.Renderers, Color.red, 0.1f);
         _bossView.TakeDamage();

         // Wait - Points
         yield return new WaitForSeconds(_configuration.Delay5BeforePointsView);
         SoundManager.Instance.PlayAudioClip(SoundConstants.Coin01);

         // Show floating text, "-35" or "Missed!"
         if (_pointsViewInstance != null)
         {
            Destroy(_pointsViewInstance.gameObject);
         }
         _pointsViewInstance = Instantiate<PointsView>(_pointsViewPrefab);
         _pointsViewInstance.transform.position = _bossView.PointsViewTarget.transform.position;

         //
         int damage = 35;
         _pointsViewInstance.Points = -damage;
         _bossHealth -= damage;

         if (_bossHealth <= 0)
         {
            _bossHealth = 0;
            _bossView.Die();
            SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverWin);
         }
         else
         {
            _gameUI.AttackButton.interactable = true;
         }

         _gameUI.BossHealthBarView.Health = _bossHealth;
      }

      //  Event Handlers -------------------------------
      public void AttackButton_OnClicked ()
      {
         StartCoroutine(Attack());
      }
   }
}