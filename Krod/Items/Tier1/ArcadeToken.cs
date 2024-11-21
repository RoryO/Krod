using RoR2;
using R2API;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier1
{
    public class ArcadeToken
    {
        public static ItemDef def;

        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "ARCADE_TOKEN_NAME";
            def.nameToken = "ARCADE_TOKEN_NAME";
            def.pickupToken = "ARCADE_TOKEN_PICKUP";
            def.descriptionToken = "ARCADE_TOKEN_DESC";
            def.loreToken = "ARCADE_TOKEN_LORE";
            def.tags = [ItemTag.Utility];
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            def.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/ArcadeToken.png");
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            On.RoR2.Items.MultiShopCardUtils.OnPurchase += MultiShopCardUtils_OnPurchase;
        }

        private static void MultiShopCardUtils_OnPurchase(On.RoR2.Items.MultiShopCardUtils.orig_OnPurchase orig, CostTypeDef.PayCostContext context, int moneyCost)
        {
            CharacterMaster master = context.activatorMaster;
            if (master && 
                master.inventory && 
                master.inventory.GetItemCount(def) > 0 && 
                context.purchasedObject)
            {
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
                        master.inventory.RemoveItem(def);
                    }
                }
            }
            orig(context, moneyCost);
        }
    }
}
