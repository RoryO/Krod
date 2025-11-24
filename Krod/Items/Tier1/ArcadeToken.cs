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
        public static GameObject insertTokenEffect;
        public static void Awake()
        {
            KrodItems.ArcadeToken = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.ArcadeToken.canRemove = true;
            KrodItems.ArcadeToken.name = "ARCADE_TOKEN_NAME";
            KrodItems.ArcadeToken.nameToken = "ARCADE_TOKEN_NAME";
            KrodItems.ArcadeToken.pickupToken = "ARCADE_TOKEN_PICKUP";
            KrodItems.ArcadeToken.descriptionToken = "ARCADE_TOKEN_DESC";
            KrodItems.ArcadeToken.loreToken = "ARCADE_TOKEN_LORE";
            KrodItems.ArcadeToken.tags = [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CanBeTemporary];
            KrodItems.ArcadeToken._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.ArcadeToken.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier1/OldArcadeToken.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            KrodItems.ArcadeToken.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/ArcadeToken.png");
            ItemAPI.Add(new CustomItem(KrodItems.ArcadeToken, new ItemDisplayRuleDict(null)));

            insertTokenEffect = new("Token Use Effect", 
                typeof(EffectComponent), 
                typeof(VFXAttributes));
            Object.DontDestroyOnLoad(insertTokenEffect);
            EffectComponent ec = insertTokenEffect.GetComponent<EffectComponent>();
            ec.noEffectData = true;
            ec.soundName = "KInsertToken";
            ContentAddition.AddEffect(insertTokenEffect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnPurchase(CostTypeDef.PayCostContext context)
        {
            CharacterMaster master = context.activatorMaster;
            bool didPurchase = false;
            ShopTerminalBehavior shopBehavior = context.purchasedObject.GetComponent<ShopTerminalBehavior>();
            DroneVendorTerminalBehavior droneVendorBehavior = context.purchasedObject.GetComponent<DroneVendorTerminalBehavior>();
            if (shopBehavior && shopBehavior.serverMultiShopController)
            {
                int remaining = (
                    from obj in shopBehavior.serverMultiShopController.terminalGameObjects
                    where (obj.GetComponent<PurchaseInteraction>()?.Networkavailable ?? false)
                    select obj
                ).Count();
                if (remaining > 1)
                {
                    didPurchase = true;
                    shopBehavior.serverMultiShopController.SetCloseOnTerminalPurchase(
                        context.purchasedObject.GetComponent<PurchaseInteraction>(),
                        false);
                }
            }
            if (droneVendorBehavior && droneVendorBehavior.serverMultiShopController)
            {
                int remaining = (
                    from obj in droneVendorBehavior.serverMultiShopController._terminals
                    where (obj.hasBeenPurchased == false)
                    select obj
                ).Count();
                if (remaining > 1)
                {
                    didPurchase = true;
                    droneVendorBehavior.serverMultiShopController.SetCloseOnTerminalPurchase(
                        context.purchasedObject.GetComponent<PurchaseInteraction>(),
                        false);
                }
            }
            if (didPurchase)
            {
                if (master.inventory.GetItemCountTemp(KrodItems.ArcadeToken.itemIndex) > 0)
                {
                    master.inventory.RemoveItemTemp(KrodItems.ArcadeToken.itemIndex);
                }
                else
                {
                    master.inventory.RemoveItemPermanent(KrodItems.ArcadeToken);
                }
                PurchaseInteraction.CreateItemTakenOrb(context.activatorBody.gameObject.transform.position,
                    context.purchasedObject.gameObject,
                    KrodItems.ArcadeToken.itemIndex);
                EffectManager.SpawnEffect(insertTokenEffect, new()
                {
                    origin = context.activatorBody.gameObject.transform.position,
                    scale = 1f
                }, true);
            }
        }
    }
}
