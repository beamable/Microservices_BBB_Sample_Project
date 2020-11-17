using DisruptorBeam.Modules.Theme.Palettes;
using UnityEngine.UI;

namespace DisruptorBeam.Modules.Theme.Appliers
{
   [System.Serializable]
   public class SelectableStyleApplier : StyleApplier<Selectable>
   {
      public SelectableBinding SelectableBinding;
      public override void Apply(ThemeObject theme, Selectable component)
      {
         var selectableStyle = theme.GetPaletteStyle(SelectableBinding);
         component.transition = selectableStyle.SelectionData.Transition;
         component.spriteState = selectableStyle.SelectionData.SpriteState;

         var colorBlock = selectableStyle.SelectionData.Colors;
         component.colors = new ColorBlock
         {
            colorMultiplier = colorBlock.ColorMultiplier,
            fadeDuration = colorBlock.FadeDuration,
            disabledColor = theme.GetPaletteStyle(colorBlock.DisabledColor).Color,
            highlightedColor = theme.GetPaletteStyle(colorBlock.HighlightedColor).Color,
            normalColor = theme.GetPaletteStyle(colorBlock.NormalColor).Color,
            pressedColor = theme.GetPaletteStyle(colorBlock.PressedColor).Color,
         };
         component.animationTriggers = selectableStyle.SelectionData.AnimationTriggers;
      }
   }
}