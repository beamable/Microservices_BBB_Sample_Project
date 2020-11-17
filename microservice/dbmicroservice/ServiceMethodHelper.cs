using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace Beamable.Server
{
   public static class ServiceMethodHelper
   {
      public static ServiceMethodCollection Scan<TMicroService>() where TMicroService : Microservice
      {
         var output = new List<ServiceMethod>();
         var type = typeof(TMicroService); // rely on dynamic

         Log.Debug("Scanning client methods for {typeName}", type.Name);

         var allMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
         foreach (var method in allMethods)
         {
            var closureMethod = method;
            var attribute = method.GetCustomAttribute<ClientCallableAttribute>();
            if (attribute == null) continue;

            var servicePath = attribute.PathName;
            if (string.IsNullOrEmpty(servicePath))
            {
               servicePath = method.Name;
            }

            Log.Debug("Found {method} for {path}", method.Name, servicePath);


            var parameters = method.GetParameters();

            var deserializers = new List<Func<string, object>>();
            foreach (var parameter in parameters)
            {
               var pType = parameter.ParameterType;

               Func<string, object> deserializer = (json) =>
               {
                  if (typeof(string) == pType)
                  {
                     return json;
                  }

                  var deserializeObject = JsonConvert.DeserializeObject(json, pType);
                  return deserializeObject;
               };
               deserializers.Add(deserializer);
            }

            var isAsync = null != method.GetCustomAttribute<AsyncStateMachineAttribute>();

            Func<object, object[], Task> executor;

            if (isAsync)
            {
               executor = (target, args) =>
               {
                  var task = (Task)closureMethod.Invoke(target, args);
                  return task;
               };
            }
            else
            {
               executor = (target, args) =>
               {
                  var invocationResult = closureMethod.Invoke(target, args);
                  return Task.FromResult(invocationResult);
               };
            }

            var serviceMethod = new ServiceMethod
            {
               Path = servicePath,
               Deserializers = deserializers,
               Executor = executor
            };
            output.Add(serviceMethod);
         }

         return new ServiceMethodCollection(output);
      }

   }
}