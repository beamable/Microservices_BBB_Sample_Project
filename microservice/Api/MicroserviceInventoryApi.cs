using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Api;
using Beamable.Common.Inventory.Api;
using Core.Platform.SDK;

namespace Beamable.Server.Api
{
   public class MicroserviceInventoryApi : IInventoryApi
   {
      public const string SERVICE_OBJECT = "/object/inventory";
      public IBeamableRequester Requester { get; }
      public RequestContext Ctx { get; }


      public MicroserviceInventoryApi(IBeamableRequester requester, RequestContext ctx)
      {
         Requester = requester;
         Ctx = ctx;
      }

      public Promise<Unit> SetCurrency(string currencyId, long amount, string transaction)
      {
         return SetCurrencies(new Dictionary<string, long>
         {
            {currencyId, amount}
         }, transaction);
      }

      public Promise<Unit> AddCurrency(string currencyId, long amount, string transaction = null)
      {
         return AddCurrencies(new Dictionary<string, long>
         {
            {currencyId, amount}
         }, transaction);
      }

      public Promise<Unit> SetCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null)
      {
         return GetCurrencies(currencyIdsToAmount.Keys.ToArray()).FlatMap(existingAmounts =>
         {
            var deltas = new Dictionary<string, long>();
            foreach (var kvp in currencyIdsToAmount)
            {
               var delta = kvp.Value;
               if (existingAmounts.TryGetValue(kvp.Key, out var existing))
               {
                  delta = kvp.Value - existing;
               }

               if (deltas.ContainsKey(kvp.Key))
               {
                  deltas[kvp.Key] = delta;
               }
               else
               {
                  deltas.Add(kvp.Key, delta);
               }
            }

            return AddCurrencies(deltas, transaction);
         });
      }

      public Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null)
      {
         var req = new InventoryUpdateRequest
         {
            transaction = transaction ?? Guid.NewGuid().ToString(),
            currencies = currencyIdsToAmount
         };
         return Requester.Request<EmptyResponse>(Method.PUT, $"{SERVICE_OBJECT}/{Ctx.UserId}", req).ToUnit();
      }

      public Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds)
      {
         return Requester.Request<InventoryView>(Method.GET, $"{SERVICE_OBJECT}/{Ctx.UserId}").Map(view =>
         {
            return view.currencies.ToDictionary(v => v.id, v => v.amount);
         });
      }

      public Promise<long> GetCurrency(string currencyId)
      {
         return GetCurrencies(new []{currencyId}).Map(all => {
            if (!all.TryGetValue(currencyId, out var result))
            {
               result = 0;
            }

            return result;
         });
      }
   }
}