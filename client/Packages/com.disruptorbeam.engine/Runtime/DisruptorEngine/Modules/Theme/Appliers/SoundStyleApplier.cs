using DisruptorBeam.Modules.Theme.Palettes;
using DisruptorBeam.UI.Scripts;

namespace DisruptorBeam.Modules.Theme.Appliers
{
   [System.Serializable]
   public class SoundStyleApplier : StyleApplier<EventSoundBehaviour>
   {
      public SoundBinding Sound;
      public override void Apply(ThemeObject theme, EventSoundBehaviour component)
      {
         if (!Sound.Exists())
         {
            component.enabled = false;
            return;
         }

         component.enabled = true;
         var soundStyle = theme.GetPaletteStyle(Sound);
         if (soundStyle != null)
         {
            component.Clip = soundStyle.AudioClip;
            component.Volume = soundStyle.Volume;
         }
      }
   }
}