using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2
{
    public static class Woodhat
    {
        public static ItemDef def;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "WOODHAT_NAME";
            def.nameToken = "WOODHAT_NAME";
            def.pickupToken = "WOODHAT_PICKUP";
            def.descriptionToken = "WOODHAT_DESC";
            def.loreToken = "WOODHAT_LORE";
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            def.tags = [ItemTag.Utility];
            def.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/WoodHat.png");
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            Inventory i = sender.inventory;
            if (i)
            {
                int c = i.GetItemCount(def);
                args.armorAdd += c * 10;
            }
        }
    }
}
