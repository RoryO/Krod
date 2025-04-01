using R2API;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2
{
    public static class ToyMotorcycle
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
            KrodItems.ToyMotorcycle.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier2/ToyMotorcycle.prefab");
            ItemAPI.Add(new CustomItem(KrodItems.ToyMotorcycle, new ItemDisplayRuleDict(null)));
        }
    }
}
