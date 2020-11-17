using System.IO;
using UnityEngine;

namespace DisruptorBeam.Modules.Theme
{
   public class ThemeConfiguration : ModuleConfigurationObject
   {
      public static ThemeConfiguration Instance => Get<ThemeConfiguration>();

      public ThemeObject Style;
   }

}