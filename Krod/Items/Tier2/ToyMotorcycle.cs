using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2
{
    public class ToyMotorcycle
    {
        public static ItemDef def;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "TOY_MOTORCYCLE_NAME";
            def.nameToken = "TOY_MOTORCYCLE_NAME";
            def.pickupToken = "TOY_MOTORCYCLE_PICKUP";
            def.descriptionToken = "TOY_MOTORCYCLE_DESC";
            def.loreToken = "TOY_MOTORCYCLE_LORE";
            def.tags = [ItemTag.Utility];
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            def.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/ToyMotorcycle.png");
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender || 
                !sender.inventory || 
                !sender.isSprinting) 
            { 
                return; 
            }
            int c = 0;
            foreach (var i in ItemCatalog.tier2ItemList)
            {
                c += sender.inventory.GetItemCount(i);
            }
            if (c == 0)
            {
                return;
            }
            int white = sender.inventory.GetItemCount(RoR2Content.Items.ScrapWhite);
            int green = sender.inventory.GetItemCount(RoR2Content.Items.ScrapGreen);
            int red = sender.inventory.GetItemCount(RoR2Content.Items.ScrapRed);
            int yellow = sender.inventory.GetItemCount(RoR2Content.Items.ScrapYellow);

            args.moveSpeedMultAdd = .05f + (c * .05f) + (white * 0.03f) + (green * .1f) + (red * 0.5f) + (yellow * 0.5f);
        }
    }
}
