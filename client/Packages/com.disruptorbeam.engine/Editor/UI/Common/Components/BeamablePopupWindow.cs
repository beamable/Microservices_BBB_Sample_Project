using System;
using System.Collections.Generic;
using System.Linq;
using DisruptorBeam.Editor.UI.Buss.Components;
using DisruptorBeam.UI.Buss;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace DisruptorBeam.Editor.UI.Buss.Components
{
   public class BeamablePopupWindow : EditorWindow
   {
      public static BeamablePopupWindow ShowDropdown(string title, Rect sourceRect, Vector2 size, BeamableVisualElement content)
      {
         var wnd = CreateInstance<BeamablePopupWindow>();
         wnd.titleContent = new GUIContent(title);
         wnd.ContentElement = content;

         wnd.ShowAsDropDown(sourceRect, size);
         wnd.Refresh();
         return wnd;
      }


      private const string PATH = "Packages/com.disruptorbeam.engine/Editor/UI/Common/Components";

      public BeamableVisualElement ContentElement;
      private VisualElement _windowRoot;
      private VisualElement _contentRoot;
      private VisualElement _container;

      private void OnEnable()
      {
         VisualElement root = this.GetRootVisualContainer();
         var uiAsset =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PATH}/beamablePopupWindow.uxml");
         _windowRoot = uiAsset.CloneTree();
         _windowRoot.AddStyleSheet($"{PATH}/beamablePopupWindow.uss");
         _windowRoot.name = nameof(_windowRoot);

         root.Add(_windowRoot);
      }

      public void Refresh()
      {
         _container = _windowRoot.Q<VisualElement>("container");
         _container.Clear();
         _container.Add(ContentElement);
         ContentElement.Refresh();
         Repaint();
      }
   }
}