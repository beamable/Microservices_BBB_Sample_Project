using System;
using DisruptorBeam.Modules.Theme;
using DisruptorBeam.Modules.Theme.Palettes;
using UnityEngine;
using FontStyle = DisruptorBeam.Modules.Theme.Palettes.FontStyle;

namespace DisruptorBeam.Editor.Modules.Theme
{
   public interface IPaletteStyleObject
   {
      void SetStyle(PaletteStyle style);
      bool Modified { get; }
   }

   public class PaletteStyleObject<T> : ScriptableObject, IPaletteStyleObject where T : PaletteStyle
   {
      public T Style;

      public void SetStyle(PaletteStyle style)
      {
         Style = style as T;
         Modified = false;
      }

      private void OnValidate()
      {
         Modified = true;
      }

      public bool Modified { get; set; }
   }

}