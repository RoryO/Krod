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
        public const string PluginVersion = "0.0.2";

        public void Awake()
        {
            Log.Init(Logger);
            Defs.Awake();
            
            LooseCards.Awake();
            MisterBoinky.Awake();

            DoubleFish.Awake();
            Woodhat.Awake();

            TimsCrucible.Awake();
            //RorysForsight.Awake();

            JeffsServiceMedal.Awake();
            AileensGlassEye.Awake();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                // And then drop our defined item in front of the player.
                var testItem = AileensGlassEye.def.equipmentIndex;
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(testItem), transform.position, transform.forward * 20f);
            }
        }
    }
}
