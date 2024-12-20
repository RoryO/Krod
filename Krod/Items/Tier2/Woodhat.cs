using R2API;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2
{
    public static class Woodhat
    {
        public static void Awake()
        {
            KrodItems.Woodhat = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.Woodhat.canRemove = true;
            KrodItems.Woodhat.name = "WOODHAT_NAME";
            KrodItems.Woodhat.nameToken = "WOODHAT_NAME";
            KrodItems.Woodhat.pickupToken = "WOODHAT_PICKUP";
            KrodItems.Woodhat.descriptionToken = "WOODHAT_DESC";
            KrodItems.Woodhat.loreToken = "WOODHAT_LORE";
            KrodItems.Woodhat._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.Woodhat.tags = [ItemTag.Utility];
            KrodItems.Woodhat.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/WoodHat.png");
            KrodItems.Woodhat.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.Woodhat, new ItemDisplayRuleDict(null)));
        }
    }
}
