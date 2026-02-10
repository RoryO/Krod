using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

namespace Krod.Items.Tier1
{
    public static class DiscountCoffee
    {
        public class Behavior : CharacterBody.ItemBehavior
        {
            public void Awake()
            {
                enabled = false;
            }

            public void OnEnable()
            {
                if (!body || body.HasBuff(buff) || !body.inventory) { return; }
                int c = body.inventory.GetItemCountEffective(KrodItems.DiscountCoffee);
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
        public static GameObject coffeeBeginEffect;
        public static GameObject coffeeEndEffect;
        public static void Awake()
        {
            KrodItems.DiscountCoffee = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.DiscountCoffee.canRemove = true;
            KrodItems.DiscountCoffee.name = "DISCOUNT_COFFEE_NAME";
            KrodItems.DiscountCoffee.nameToken = "DISCOUNT_COFFEE_NAME";
            KrodItems.DiscountCoffee.pickupToken = "DISCOUNT_COFFEE_PICKUP";
            KrodItems.DiscountCoffee.descriptionToken = "DISCOUNT_COFFEE_DESC";
            KrodItems.DiscountCoffee.loreToken = "DISCOUNT_COFFEE_LORE";
            KrodItems.DiscountCoffee.tags = [ItemTag.Utility, ItemTag.MobilityRelated, ItemTag.CanBeTemporary];
            KrodItems.DiscountCoffee._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            KrodItems.DiscountCoffee.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/DiscountCoffee.png");
#pragma warning disable CS0618
            KrodItems.DiscountCoffee.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier1/DiscountCoffee.prefab");
#pragma warning restore CS0618
            KrodItems.DiscountCoffee.requiredExpansion = KrodContent.expansionDef;
            ItemAPI.Add(new CustomItem(KrodItems.DiscountCoffee, new ItemDisplayRuleDict([])));

            buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.isDebuff = false;
            buff.canStack = false;
            buff.name = "Discount Coffee";
            buff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/DiscountCoffeeBD.png");
            ContentAddition.AddBuffDef(buff);

            {
                coffeeBeginEffect = new("Coffee Begin Effect",
                    typeof(EffectComponent),
                    typeof(VFXAttributes));
                Object.DontDestroyOnLoad(coffeeBeginEffect);
                EffectComponent ec = coffeeBeginEffect.GetComponent<EffectComponent>();
                ec.noEffectData = true;
                ec.soundName = "KCoffeeStart";
                ContentAddition.AddEffect(coffeeBeginEffect);
            }

            {
                coffeeEndEffect = new("Coffee End Effect",
                    typeof(EffectComponent),
                    typeof(VFXAttributes));
                Object.DontDestroyOnLoad(coffeeEndEffect);
                EffectComponent ec = coffeeEndEffect.GetComponent<EffectComponent>();
                ec.noEffectData = true;
                ec.soundName = "KCoffeeEnd";
                ContentAddition.AddEffect(coffeeEndEffect);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnInteractionBegin(PurchaseInteraction self, Interactor activator)
        {
            if (!NetworkServer.active || self.costType != CostTypeIndex.Money ) { return; }
            CharacterBody characterBody = activator.GetComponent<CharacterBody>();
            Inventory inventory = characterBody.inventory ? characterBody.inventory : null;
            if (characterBody && inventory)
            {
                int c = characterBody.inventory.GetItemCountEffective(KrodItems.DiscountCoffee);
                if (c > 0)
                {
                    if (!characterBody.HasBuff(buff))
                    {
                        EffectManager.SpawnEffect(coffeeBeginEffect, new EffectData()
                        {
                            origin = characterBody.gameObject.transform.position,
                            scale = 1f,
                        }, true);
                    }
                    characterBody.AddTimedBuff(buff, 50f + (c * 10f));
                }
            }
        }
    }
}
