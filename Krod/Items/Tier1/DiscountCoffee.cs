using System;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using EntityStates.AffixVoid;
using System.Runtime.CompilerServices;

namespace Krod.Items.Tier1
{
    public static class DiscountCoffee
    {
        public class DiscountCoffeeBehavior : CharacterBody.ItemBehavior
        {
            public void Awake()
            {
                enabled = false;
            }
        

            public void OnEnable()
            {
                if (!body || !body.HasBuff(buff) || !body.inventory) { return; }
                int c = body.inventory.GetItemCount(KrodItems.DiscountCoffee);
                if (c > 0)
                {
                    body.AddTimedBuff(buff, 50f + (c * 10f));
                }
            }

            public void OnDisable()
            {
                if (body && body.HasBuff(buff))
                {
                    body.RemoveBuff(buff);
                }
            }
        }
        public static BuffDef buff;
        public static void Awake()
        {
            KrodItems.DiscountCoffee = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.DiscountCoffee.canRemove = true;
            KrodItems.DiscountCoffee.name = "DISCOUNT_COFFEE_NAME";
            KrodItems.DiscountCoffee.nameToken = "DISCOUNT_COFFEE_NAME";
            KrodItems.DiscountCoffee.pickupToken = "DISCOUNT_COFFEE_PICKUP";
            KrodItems.DiscountCoffee.descriptionToken = "DISCOUNT_COFFEE_DESC";
            KrodItems.DiscountCoffee.loreToken = "DISCOUNT_COFFEE_LORE";
            KrodItems.DiscountCoffee.tags = [ItemTag.Utility];
            KrodItems.DiscountCoffee._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            KrodItems.DiscountCoffee.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/DiscountCoffee.png");
            KrodItems.DiscountCoffee.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.DiscountCoffee, new ItemDisplayRuleDict(null)));
            buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.isDebuff = false;
            buff.canStack = false;
            buff.name = "Discount Coffee";
            buff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            ContentAddition.AddBuffDef(buff);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnInteractionBegin(PurchaseInteraction self, Interactor activator)
        {
            if (!NetworkServer.active || self.costType != CostTypeIndex.Money ) { return; }
            CharacterBody characterBody = activator.GetComponent<CharacterBody>();
            Inventory inventory = characterBody.inventory ? characterBody.inventory : null;
            if (characterBody && inventory)
            {
                int c = characterBody.inventory.GetItemCount(KrodItems.DiscountCoffee);
                if (c > 0)
                {
                    characterBody.AddTimedBuff(buff, 50f + (c * 10f));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender || !sender.inventory || !sender.HasBuff(buff)) { return; }
            int c = sender.inventory.GetItemCount(KrodItems.DiscountCoffee);
            args.attackSpeedMultAdd = c * 0.15f;
            args.sprintSpeedAdd = c * 0.25f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnInventoryChanged(CharacterBody self)
        {
            self.AddItemBehavior<DiscountCoffeeBehavior>(self.inventory.GetItemCount(KrodItems.DiscountCoffee));
        }
    }
}
