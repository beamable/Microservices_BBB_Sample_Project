using System;
using System.Linq;
using System.Reflection;
using Beamable.Common.Content;
using DisruptorBeam.Content;
using UnityEditor;
using UnityEngine;

namespace DisruptorBeam.Editor.Content
{
   [CustomPropertyDrawer(typeof(BaseContentRef), true)]
   public class ContentPropertyDrawer : PropertyDrawer
   {
      public override async void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         // TODO disallow editing of multiple objects
         // TODO dont do so much file IO. This runs every time....
         // TODO probably don't reconstruct the generic method EVERY time...

         var target = property.serializedObject.targetObject;
         var fieldValue = GetTargetObjectOfProperty(property) as BaseContentRef;
         if (fieldValue == null)
            return;
         var referenceType = fieldValue.GetReferencedBaseType();
         var de = await EditorDisruptorEngine.Instance;
         de.ContentIO.EnsureDefaultContentByType(referenceType);
         var allContent = de.ContentIO.FindAllContentByType(referenceType).ToList();
         var choices = allContent.Select(c => c.ContentName).ToList();
         var index = 0;
         for (var i = 0; i < allContent.Count; i++)
         {
            if (fieldValue.IsContent(allContent[i]))
            {
               index = i + 1;
               break;
            }
         }

         choices.Insert(0, "<null>"); // TODO do we even want to do this? Should we allow null values?

         var displayName = property.displayName;
         var choiceArray = choices.ToArray();

         var outputIndex = EditorGUI.Popup(position, displayName, index, choiceArray);

         if (outputIndex != index)
         {
            fieldValue.SetId(outputIndex > 0 ? allContent[outputIndex - 1].Id : null);
            EditorUtility.SetDirty(target);
         }

         if (outputIndex > 0)
         {
            de.ContentIO.EnsureDefaultAssetsByType(referenceType);
         }
      }


      /// TAKEN FROM https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
      /// <summary>
      /// Gets the object the property represents.
      /// </summary>
      /// <param name="prop"></param>
      /// <returns></returns>
      public static object GetTargetObjectOfProperty(SerializedProperty prop)
      {
         if (prop == null) return null;

         var path = prop.propertyPath.Replace(".Array.data[", "[");
         object obj = prop.serializedObject.targetObject;
         var elements = path.Split('.');
         foreach (var element in elements)
         {
            if (element.Contains("["))
            {
               var elementName = element.Substring(0, element.IndexOf("["));
               var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
               obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
               obj = GetValue_Imp(obj, element);
            }
         }
         return obj;
      }
      private static object GetValue_Imp(object source, string name)
      {
         if (source == null)
            return null;
         var type = source.GetType();

         while (type != null)
         {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
               return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
               return p.GetValue(source, null);

            type = type.BaseType;
         }
         return null;
      }

      private static object GetValue_Imp(object source, string name, int index)
      {
         var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
         if (enumerable == null) return null;
         var enm = enumerable.GetEnumerator();
         //while (index-- >= 0)
         //    enm.MoveNext();
         //return enm.Current;

         for (int i = 0; i <= index; i++)
         {
            if (!enm.MoveNext()) return null;
         }
         return enm.Current;
      }

   }
}
