using System;
using DisruptorBeam.Editor.UI.Buss.Extensions;
using DisruptorBeam.Editor.UI.Buss.Model;
using DisruptorBeam.UI.Buss;
using DisruptorBeam.UI.Buss.Properties;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;

#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


namespace DisruptorBeam.Editor.UI.Buss.Components
{
   public class VariableScopeVisualElement : BeamableVisualElement
   {
      public VariableScope Model { get; }
      public const string COMMON = UI_PACKAGE_PATH + "/Buss/Components/variableScopeVisualElement";

      public VariableScopeVisualElement(VariableScope model) : base(COMMON)
      {
         Model = model;
      }
   }
}