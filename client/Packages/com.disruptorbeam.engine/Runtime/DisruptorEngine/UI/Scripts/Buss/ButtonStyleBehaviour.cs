using System;
using DisruptorBeam.UI.Buss.Extensions;
using DisruptorBeam.UI.MSDF;
using UnityEngine;
using UnityEngine.UI;

namespace DisruptorBeam.UI.Buss
{
   public class ButtonStyleBehaviour : StyleBehaviour
   {
      static ButtonStyleBehaviour()
      {
         RegisterType<ButtonStyleBehaviour>("button");
      }

      public override string TypeString => "button";

      public Selectable Button;
      public BeamableMSDFBehaviour MsdfBehaviour;

      public void Update()
      {
         SetClass("disabled", !Button.interactable);
      }

      public override void Apply(StyleObject styles)
      {
         MsdfBehaviour.ApplyStyleObject(styles);
      }
   }
}