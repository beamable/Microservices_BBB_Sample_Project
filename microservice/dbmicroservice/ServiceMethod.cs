using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Beamable.Server
{

   public class ServiceMethod
   {
      public string Path;

      public List<Func<string, object>> Deserializers;

      public Func<object, object[], Task> Executor;

      public async Task<string> Execute(Microservice target, string[] jsonArgs)
      {

         var args = new object[Deserializers.Count];
         if (jsonArgs == null)
         {
            throw new Exception($"Parameter failure. given nullset");
         }

         if (jsonArgs.Length != args.Length)
         {
            throw new Exception($"Parameter cardinality failure. required={args.Length} given={jsonArgs.Length}");
         }

         for (var i = 0; i < args.Length; i++)
         {
            args[i] = Deserializers[i](jsonArgs[i]);
         }

         var task = Executor(target, args);
         await task;
         var resultProperty = task.GetType().GetProperty("Result");
         var result = resultProperty.GetValue(task); // TODO: XXX It stinks that there is active reflection going on the callpath

         var jsonResult = JsonConvert.SerializeObject(result);
         return jsonResult;
      }
   }
}