using System;
using Beamable.Common.Content;
using Core.Platform.SDK;
using DisruptorBeam.Content;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace DisruptorBeam.Editor.Content.UI.ProjectView
{

   public abstract class ContentGroupVisualElement : VisualElement
   {
      public abstract void CreateNew();
      public abstract void Delete(ContentObject content);
   }

   public class ContentGroupVisualElement<TContent> : ContentGroupVisualElement
      where TContent: ContentObject, new()
   {

      private const string Asset_UXML_ContentGroupElement =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentGroupElement.uxml";

      private const string Asset_USS_ContentGroupElement =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentGroupElement.uss";


      private readonly VisualTreeAsset _treeAsset;
      private readonly Toggle _foldOutToggle;
      private readonly VisualElement _list;
      private readonly VisualElement _main;

      private Promise<EditorDisruptorEngine> _de;

      public ContentGroupVisualElement()
      {
         _treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_ContentGroupElement);
         _main = _treeAsset.CloneTree();

         _foldOutToggle = _main.Q<Toggle>(null, "unity-foldout-toggle");
         _list = _main.Q<VisualElement>(name = "list");

         this.AddStyleSheet(Asset_USS_ContentGroupElement);
         Add(_main);

         _de = EditorDisruptorEngine.Instance;

         _main.Q<Button>("create-new-button").clickable.clicked += CreateNew;

         RegisterListeners();
         Refresh();
      }

      string GetName(ContentObject content)
      {
         return content == null ? null : $"content_{content.Id}";
      }

      void RegisterListeners()
      {
         var group = ContentIO.GetEventGroup<TContent>();
         group.OnCreated += content =>
         {
            // check to see if this already exists...
            var existingItem = _list.Q<ContentItemVisualElement<TContent>>(name = GetName(content));
            if (existingItem != null)
            {
               existingItem.Content = content;
               existingItem.Refresh();
               return;
            }

            _de.Then(de => CreateItem(content, de.ContentIO));

         };
         group.OnDeleted += content =>
         {
            var existingItem = _list.Q<ContentItemVisualElement<TContent>>(name = GetName(content));
            if (existingItem == null) return;

            _list.Remove(existingItem);
         };
      }

      ContentItemVisualElement<TContent> CreateItem(TContent content, ContentIO io)
      {
         var contentItem = new ContentItemVisualElement<TContent>();
         contentItem.name = GetName(content);
         contentItem.Content = content;

         contentItem.OnClicked += () => { io.Select(content); };

         contentItem.Refresh();
         _list.Add(contentItem);

         return contentItem;
      }

      public void Refresh()
      {
         _de.Then(de =>
         {
            var allContent = de.ContentIO.FindAllContent<TContent>(false);

            _main.Q<Label>(name = "title").text = ContentRegistry.TypeToName(typeof(TContent));

            foreach (var content in allContent)
            {
               CreateItem(content, de.ContentIO);
            }
         });
      }

      public override void CreateNew()
      {
         var content = ScriptableObject.CreateInstance<TContent>();

         _de.Then(de =>
         {
            _foldOutToggle.value = true;
            var item = CreateItem(content, de.ContentIO);
            item.BeginNaming()
               .Then(name =>
               {
                  content.SetContentName(name);
                  item.Refresh();
                  de.ContentIO.Create(content);
               })
               .Error(err =>
            {
               _list.Remove(item);
            });
         });
      }

      public override void Delete(ContentObject content)
      {
         _de.Then(de =>
         {
            var typedContent = content as TContent;
            de.ContentIO.Delete(typedContent);
         });
      }
   }
}