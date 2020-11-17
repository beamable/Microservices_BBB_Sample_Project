using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Server.Editor.DockerCommands;
using Beamable.Server.Editor.UI.Components;
using Core.Platform.SDK;
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

namespace Beamable.Server.Editor.UI
{
   public class DeployWindow : CommandRunnerWindow
   {
      public static DeployWindow ShowPopup()
      {
         var wnd = CreateInstance<DeployWindow>();
         wnd.titleContent = new GUIContent("Deploy");

         ((EditorWindow) wnd).ShowUtility();

         Microservices.GenerateUploadModel().Then(model =>
         {
            wnd._model = model;
            wnd.Refresh();
         });

         return wnd;
      }

      private const string PATH = "Packages/com.disruptorbeam.engine/Editor/UI/Common/Components";

      private VisualElement _windowRoot;
      private VisualElement _contentRoot;
      private VisualElement _container;
      private ManifestModel _model;


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


         var e = new ManifestVisualElement(_model);
         e.OnCancel += Close;
         e.OnSubmit += async (model) =>
         {
            /*
             * We need to build each image...
             * upload each image that is different than whats in the manifest...
             * upload the manifest file...
             */
            e.parent.Remove(e);

            await Microservices.Deploy(_model, this);
            Close();
         };

         _container.Add(e);
         e.Refresh();
         Repaint();
      }
   }
}