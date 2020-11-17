using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Samples.BBB.Views
{
   /// <summary>
   /// Handles the view concerns for the Boss character.
   /// </summary>
   public class HeroView : MonoBehaviour
   {
      //  Properties -----------------------------------
      public List<Renderer> Renderers { get { return _renderers; } }

      //  Fields ---------------------------------------
      [SerializeField]
      private Animator _animator = null;

      private List<Renderer> _renderers = null;

      //  Unity Methods   ------------------------------
      protected void Awake()
      {
         _renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();
      }

      //  Other Methods --------------------------------
      public void PrepareAttack()
      {
         _animator.SetTrigger(BBBConstants.Hero_Cover);
      }

      public void Attack()
      {
         string triggerName = BBBHelper.GetRandomString(
            new List<string> { BBBConstants.Hero_Attack01}); //BBBConstants.Hero_Attack02 = a double swing. Use?

         _animator.SetTrigger(triggerName);
      }

      //  Event Handlers -------------------------------
   }
}