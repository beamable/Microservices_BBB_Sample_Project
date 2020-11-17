using System.Collections.Generic;
using System.Linq;
using DisruptorBeam.Content;

namespace DisruptorBeam.Editor.Content
{
   public class ContentPublishSet
   {
      public Manifest ServerManifest;

      public List<ContentObject> ToAdd, ToModify;
      public List<string> ToDelete;

      public int totalOpsCount => ToAdd.Count + ToDelete.Count + ToModify.Count;

      public bool HasValidationErrors(out List<string> errors)
      {
         errors = new List<string>();

         var allContent = ToAdd.Concat(ToModify);
         foreach (var content in allContent)
         {
            if (content.HasValidationErrors(out var addErrors))
            {
               errors.AddRange(addErrors.Select(err => $"[{content.Id}] {err}"));
            }
         }

         return errors.Count > 0;
      }
   }

   public struct PublishProgress
   {
      public int TotalOperations, CompletedOperations;

      public float Progress => CompletedOperations / (float) TotalOperations;
   }
}