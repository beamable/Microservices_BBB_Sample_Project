using System;
using System.Text.Json;
namespace Beamable.Server
{
   public static class RequestContextExtensions
   {
      public static RequestContext BuildWebRequest(this string msg, IMicroserviceArgs args)
      {
         long id = 0;
         string path = "";
         string methodName = "";
         string body = ""; //is there an advantage to keeping it a JsonElement?
         long userID = 0;
         int status = 0;

         try
         {
            using (JsonDocument document = JsonDocument.Parse(msg))
            {
               id = document.RootElement.GetProperty("id").GetInt32();
               body = document.RootElement.GetProperty("body").ToString();
               JsonElement temp;
               if (document.RootElement.TryGetProperty("path", out temp))
                  path = temp.GetString();
               if (document.RootElement.TryGetProperty("method", out temp))
                  methodName = temp.GetString();
               if (document.RootElement.TryGetProperty("from", out temp))
                  userID = temp.GetInt64();
               if (document.RootElement.TryGetProperty("status", out temp))
               {
                  status = temp.GetInt32();
               }
            }
         }
         catch (Exception e)
         {
            Console.Write("Exception: " +
                          e); // was not printing exceptions without this was just silently moving on so I am doing that
         }

         return new RequestContext(args.CustomerID, args.ProjectName, id, status, userID, path, methodName, body);
      }
   }
}