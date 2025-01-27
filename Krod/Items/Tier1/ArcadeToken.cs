using RoR2;
using R2API;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Runtime.CompilerServices;

namespace Krod.Items.Tier1
{
    public static class ArcadeToken
    {
        public static void Awake()
        {
            KrodItems.ArcadeToken = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.ArcadeToken.canRemove = true;
            KrodItems.ArcadeToken.name = "ARCADE_TOKEN_NAME";
            KrodItems.ArcadeToken.nameToken = "ARCADE_TOKEN_NAME";
            KrodItems.ArcadeToken.pickupToken = "ARCADE_TOKEN_PICKUP";
            KrodItems.ArcadeToken.descriptionToken = "ARCADE_TOKEN_DESC";
            KrodItems.ArcadeToken.loreToken = "ARCADE_TOKEN_LORE";
            KrodItems.ArcadeToken.tags = [ItemTag.Utility];
            KrodItems.ArcadeToken._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            KrodItems.ArcadeToken.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            KrodItems.ArcadeToken.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/ArcadeToken.png");
            ItemAPI.Add(new CustomItem(KrodItems.ArcadeToken, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnPurchase(CostTypeDef.PayCostContext context)
        {
            CharacterMaster master = context.activatorMaster;
            ShopTerminalBehavior behavior = context.purchasedObject.GetComponent<ShopTerminalBehavior>();
            if (behavior && behavior.serverMultiShopController)
            {
                int remaining = (
                    from obj in behavior.serverMultiShopController.terminalGameObjects
                    where (obj.GetComponent<PurchaseInteraction>()?.Networkavailable ?? false)
                    select obj
                ).Count();
                if (remaining > 1)
                {
                    behavior.serverMultiShopController.SetCloseOnTerminalPurchase(context.purchasedObject.GetComponent<PurchaseInteraction>(), false);
                    master.inventory.RemoveItem(KrodItems.ArcadeToken);
                    PurchaseInteraction.CreateItemTakenOrb(context.activatorBody.gameObject.transform.position,
                        context.purchasedObject.gameObject,
                        KrodItems.ArcadeToken.itemIndex);
                    AkSoundEngine.PostEvent("KInsertToken", context.activatorBody.gameObject);
                }
            }
        }
    }
}
