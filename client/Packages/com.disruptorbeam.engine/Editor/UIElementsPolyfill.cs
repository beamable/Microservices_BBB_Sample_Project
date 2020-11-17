using System;

#if UNITY_2018
namespace UnityEngine.Experimental.UIElements
{
    using StyleSheets;

    public static class UIElementsPolyfill2018
    {
        public static VisualElement CloneTree(this VisualTreeAsset self)
        {
            return self.CloneTree(null);
        }

        public static void AddStyleSheet(this VisualElement self, string path)
		    {
			    self.AddStyleSheetPath(path);
		    }

        public static void RemoveStyleSheet(this VisualElement self, string path)
        {
          self.RemoveStyleSheetPath(path);
        }

        public static void SetRight(this IStyle self, float value)
        {
            self.positionRight = value;
        }

        public static float GetMaxHeight(this IStyle self)
        {
            return self.maxHeight;
        }

        public static void SetImage(this Image self, Texture texture)
        {
            self.image = StyleValue<Texture>.Create(texture);
        }

        public static void BeamableFocus(this TextField self)
        {
            self.Focus();
        }

        public static void BeamableAppendAction(this DropdownMenu self, string title, Action<Vector2> callback)
        {
          self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition), DropdownMenu.MenuAction.AlwaysEnabled);
        }

        public static bool RegisterValueChangedCallback<T>(
          this INotifyValueChanged<T> control,
          EventCallback<ChangeEvent<T>> callback)
        {
          CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
          if (callbackEventHandler == null)
            return false;
          callbackEventHandler.RegisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
          return true;
        }

        public static bool UnregisterValueChangedCallback<T>(
          this INotifyValueChanged<T> control,
          EventCallback<ChangeEvent<T>> callback)
        {
          CallbackEventHandler callbackEventHandler = control as CallbackEventHandler;
          if (callbackEventHandler == null)
            return false;
          callbackEventHandler.UnregisterCallback<ChangeEvent<T>>(callback, TrickleDown.NoTrickleDown);
          return true;
        }
    }
}
#endif

#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;

namespace UnityEditor
{
  public static class UnityEditorPolyfill
  {
    public static VisualElement GetRootVisualContainer(this EditorWindow self)
    {
      return self.rootVisualElement;
    }
  }
}

namespace UnityEngine.UIElements
{
  public static class UIElementsPolyfill2019
  {
    public static void AddStyleSheet(this VisualElement self, string path)
    {
      self.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));
    }

    public static void RemoveStyleSheet(this VisualElement self, string path)
    {
      self.styleSheets.Remove(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));
    }
    public static void SetRight(this IStyle self, float value)
    {
      self.right = new StyleLength(value);
    }

    public static float GetMaxHeight(this IStyle self)
    {
      return self.maxHeight.value.value;
    }

    public static void SetImage(this Image self, Texture texture)
    {
      self.image = texture;
    }

    public static void BeamableFocus(this TextField self)
    {
      self.Q("unity-text-input").Focus();
    }

    public static void BeamableAppendAction(this DropdownMenu self, string title, Action<Vector2> callback)
    {
      self.AppendAction(title, evt => callback(evt.eventInfo.mousePosition));
    }
  }
}
#endif