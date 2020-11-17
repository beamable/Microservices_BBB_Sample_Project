using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Pooling;
using Core.Serialization.SmallerJSON;

namespace Core.Platform.SDK.Inventory
{
   public class InventoryService : PlatformSubscribable<InventoryResponse, InventoryView>
   {
      public PlatformRequester Requester { get; }
      private InventoryView view = new InventoryView();

      public const string SERVICE_OBJECT = "/object/inventory";


      public InventoryService (PlatformService platform, PlatformRequester requester) : base(platform, requester, "inventory")
      {
         Requester = requester;
      }

      protected override void OnRefresh(InventoryResponse data)
      {
         data.MergeView(view);
         foreach (var key in view.currencies.Keys)
         {
            Notify(key, view);
         }

         foreach (var key in view.items.Keys)
         {
            Notify(key, view);
         }
      }

      public Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null)
      {
         var dbid = platform.User.id;

         // the default json serializer won't work for the dictionary, so lets do it ourselves...
         using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
         {
            var dict = new ArrayDict();
            dict.Add("transaction", transaction ?? Guid.NewGuid().ToString());
            dict.Add("currencies", currencyIdsToAmount);
            var json = Json.Serialize(dict, pooledBuilder.Builder);
            return Requester.Request<EmptyResponse>(Method.PUT, $"{SERVICE_OBJECT}/{dbid}", json).ToUnit();

         }

      }

      public Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds)
      {
         var dbid = platform.User.id;
         return Requester.Request<GetInventoryResponse>(Method.GET, $"{SERVICE_OBJECT}/{dbid}").Map(view =>
         {
            return view.currencies.ToDictionary(v => v.id, v => v.amount);
         });
      }

      [System.Serializable]
      public class GetInventoryResponse
      {
         public List<Currency> currencies;
      }
      [System.Serializable]
      public class InventoryUpdateRequest
      {
         public string transaction; // will be set by api
         public Dictionary<string, long> currencies;
      }

   }

   [Serializable]
   public class InventoryResponse
   {
      public List<Currency> currencies;
      public List<ItemGroup> items;

      public void MergeView(InventoryView view)
      {
         foreach (var currency in currencies)
         {
            view.currencies[currency.id] = currency.amount;
         }
         view.items.Clear();
         foreach (var itemGroup in items)
         {
            List<ItemView> itemList;
            if (!view.items.TryGetValue(itemGroup.id, out itemList))
            {
               itemList = new List<ItemView>();
               view.items[itemGroup.id] = itemList;
            }

            foreach (var item in itemGroup.items)
            {
               ItemView itemView = new ItemView();
               itemView.id = item.id;
               itemView.properties = new Dictionary<string, string>();
               foreach (var property in item.properties)
               {
                  itemView.properties[property.name] = property.value;
               }
               itemList.Add(itemView);
            }
         }
      }
   }

   [Serializable]
   public class Currency
   {
      public string id;
      public long amount;
   }

   [Serializable]
   public class Item
   {
      public string id;
      public List<ItemProperty> properties;
   }

   [Serializable]
   public class ItemGroup
   {
      public string id;
      public List<Item> items;
   }

   [Serializable]
   public class ItemProperty
   {
      public string name;
      public string value;
   }

   public class InventoryView
   {
      public Dictionary<string, long> currencies = new Dictionary<string, long>();
      public Dictionary<string, List<ItemView>> items = new Dictionary<string, List<ItemView>>();
   }

   public class ItemView
   {
      public string id;
      public Dictionary<string, string> properties;
   }
}