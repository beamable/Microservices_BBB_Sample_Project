using System;
using DisruptorBeam.UI.Buss;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace DisruptorBeam.Editor.UI.Buss
{
   public class BeamableVisualElement : VisualElement
   {
      public const string UI_PACKAGE_PATH = "Packages/com.disruptorbeam.engine/Editor/UI";

      protected VisualTreeAsset TreeAsset { get; private set; }
      protected VisualElement Root { get; private set; }

      protected string UXMLPath { get; private set; }

      protected string USSPath { get; private set; }

      public BeamableVisualElement(string commonPath) : this(commonPath + ".uxml", commonPath + ".uss") {}

      public BeamableVisualElement(string uxmlPath, string ussPath)
      {
         UXMLPath = uxmlPath;
         USSPath = ussPath;
         TreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXMLPath);

      }

      public virtual void Refresh()
      {
         Clear();

         Root = TreeAsset.CloneTree();

         this.AddStyleSheet(USSPath);
         Add(Root);
      }
   }
}