//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Beamable.Server.Clients
{
    using System;
    using Beamable.Platform.SDK;
    using Beamable.Server;
    
    
    /// <summary> A generated client for <see cref="Beamable.Server.BBBGameMicroservice.BBBGameMicroservice"/> </summary
    public sealed class BBBGameMicroserviceClient : Beamable.Server.MicroserviceClient
    {
        
        public BBBGameMicroserviceClient(Beamable.BeamContext context = null) : 
                base(context)
        {
        }
        
        /// <summary>
        /// Call the StartTheBattle method on the BBBGameMicroservice microservice
        /// <see cref="Beamable.Server.BBBGameMicroservice.BBBGameMicroservice.StartTheBattle"/>
        /// </summary>
        public Beamable.Common.Promise<Beamable.Server.BBBGameMicroservice.StartTheBattleResults> StartTheBattle(Beamable.Server.BBBGameMicroservice.Content.BossContentRef bossContentRef, int heroWeaponIndexMax)
        {
            string serialized_bossContentRef = this.SerializeArgument<Beamable.Server.BBBGameMicroservice.Content.BossContentRef>(bossContentRef);
            string serialized_heroWeaponIndexMax = this.SerializeArgument<int>(heroWeaponIndexMax);
            string[] serializedFields = new string[] {
                    serialized_bossContentRef,
                    serialized_heroWeaponIndexMax};
            return this.Request<Beamable.Server.BBBGameMicroservice.StartTheBattleResults>("BBBGameMicroservice", "StartTheBattle", serializedFields);
        }
        
        /// <summary>
        /// Call the AttackTheBoss method on the BBBGameMicroservice microservice
        /// <see cref="Beamable.Server.BBBGameMicroservice.BBBGameMicroservice.AttackTheBoss"/>
        /// </summary>
        public Beamable.Common.Promise<Beamable.Server.BBBGameMicroservice.AttackTheBossResults> AttackTheBoss(System.Collections.Generic.List<Beamable.Server.BBBGameMicroservice.Content.WeaponContentRef> weaponContentRefs)
        {
            string serialized_weaponContentRefs = this.SerializeArgument<System.Collections.Generic.List<Beamable.Server.BBBGameMicroservice.Content.WeaponContentRef>>(weaponContentRefs);
            string[] serializedFields = new string[] {
                    serialized_weaponContentRefs};
            return this.Request<Beamable.Server.BBBGameMicroservice.AttackTheBossResults>("BBBGameMicroservice", "AttackTheBoss", serializedFields);
        }
    }
    
    internal sealed class MicroserviceParametersBBBGameMicroserviceClient
    {
        
        [System.SerializableAttribute()]
        internal sealed class ParameterBeamable_Server_BBBGameMicroservice_Content_BossContentRef : Beamable.Server.MicroserviceClientDataWrapper<Beamable.Server.BBBGameMicroservice.Content.BossContentRef>
        {
        }
        
        [System.SerializableAttribute()]
        internal sealed class ParameterSystem_Int32 : Beamable.Server.MicroserviceClientDataWrapper<int>
        {
        }
        
        [System.SerializableAttribute()]
        internal sealed class ParameterSystem_Collections_Generic_List_Beamable_Server_BBBGameMicroservice_Content_WeaponContentRef : Beamable.Server.MicroserviceClientDataWrapper<System.Collections.Generic.List<Beamable.Server.BBBGameMicroservice.Content.WeaponContentRef>>
        {
        }
    }
    
    [BeamContextSystemAttribute()]
    internal static class ExtensionsForBBBGameMicroserviceClient
    {
        
        [Beamable.Common.Dependencies.RegisterBeamableDependenciesAttribute()]
        public static void RegisterService(Beamable.Common.Dependencies.IDependencyBuilder builder)
        {
            builder.AddScoped<BBBGameMicroserviceClient>();
        }
        
        public static BBBGameMicroserviceClient BBBGameMicroservice(this Beamable.Server.MicroserviceClients clients)
        {
            return clients.GetClient<BBBGameMicroserviceClient>();
        }
    }
}
