using System.Collections.Generic;
using Beamable.Common.Api;
using Core.Platform.SDK;

namespace Beamable.Common.Inventory.Api
{
   public interface IInventoryApi : IBeamableApi
   {
      Promise<Unit> SetCurrency(string currencyId, long amount, string transaction=null);

      Promise<Unit> AddCurrency(string currencyId, long amount, string transaction=null);

      Promise<Unit> SetCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);
      Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);

      Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds);

      Promise<long> GetCurrency(string currencyId);

   }


   [System.Serializable]
   public class InventoryUpdateRequest
   {
      public string transaction; // will be set by api
      public Dictionary<string, long> currencies;
   }

   [System.Serializable]
   public class InventoryView
   {
      public List<CurrencyView> currencies;
   }

   [System.Serializable]
   public class CurrencyView
   {
      public string id;
      public long amount;
   }
}