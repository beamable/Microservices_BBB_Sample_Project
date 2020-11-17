
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using DisruptorBeam.Editor.UI.Buss.Model;
using DisruptorBeam.Editor.UI.Buss.Wizard.Model;
using DisruptorBeam.UI.Buss;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;

#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace DisruptorBeam.Editor.UI.Buss.Wizard.Components
{
   public abstract class WizardStepVisualElement : BeamableVisualElement
   {
      public abstract string StepName { get; }
      public abstract string QuestionText { get; }
      public abstract string AboutText { get; }

      public abstract bool Enabled { get; }

      protected WizardModel Model { get; private set; }

      private const string COMMON = UI_PACKAGE_PATH + "/Buss/Wizard/Components/wizardStepVisualElement";

      protected VisualElement _questionContainer, _aboutContainer, _answerContainer;

      private Label _questionLabel, _aboutLabel;

      protected WizardStepVisualElement(WizardModel model) : base(COMMON)
      {
         Model = model;
      }

      protected abstract void Populate(WizardContext ctx, VisualElement container);

      public void Activate(WizardContext ctx)
      {
         OnActivate(ctx);
      }
      protected virtual void OnActivate(WizardContext ctx)
      {

      }

      public void Refresh(WizardContext ctx)
      {
         base.Refresh();
         _questionContainer = Root.Q<VisualElement>(className: "question");
         _answerContainer = Root.Q<VisualElement>(className: "answer");
         _aboutContainer = Root.Q<VisualElement>(className: "about");

         _questionLabel = _questionContainer.Q<Label>();
         _aboutLabel = _aboutContainer.Q<Label>();

         _questionLabel.text = QuestionText;
         _aboutLabel.text = AboutText;

         Populate(ctx, _answerContainer);
      }
   }
}