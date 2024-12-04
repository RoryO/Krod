using R2API;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2
{
    public class ToyMotorcycle
    {
        public static void Awake()
        {
            KrodItems.ToyMotorcycle = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.ToyMotorcycle.canRemove = true;
            KrodItems.ToyMotorcycle.name = "TOY_MOTORCYCLE_NAME";
            KrodItems.ToyMotorcycle.nameToken = "TOY_MOTORCYCLE_NAME";
            KrodItems.ToyMotorcycle.pickupToken = "TOY_MOTORCYCLE_PICKUP";
            KrodItems.ToyMotorcycle.descriptionToken = "TOY_MOTORCYCLE_DESC";
            KrodItems.ToyMotorcycle.loreToken = "TOY_MOTORCYCLE_LORE";
            KrodItems.ToyMotorcycle.tags = [ItemTag.Utility];
            KrodItems.ToyMotorcycle._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.ToyMotorcycle.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/ToyMotorcycle.png");
            KrodItems.ToyMotorcycle.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.ToyMotorcycle, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender || 
                !sender.inventory || 
                !sender.isSprinting ||
                sender.inventory.GetItemCount(KrodItems.ToyMotorcycle) == 0) 
            { 
                return; 
            }
            int c = 0;
            foreach (var i in ItemCatalog.tier2ItemList)
            {
                c += sender.inventory.GetItemCount(i);
            }
            int white = sender.inventory.GetItemCount(RoR2Content.Items.ScrapWhite);
            int green = sender.inventory.GetItemCount(RoR2Content.Items.ScrapGreen);
            int red = sender.inventory.GetItemCount(RoR2Content.Items.ScrapRed);
            int yellow = sender.inventory.GetItemCount(RoR2Content.Items.ScrapYellow);

            args.moveSpeedMultAdd = .05f + (c * .05f) + (white * 0.03f) + (green * .1f) + (red * 0.5f) + (yellow * 0.5f);
        }
    }
}
