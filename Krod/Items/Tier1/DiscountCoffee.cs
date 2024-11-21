using System;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using EntityStates.AffixVoid;

namespace Krod.Items.Tier1
{
    public class DiscountCoffee
    {
        public class DiscountCoffeeBehavior : CharacterBody.ItemBehavior
        {
            public void Awake()
            {
                enabled = false;
            }
        

            public void OnEnable()
            {
                if (body?.HasBuff(buff) ?? false) { return; }
                int c = body?.inventory?.GetItemCount(def) ?? 0;
                if (c > 0)
                {
                    body?.AddTimedBuff(buff, 50f + (c * 10f));
                }
            }

            public void OnDisable()
            {
                if (body?.HasBuff(buff) ?? false)
                {
                    body.RemoveBuff(buff);
                }
            }
        }
        public static ItemDef def;
        public static BuffDef buff;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "DISCOUNT_COFFEE_NAME";
            def.nameToken = "DISCOUNT_COFFEE_NAME";
            def.pickupToken = "DISCOUNT_COFFEE_PICKUP";
            def.descriptionToken = "DISCOUNT_COFFEE_DESC";
            def.loreToken = "DISCOUNT_COFFEE_LORE";
            def.tags = [ItemTag.Utility];
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            def.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/DiscountCoffee.png");
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.isDebuff = false;
            buff.canStack = false;
            buff.name = "Discount Coffee";
            buff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            ContentAddition.AddBuffDef(buff);
        }

        public static void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (!NetworkServer.active || self.costType != CostTypeIndex.Money ) { return; }
            CharacterBody characterBody = activator.GetComponent<CharacterBody>();
            int c = characterBody?.inventory?.GetItemCount(def) ?? 0;
            if (c > 0)
            {
                characterBody?.AddTimedBuff(buff, 50f + (c * 10f));
            }
        }

        public static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.HasBuff(buff)) { return; }
            int c = sender?.inventory?.GetItemCount(def) ?? 0;
            args.attackSpeedMultAdd = c * 0.15f;
            args.sprintSpeedAdd = c * 0.25f;
        }

        public static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active && self)
            {
                int c = self?.inventory?.GetItemCount(def) ?? 0;
                self.AddItemBehavior<DiscountCoffeeBehavior>(c);
            }
        }
    }
}
