using Beamable.Samples.BBB.Audio;
using Beamable.Samples.BBB.Data;
using Beamable.Samples.BBB.Views;
using Beamable.Server.BBBGameMicroservice;
using Beamable.Server.BBBGameMicroservice.Content;
using Beamable.Server.Clients;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Samples.BBB
{
   /// <summary>
   /// The main entry point for in-game logic for the primary game scene.
   ///
   /// NOTE: This is the SERVER-AUTHORITATIVE (CA) implementation.
   /// This uses Beamable Microservices.
   ///
   /// </summary>
   public class BBBGameManagerSA : MonoBehaviour
   {
      //  Fields ---------------------------------------

      [SerializeField]
      private GameUI _gameUI = null;

      [SerializeField]
      private HeroView _heroView = null;

      [SerializeField]
      private BossView _bossView = null;

      [SerializeField]
      private PointsView _pointsViewPrefab = null;

      [SerializeField]
      private Configuration _configuration = null;

      [Header("Content")]
      [SerializeField]
      private BossContentRef _bossContentRef = null;

      [SerializeField]
      private List<WeaponContentRef> _weaponContentRefs = null;

      private PointsView _pointsViewInstance = null;
      private BBBGameMicroserviceClient _bbbGameMicroserviceClient = null;

      //  Unity Methods   ------------------------------
      protected void Start()
      {
         _gameUI.AttackButton.onClick.AddListener(AttackButton_OnClicked);

         // Block user interaction
         _gameUI.CanvasGroup.DOFade(0, 0);
         _gameUI.AttackButton.interactable = false;

         _bbbGameMicroserviceClient = new BBBGameMicroserviceClient();
         StartTheBattle();

      }

      //  Other Methods --------------------------------
      private void StartTheBattle ()
      {
         int heroWeaponIndexMax = _weaponContentRefs.Count;

         // ----------------------------
         // Call Microservice Method #1
         // ----------------------------
         _bbbGameMicroserviceClient.StartTheBattle(_bossContentRef, heroWeaponIndexMax)
            .Then((StartTheBattleResults results) =>
            {
               _gameUI.BossHealthBarView.Health = results.BossHealthRemaining;
               _gameUI.AttackButtonText.text = BBBHelper.GetAttackButtonText(results.HeroWeaponIndex);

               // Find the Weapon data from the Weapon content
               _weaponContentRefs[results.HeroWeaponIndex].Resolve()
                  .Then(content =>
                  {
                     //TODO; Fix fade in of models (Both scenes)
                     BBBHelper.RenderersDoFade(_bossView.Renderers, 0, 1, 0, 3);
                     BBBHelper.RenderersDoFade(_heroView.Renderers, 0, 1, 1, 3);

                     // Allow user interaction
                     _gameUI.AttackButton.interactable = true;
                     _gameUI.CanvasGroup.DOFade(1, 1).SetDelay(0.50f);

                  })
                  .Error((Exception exception) =>
                  {
                     System.Console.WriteLine("_bossContentRef.Resove() error: " + exception.Message);
                  });

            })
            .Error((Exception exception) =>
            {
               UnityEngine.Debug.Log($"StartTheBattle() error:{exception.Message}");
            });
      }


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

         bool isDone = false;

         // ----------------------------
         // Call Microservice Method #2
         // ----------------------------
         AttackTheBossResults attackTheBossResults = null;
         _bbbGameMicroserviceClient.AttackTheBoss(_weaponContentRefs)
            .Then((AttackTheBossResults results) =>
            {
               isDone = true;
               attackTheBossResults = results;

            })
               .Error((Exception exception) =>
            {
               UnityEngine.Debug.Log($"AttackTheBoss() error:{exception.Message}");
            });

         while (!isDone)
         {
            yield return new WaitForEndOfFrame();
         }

         // Wait - Swing
         yield return new WaitForSeconds(_configuration.Delay3BeforeForeswing);
         SoundManager.Instance.PlayAudioClip(SoundConstants.Swing01);
         _heroView.Attack();

         // Show floating text, "-35" or "Missed!"
         if (_pointsViewInstance != null)
         {
            Destroy(_pointsViewInstance.gameObject);
         }
         _pointsViewInstance = Instantiate<PointsView>(_pointsViewPrefab);
         _pointsViewInstance.transform.position = _bossView.PointsViewTarget.transform.position;

         // Evaluate damage
         if (attackTheBossResults.DamageAmount > 0)
         {
            // Wait - Damage
            yield return new WaitForSeconds(_configuration.Delay4BeforeTakeDamage);
            SoundManager.Instance.PlayAudioClip(SoundConstants.TakeDamage01);
            BBBHelper.RenderersDoColorFlicker(_bossView.Renderers, Color.red, 0.1f);
            _bossView.TakeDamage();

            // Wait - Points
            yield return new WaitForSeconds(_configuration.Delay5BeforePointsView);
            SoundManager.Instance.PlayAudioClip(SoundConstants.Coin01);

            _pointsViewInstance.Points = -attackTheBossResults.DamageAmount;

         }
         else
         {
            // Wait - Points
            yield return new WaitForSeconds(_configuration.Delay5BeforePointsView);
            SoundManager.Instance.PlayAudioClip(SoundConstants.Coin01);

            _pointsViewInstance.Text = BBBHelper.GetAttackMissedText();
         }

         if (attackTheBossResults.BossHealthRemaining <= 0)
         {
            _bossView.Die();
            SoundManager.Instance.PlayAudioClip(SoundConstants.GameOverWin);
         }
         else
         {
            _gameUI.AttackButton.interactable = true;
         }

         _gameUI.BossHealthBarView.Health = attackTheBossResults.BossHealthRemaining;
      }

      //  Event Handlers -------------------------------
      public void AttackButton_OnClicked ()
      {
         StartCoroutine(Attack());
      }
   }
}