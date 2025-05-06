using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using RoR2.Achievements;

namespace Krod.Items.Tier2
{
    [RegisterAchievement(
        "FIND_MISTERBOINKY_REBORN",
        "KrodItems.MisterBoinkyReborn",
        null,
        10,
        null
    )]

    public class FoundBoinkyReborn : BaseAchievement
    {
        public Inventory currentInventory;

        public override void OnInstall()
        {
            base.OnInstall();
            localUser.onMasterChanged += UpdateCurrentInventory;
            UpdateCurrentInventory();
        }
        public override void OnUninstall()
        {
            localUser.onMasterChanged -= UpdateCurrentInventory;
            if (currentInventory)
            {
                currentInventory.onInventoryChanged -= OnInventoryChanged;
                currentInventory = null;
            }
            base.OnUninstall();
        }

        public void UpdateCurrentInventory()
        {
            Inventory inventory = null;
            if (localUser.cachedMasterController)
            {
                inventory = localUser.cachedMasterController.master.inventory;
            }
            if (inventory != currentInventory)
            {
                if (currentInventory)
                {
                    currentInventory.onInventoryChanged -= OnInventoryChanged;
                }
                currentInventory = inventory;
                if (currentInventory)
                {
                    currentInventory.onInventoryChanged += OnInventoryChanged;
                    OnInventoryChanged();
                }
            }
        }

        public void OnInventoryChanged()
        {
            if (currentInventory.GetItemCount(KrodItems.MisterBoinkyReborn) > 0)
            {
                Grant();
            }
        }
    }
    public static class MisterBoinkyReborn
    {
        public class RebornTracker : MonoBehaviour
        {
            public Run.FixedTimeStamp evolveAt;
        }
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
                        damageInfo.rejected = true;
                        Inventory inventory = body.inventory;
                        if (!inventory) { return; }
                        inventory.RemoveItem(KrodItems.MisterBoinkyReborn, 1);
                        inventory.GiveItem(RoR2Content.Items.ScrapGreen, 1);
                        CharacterMasterNotificationQueue.SendTransformNotification(
                            body.master, 
                            KrodItems.MisterBoinkyReborn.itemIndex, 
                            RoR2Content.Items.ScrapGreen.itemIndex,
                            CharacterMasterNotificationQueue.TransformationType.Default);
                        body.AddTimedBuff(RoR2Content.Buffs.Immune, 5);
                        if (inventory.GetItemCount(KrodItems.MisterBoinkyReborn) == 0)
                        {
                            RebornTracker t = body.master.gameObject.GetComponent<RebornTracker>();
                            if (t)
                            {
                                Destroy(t);
                            }
                        }
                    }
                }
            }

            public void OnEnable()
            {
                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i == -1)
                    {
                        HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
                    }
                }
                if (body.master)
                {
                    PlayerCharacterMasterController c = body.master.GetComponent<PlayerCharacterMasterController>();
                    if (c && c.networkUser)
                    {
                        PickupIndex idx = PickupCatalog.FindPickupIndex(KrodItems.MisterBoinkyReborn.itemIndex);
                        c.networkUser.localUser?.userProfile.DiscoverPickup(idx);
                    }
                    RebornTracker t = body.master.gameObject.GetComponent<RebornTracker>();
                    if (!t)
                    {
                        t = body.master.gameObject.AddComponent<RebornTracker>();
                        t.evolveAt = Run.FixedTimeStamp.now + 600f;
                    }
                }
            }

            public void OnDisable()
            {
                if (!body || !body.healthComponent) { return; }
                int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                if (i > -1)
                {
                    body.healthComponent.onIncomingDamageReceivers = body.healthComponent.onIncomingDamageReceivers.Where(val => (object)val != this).ToArray();
                }
            }

            public void FixedUpdate()
            {
                if (!body || !body.inventory || !body.master) { return; }
                int boinkyCount = body.inventory.GetItemCount(KrodItems.MisterBoinky);
                if (boinkyCount > 0)
                {
                    body.inventory.GiveItem(KrodItems.MisterBoinkyReborn, boinkyCount);
                    body.inventory.RemoveItem(KrodItems.MisterBoinky, boinkyCount);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinky.itemIndex,
                        KrodItems.MisterBoinkyReborn.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }

                RebornTracker t = body.master.gameObject.GetComponent<RebornTracker>();
                if (!t) { return; }
                if (Run.FixedTimeStamp.now > t.evolveAt)
                {
                    body.inventory.GiveItem(KrodItems.MisterBoinkyAscended, stack);
                    body.inventory.RemoveItem(KrodItems.MisterBoinkyReborn, stack);
                    Destroy(t);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinkyReborn.itemIndex,
                        KrodItems.MisterBoinkyAscended.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }
            }
        }

        public static UnlockableDef unlockableDef;
        public static void Awake()
        {
            KrodItems.MisterBoinkyReborn = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.MisterBoinkyReborn.canRemove = true;
            KrodItems.MisterBoinkyReborn.name = "MISTERBOINKY_REBORN_NAME";
            KrodItems.MisterBoinkyReborn.nameToken = "MISTERBOINKY_REBORN_NAME";
            KrodItems.MisterBoinkyReborn.pickupToken = "MISTERBOINKY_REBORN_PICKUP";
            KrodItems.MisterBoinkyReborn.descriptionToken = "MISTERBOINKY_REBORN_DESC";
            KrodItems.MisterBoinkyReborn.loreToken = "MISTERBOINKY_REBORN_LORE";
            KrodItems.MisterBoinkyReborn._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.MisterBoinkyReborn.tags = [ItemTag.WorldUnique];
            KrodItems.MisterBoinkyReborn.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/MisterBoinkyReborn.png");
            KrodItems.MisterBoinkyReborn.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = "KrodItems.MisterBoinkyReborn";
            unlockableDef.achievementIcon = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/MisterBoinkyRebornAD.png");
            ContentAddition.AddUnlockableDef(unlockableDef);
            KrodItems.MisterBoinkyReborn.unlockableDef = unlockableDef;
            ItemAPI.Add(new CustomItem(KrodItems.MisterBoinkyReborn, new ItemDisplayRuleDict(null)));
        }
    }
}
