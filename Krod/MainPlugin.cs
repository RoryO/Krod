using BepInEx;
using Krod.Equipment;
using Krod.Items.Boss;
using Krod.Items.Lunar;
using Krod.Items.Tier1;
using Krod.Items.Tier2;
using Krod.Items.Tier2.Void;
using Krod.Items.Tier3;
using R2API;
using RoR2;
using UnityEngine;

namespace Krod
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class MainPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Rory";
        public const string PluginName = "Krod";
        public const string PluginVersion = "0.0.29";
        public static PluginInfo PInfo;

        public void Awake()
        {
            Log.Init(Logger);

            PInfo = Info;

            Assets.Init();

            LooseCards.Awake();
            MisterBoinky.Awake();
            ArcadeToken.Awake();
            DiscountCoffee.Awake();
            WeightedDice.Awake();

            DoubleFish.Awake();
            Woodhat.Awake();
            TheExtra.Awake();
            ToyMotorcycle.Awake();
            NinjaShowerScrub.Awake();

            CaudalFin.Awake();
            PrismaticCoral.Awake();

            TimsCrucible.Awake();
            GodHand.Awake();
            RorysForsight.Awake();

            JeffsServiceMedal.Awake();
            AileensGlassEye.Awake();
            AncientRecordingSystem.Awake();
            JeremiahsAccident.Awake();

            ShipOfRegret.Awake();

#if DEBUG
            MisterBoinkyReborn.Awake();
            MisterBoinkyAscended.Awake();
            MisterBoinkyTranscended.Awake();
#endif

            Hooks.Awake();
#if DEBUG
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
#endif
        }

        private void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F2)) 
            { 
                // Get the player body to use a position:
                var t = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                // And then drop our defined item in front of the player.
                var testItem = KrodItems.RorysForesight.itemIndex;

                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(testItem), t.position, t.forward * 20f);
            }
#endif
        }
    }
}
