using RoR2;
using R2API;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Runtime.CompilerServices;

namespace Krod.Items.Tier1
{
    public static class LooseCards
    {
        public class Behavior : CharacterBody.ItemBehavior
        {
            public DotController.DotIndex dotIndex;
            public void Awake()
            {
                enabled = false;
            }

            private void OnEnable()
            {
                if (!body) { return;  }
                Array x = Enum.GetValues(typeof(DotController.DotIndex));
                var y = (DotController.DotIndex)x.GetValue(UnityEngine.Random.Range(0, x.Length - 2));
                dotIndex = y;
                body.RemoveBuff(redBuff);
                body.RemoveBuff(greenBuff);
                body.RemoveBuff(blueBuff);
                body.RemoveBuff(orangeBuff);
                Log.Info(dotIndex);
                if (dotIndex is DotController.DotIndex.Burn or DotController.DotIndex.StrongerBurn)
                {
                    body.AddBuff(orangeBuff);
                }
                else if (dotIndex is DotController.DotIndex.Bleed or 
                    DotController.DotIndex.SuperBleed or 
                    DotController.DotIndex.Fracture)
                {
                    body.AddBuff(redBuff);
                }
                else if (dotIndex is DotController.DotIndex.Blight or DotController.DotIndex.Poison)
                {
                    body.AddBuff(greenBuff);
                }
                else
                {
                    body.AddBuff(blueBuff);
                }
            }

            private void OnDisable()
            {
                if (!body) { return; }
                dotIndex = DotController.DotIndex.None;
                body.RemoveBuff(redBuff);
                body.RemoveBuff(greenBuff);
                body.RemoveBuff(blueBuff);
                body.RemoveBuff(orangeBuff);
            }
        }

        public static BuffDef redBuff;
        public static BuffDef greenBuff;
        public static BuffDef blueBuff;
        public static BuffDef orangeBuff;
        public static GameObject cardHitSFX;

        public static void Awake()
        {
            KrodItems.LooseCards = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.LooseCards.canRemove = true;
            KrodItems.LooseCards.name = "LOOSE_CARDS_NAME";
            KrodItems.LooseCards.nameToken = "LOOSE_CARDS_NAME";
            KrodItems.LooseCards.pickupToken = "LOOSE_CARDS_PICKUP";
            KrodItems.LooseCards.descriptionToken = "LOOSE_CARDS_DESC";
            KrodItems.LooseCards.loreToken = "LOOSE_CARDS_LORE";
            KrodItems.LooseCards.tags = [ItemTag.Damage, ItemTag.CanBeTemporary];

            KrodItems.LooseCards._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            KrodItems.LooseCards.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/LooseCards.png");
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.LooseCards.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier1/LooseCards.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            KrodItems.LooseCards.requiredExpansion = KrodContent.expansionDef;
            ItemAPI.Add(new CustomItem(KrodItems.LooseCards, new ItemDisplayRuleDict(null)));

            redBuff = ScriptableObject.CreateInstance<BuffDef>();
            redBuff.isDebuff = false;
            redBuff.canStack = false;
            redBuff.name = "Loose Cards Red";
            redBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/LooseCardsRedBD.png");
            ContentAddition.AddBuffDef(redBuff);

            blueBuff = ScriptableObject.CreateInstance<BuffDef>();
            blueBuff.isDebuff = false;
            blueBuff.canStack = false;
            blueBuff.name = "Loose Cards Blue";
            blueBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/LooseCardsBlueBD.png");
            ContentAddition.AddBuffDef(blueBuff);

            greenBuff = ScriptableObject.CreateInstance<BuffDef>();
            greenBuff.isDebuff = false;
            greenBuff.canStack = false;
            greenBuff.name = "Loose Cards Green";
            greenBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/LooseCardsGreenBD.png");
            ContentAddition.AddBuffDef(greenBuff);

            orangeBuff = ScriptableObject.CreateInstance<BuffDef>();
            orangeBuff.isDebuff = false;
            orangeBuff.canStack = false;
            orangeBuff.name = "Loose Cards Orange";
            orangeBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/LooseCardsOrangeBD.png");
            ContentAddition.AddBuffDef(orangeBuff);

            cardHitSFX = KUtils.SFXEffect("Loose Cards Hit", "KCardHit");
            ContentAddition.AddEffect(cardHitSFX);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnHitEnemy(DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && (!damageInfo.rejected || !(damageInfo.procCoefficient <= 0)))
            {
                CharacterBody cb = damageInfo.attacker.GetComponent<CharacterBody>();
                if (cb && cb.master && cb.inventory)
                {
                    int c = cb.inventory.GetItemCountEffective(KrodItems.LooseCards);
                    if (c > 0)
                    {
                        float pct = 2.5f + (c * 2.5f);
                        if (Util.CheckRoll(pct, cb.master))
                        {
                            Behavior b = damageInfo.attacker.GetComponent<Behavior>();
                            if (b)
                            {
                                DotController.DotIndex idx = b.dotIndex;
                                InflictDotInfo dotInfo = new()
                                {
                                    dotIndex = idx,
                                    victimObject = victim,
                                    attackerObject = damageInfo.attacker,
                                    duration = 3f * damageInfo.procCoefficient
                                };
                                DotController.InflictDot(ref dotInfo);
                                if (UnityEngine.Random.Range(0, 100) < 10f)
                                {
                                    EffectManager.SpawnEffect(cardHitSFX, new()
                                    {
                                        origin = damageInfo.position,
                                        scale = 1f,
                                    }, true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
