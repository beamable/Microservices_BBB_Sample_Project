using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Server.Common;

namespace Beamable.Server
{
   public class ServiceMethodCollection
   {
      private Dictionary<string, ServiceMethod> _pathToMethod;

      public ServiceMethodCollection(IEnumerable<ServiceMethod> methods)
      {
         _pathToMethod = methods.ToDictionary(method => method.Path);
      }

      public async Task<string> Handle(Microservice service, string path, string[] jsonArgs)
      {
         BeamableSerilogProvider.LogContext.Value.Debug("Handling {path} with {jsonArgs}", path, jsonArgs);
         if (_pathToMethod.TryGetValue(path, out var method))
         {
            var output = method.Execute(service, jsonArgs);
            var result = await output;
            BeamableSerilogProvider.LogContext.Value.Debug("Method finished with {result}", result);
            return result;
         }
         else
         {
            BeamableSerilogProvider.LogContext.Value.Warning("No method available for {path}", path);

            throw new Exception($"Unhandled path=[{path}]");
         }
      }
   }
}