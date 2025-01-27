using RoR2;
using R2API;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
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
                Array x = Enum.GetValues(typeof(DotController.DotIndex));
                var y = (DotController.DotIndex)x.GetValue(UnityEngine.Random.Range(0, x.Length - 2));
                dotIndex = y;
            }

            private void OnDisabled()
            {
                dotIndex = DotController.DotIndex.None;
            }
        }

        public static void Awake()
        {
            KrodItems.LooseCards = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.LooseCards.canRemove = true;
            KrodItems.LooseCards.name = "LOOSE_CARDS_NAME";
            KrodItems.LooseCards.nameToken = "LOOSE_CARDS_NAME";
            KrodItems.LooseCards.pickupToken = "LOOSE_CARDS_PICKUP";
            KrodItems.LooseCards.descriptionToken = "LOOSE_CARDS_DESC";
            KrodItems.LooseCards.loreToken = "LOOSE_CARDS_LORE";
            KrodItems.LooseCards.tags = [ItemTag.Damage];
            KrodItems.LooseCards._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            KrodItems.LooseCards.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/LooseCards.png");
            KrodItems.LooseCards.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.LooseCards, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnHitEnemy(DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && (!damageInfo.rejected || !(damageInfo.procCoefficient <= 0)))
            {
                CharacterBody cb = damageInfo.attacker.GetComponent<CharacterBody>();
                if (cb && cb.master && cb.inventory)
                {
                    int c = cb.inventory.GetItemCount(KrodItems.LooseCards);
                    if (c > 0)
                    {
                        float pct = 2.5f + (c * 2.5f);
                        if (Util.CheckRoll(pct, cb.master))
                        {
                            Behavior b = damageInfo.attacker.GetComponent<Behavior>();
                            if (b)
                            {
                                DotController.DotIndex idx = b.dotIndex;
                                InflictDotInfo dotInfo = new InflictDotInfo()
                                {
                                    dotIndex = idx,
                                    victimObject = victim,
                                    attackerObject = damageInfo.attacker,
                                    duration = 3f * damageInfo.procCoefficient
                                };
                                DotController.InflictDot(ref dotInfo);
                            }
                        }
                    }
                }
            }
        }
    }
}
