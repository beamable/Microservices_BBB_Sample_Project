using DisruptorBeam.Modules.Theme.Palettes;
using DisruptorBeam.UI.Scripts;
using UnityEngine;

namespace DisruptorBeam.Modules.Theme.Appliers
{
   [System.Serializable]
   public class TransformStyleApplier : StyleApplier<TransformOffsetBehaviour>
   {
      public TransformBinding Transform;
      public override void Apply(ThemeObject theme, TransformOffsetBehaviour component)
      {
         var transformStyle = theme.GetPaletteStyle(Transform);
         if (transformStyle == null) return;

         component.Offset = transformStyle.PositionOffset;
         component.Scale = transformStyle.Scale;
         component.ApplyOffset();
      }
   }
}