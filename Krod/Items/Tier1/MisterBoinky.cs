using R2API;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier1
{
    public static class MisterBoinky
    {
        public class Behavior : CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver 
        {
            public void Awake()
            {
                enabled = false;
            }
            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo != null && !damageInfo.rejected)
                {
                    CharacterBody body = GetComponent<CharacterBody>();
                    HealthComponent c = body.healthComponent;
                    if (!body || !c) { return; }

                    if ((damageInfo.damage / c.fullCombinedHealth) > 0.25f)
                    {
                        Log.Info("boinking");
                        Log.Info(damageInfo.damage);
                        Log.Info(c.fullCombinedHealth);
                        Log.Info($"{damageInfo.damage / c.fullCombinedHealth}");
                        damageInfo.rejected = true;
                        body.inventory.RemoveItemPermanent(KrodItems.MisterBoinky, 1);
                        body.inventory.GiveItemPermanent(KrodItems.MisterBoinkyConsumed, 1);
                        CharacterMasterNotificationQueue.SendTransformNotification(
                            body.master, 
                            KrodItems.MisterBoinky.itemIndex, 
                            KrodItems.MisterBoinkyConsumed.itemIndex, 
                            CharacterMasterNotificationQueue.TransformationType.Default);
                        body.AddTimedBuff(RoR2Content.Buffs.Immune, 5);
                    }
                }
            }
            
            private void OnEnable()
            {
                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i == -1)
                    {
                        HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
                    }
                }
            }

            private void OnDisable()
            {
                if (!body || !body.healthComponent) { return; }
                int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                if (i > -1)
                {
                    body.healthComponent.onIncomingDamageReceivers = body.healthComponent.onIncomingDamageReceivers.Where(val => (object)val != this).ToArray();
                }
            }
            
        }

        public static class Consumed
        {
            public static void Awake()
            {
                KrodItems.MisterBoinkyConsumed = ScriptableObject.CreateInstance<ItemDef>();
                KrodItems.MisterBoinkyConsumed.canRemove = false;
                KrodItems.MisterBoinkyConsumed.name = "MISTERBOINKY_USED_NAME";
                KrodItems.MisterBoinkyConsumed.nameToken = "MISTERBOINKY_USED_NAME";
                KrodItems.MisterBoinkyConsumed.pickupToken = "MISTERBOINKY_USED_PICKUP";
                KrodItems.MisterBoinkyConsumed.descriptionToken = "MISTERBOINKY_USED_DESC";
                KrodItems.MisterBoinkyConsumed.loreToken = "";
                KrodItems.MisterBoinkyConsumed.tags = [
                    ItemTag.CannotCopy,
                    ItemTag.CannotDuplicate,
                    ItemTag.CannotSteal,
                    ItemTag.AIBlacklist,
                    ItemTag.BrotherBlacklist,
                    ItemTag.WorldUnique,
                ];
                KrodItems.MisterBoinkyConsumed._itemTierDef = new ItemTierDef()
                {
                    isDroppable = false,
                    canScrap = true,
                    tier = ItemTier.NoTier,
                };
                KrodItems.MisterBoinkyConsumed.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/MisterBoinkyConsumed.png");
#pragma warning disable CS0618 // Type or member is obsolete
                KrodItems.MisterBoinkyConsumed.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
#pragma warning restore CS0618 // Type or member is obsolete
                ItemAPI.Add(new CustomItem(KrodItems.MisterBoinkyConsumed, new ItemDisplayRuleDict(null)));
            }
        }
        public static void Awake()
        {
            Consumed.Awake();
            KrodItems.MisterBoinky = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.MisterBoinky.canRemove = true;
            KrodItems.MisterBoinky.name = "MISTERBOINKY_NAME";
            KrodItems.MisterBoinky.nameToken = "MISTERBOINKY_NAME";
            KrodItems.MisterBoinky.pickupToken = "MISTERBOINKY_PICKUP";
            KrodItems.MisterBoinky.descriptionToken = "MISTERBOINKY_DESC";
            KrodItems.MisterBoinky.loreToken = "MISTERBOINKY_LORE";
            KrodItems.MisterBoinky.tags = [
                ItemTag.AIBlacklist
            ];
            KrodItems.MisterBoinky._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            KrodItems.MisterBoinky.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/MisterBoinky.png");
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.MisterBoinky.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier1/MisterBoinky.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            ItemAPI.Add(new CustomItem(KrodItems.MisterBoinky, new ItemDisplayRuleDict(null)));
        }
    }
}
