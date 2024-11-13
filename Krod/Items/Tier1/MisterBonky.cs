using R2API;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Tier1
{
    public static class MisterBonky
    {
        public class MisterBonkyBehavior : CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver 
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
                    if (c == null || body == null) { return; }

                    if ((damageInfo.damage / c.fullCombinedHealth) > 0.25f)
                    {
                        damageInfo.rejected = true;
                        body.inventory.RemoveItem(def, 1);
                        body.inventory.GiveItem(Consumed.def, 1);
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
                if (body == null || body.healthComponent == null) { return; }
                int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                if (i > -1)
                {
                    body.healthComponent.onIncomingDamageReceivers = body.healthComponent.onIncomingDamageReceivers.Where(val => (object)val != this).ToArray();
                }
            }
            
        }

        public static class Consumed
        {
            public static ItemDef def;
            public static void Awake()
            {
                def = ScriptableObject.CreateInstance<ItemDef>();
                def.canRemove = false;
                def.name = "MISTERBONKY_USED_NAME";
                def.nameToken = "MISTERBONKY_USED_NAME";
                def.pickupToken = "MISTERBONKY_USED_PICKUP";
                def.descriptionToken = "MISTERBONKY_USED_DESC";
                def.loreToken = "";
                def.tier = ItemTier.NoTier;
                def._itemTierDef = new ItemTierDef()
                {
                    isDroppable = false,
                    canScrap = false,
                    tier = ItemTier.NoTier,
                };
                def.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
                def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
                ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            }
        }
        public static ItemDef def;
        public static void Awake()
        {
            Consumed.Awake();
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "MISTERBONKY_NAME";
            def.nameToken = "MISTERBONKY_NAME";
            def.pickupToken = "MISTERBONKY_PICKUP";
            def.descriptionToken = "MISTERBONKY_DESC";
            def.loreToken = "MISTERBONKY_LORE";
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            def.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (NetworkServer.active)
            {
                self.AddItemBehavior<MisterBonkyBehavior>(self.inventory.GetItemCount(def));
            }
            orig(self);
        }
    }
}
