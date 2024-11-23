using BepInEx;
using Krod.Buffs;
using Krod.Equipment;
using Krod.Items.Tier1;
using Krod.Items.Tier2;
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
        public const string PluginVersion = "0.0.5";
        public static PluginInfo PInfo;

        public void Awake()
        {
            Log.Init(Logger);

            PInfo = Info;

            Assets.Init();
            Defs.Awake();
            
            LooseCards.Awake();
            MisterBoinky.Awake();
            ArcadeToken.Awake();
            DiscountCoffee.Awake();

            DoubleFish.Awake();
            Woodhat.Awake();
            TheExtra.Awake();

            TimsCrucible.Awake();
            //RorysForsight.Awake();

            JeffsServiceMedal.Awake();
            AileensGlassEye.Awake();
            AncientRecordingSystem.Awake();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F2)) { return; }
            // Get the player body to use a position:
            var t = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            // And then drop our defined item in front of the player.
            var testItem = AncientRecordingSystem.def.equipmentIndex;
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(testItem), t.position, t.forward * 20f);
        }
    }
}
