using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Server.BBBGameMicroservice.Content;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beamable.Server.BBBGameMicroservice
{
   [Microservice("BBBGameMicroservice")]
   public class BBBGameMicroservice : Microservice
   {
      [ClientCallable]
      public async Task<StartTheBattleResults> StartTheBattle(BossContentRef bossContentRef, int heroWeaponIndexMax)
      {
         // Find the Boss data from the Boss content
         var boss = await bossContentRef.Resolve();

         // Set boss health to 100
         await BBBHelper.SetProtectedPlayerStat(Services, Context.UserId,
                BBBConstants.StatKeyBossHealth,
                boss.MaxHealth.ToString());

         // Set hero weapon index to random (0,1)
         int heroWeaponIndex = new System.Random().Next(heroWeaponIndexMax);

         await BBBHelper.SetProtectedPlayerStat(Services, Context.UserId,
                BBBConstants.StatKeyHeroWeaponIndex,
                heroWeaponIndex.ToString());

         return new StartTheBattleResults
         {
            BossHealthRemaining = boss.MaxHealth,
            HeroWeaponIndex = heroWeaponIndex
         };
      }

      [ClientCallable]
      public async Task<AttackTheBossResults> AttackTheBoss(List<WeaponContentRef> weaponContentRefs)
      {
         // Get the weapon index
         string heroWeaponIndexString = await BBBHelper.GetProtectedPlayerStat(Services, Context.UserId,
            BBBConstants.StatKeyHeroWeaponIndex);

         // Get the weapon
         int heroWeaponIndex = int.Parse(heroWeaponIndexString);

         // Find the weapon data from the Weapon content
         var weapon = await weaponContentRefs[heroWeaponIndex].Resolve();

         // Calculate the damage
         Random random = new Random();
         int damageAmount = 0;
         double hitRandom = random.NextDouble();

         //Console.WriteLine($"weaponData.hitChance={weapon.HitChance}.");
         //Console.WriteLine($"hitRandom={hitRandom}.");

         if (hitRandom <= weapon.HitChance)
         {
            damageAmount = random.Next(weapon.MinDamage, weapon.MaxDamage);
            //Console.WriteLine($"weapon.MinDamage={weaponData.MinDamage}.");
            //Console.WriteLine($"weapon.MaxDamage={weaponData.MaxDamage}.");
         }

         //Console.WriteLine($"damageAmount={damageAmount}.");

         // Return the damage
         return await DamageTheBoss(damageAmount);
      }

      public async Task<AttackTheBossResults> DamageTheBoss(int damageAmount)
      {
         // Get the health
         string bossHealthRemainingString = await BBBHelper.GetProtectedPlayerStat(Services, Context.UserId,
            BBBConstants.StatKeyBossHealth);

         int bossHealthRemaining = int.Parse(bossHealthRemainingString);

         // Decrement the health
         bossHealthRemaining = Math.Max(0, bossHealthRemaining - damageAmount);

         // Set the health
         await BBBHelper.SetProtectedPlayerStat(Services, Context.UserId,
             BBBConstants.StatKeyBossHealth,
             bossHealthRemaining.ToString());

         // Return the health
         return new AttackTheBossResults {
           DamageAmount = damageAmount,
           BossHealthRemaining = bossHealthRemaining
         };
      }
   }

   //TODO: Propose to add this to Beamable SDK API
   public static class BBBHelper
   {
      public static async Task<EmptyResponse> SetProtectedPlayerStat(IBeamableServices services, long userId, string key, string value)
      {
         return await services.Stats.SetProtectedPlayerStat(userId, key, value)
           .Error ((Exception exception) =>
            {
               BeamableLogger.Log($"SetProtectedPlayerStat() error={exception.Message}.");
            });
      }

      public static async Task<string> GetProtectedPlayerStat(IBeamableServices services, long userId, string key)
      {
         var stat = await services.Stats.GetProtectedPlayerStat(userId, key);

         if (!string.IsNullOrEmpty (stat))
         {
            return stat;
         }
         else
         {
            throw new System.Exception("GetProtectedPlayerStat() error.");
         }
      }
   }

   public static class BBBConstants
   {
      public const string StatKeyBossHealth = "BossHealth";
      public const string StatKeyHeroWeaponIndex = "HeroWeaponIndex";
   }

   public class AttackTheBossResults
   {
      public int DamageAmount;
      public int BossHealthRemaining;
   }

   public class StartTheBattleResults
   {
      public int BossHealthRemaining;
      public int HeroWeaponIndex;
   }
}