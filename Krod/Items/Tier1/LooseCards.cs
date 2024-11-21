using RoR2;
using R2API;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Tier1
{
    public class LooseCardsBehavior : CharacterBody.ItemBehavior
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

    public static class LooseCards
    {
        public static ItemDef def;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "LOOSE_CARDS_NAME";
            def.nameToken = "LOOSE_CARDS_NAME";
            def.pickupToken = "LOOSE_CARDS_PICKUP";
            def.descriptionToken = "LOOSE_CARDS_DESC";
            def.loreToken = "LOOSE_CARDS_LORE";
            def.tags = [ItemTag.Damage];
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            def.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/LooseCards.png");
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        public static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && (!damageInfo.rejected || !(damageInfo.procCoefficient <= 0)))
            {
                CharacterBody cb = damageInfo.attacker.GetComponent<CharacterBody>();
                if (cb && cb.master && cb.inventory)
                {
                    int c = cb.inventory.GetItemCount(def);
                    if (c > 0)
                    {
                        float pct = 2.5f + (c * 2.5f);
                        if (Util.CheckRoll(pct, cb.master))
                        {
                            LooseCardsBehavior b = damageInfo.attacker.GetComponent<LooseCardsBehavior>();
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
            orig(self, damageInfo, victim);
        }

        public static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (NetworkServer.active && self.inventory)
            {
                self.AddItemBehavior<LooseCardsBehavior>(self.inventory.GetItemCount(def));
            }
            orig(self);
        }
    }
}
