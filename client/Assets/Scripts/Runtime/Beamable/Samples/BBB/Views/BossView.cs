using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace Beamable.Samples.BBB.Views
{
   /// <summary>
   /// Handles the view concerns for the Boss character.
   /// </summary>
   public class BossView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public List<Renderer> Renderers { get { return _renderers; } }
      public GameObject PointsViewTarget { get { return _pointsViewTarget; } }

      //  Fields ---------------------------------------
      [SerializeField]
      private Animator _animator = null;

      [SerializeField]
      private GameObject _pointsViewTarget = null;

      private List<Renderer> _renderers = null;

      //  Unity Methods   ------------------------------
      protected void Awake()
      {
         _renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();
      }

      //  Other Methods --------------------------------
      public void TakeDamage()
      {
         string triggerName = BBBHelper.GetRandomString(
            new List<string> { BBBConstants.Boss_TakeDamage01, BBBConstants.Boss_TakeDamage02 });

         _animator.SetTrigger(triggerName);
      }

      public void Die()
      {
         _animator.SetTrigger(BBBConstants.Boss_Die);

         //Fall to the ground
         gameObject.transform.DOMove(new Vector3(-2.5f, 0.1f, -.95f), 1);
         gameObject.transform.DORotate(new Vector3(0, 180, 0), 1);

      }

      //  Event Handlers -------------------------------
   }
}