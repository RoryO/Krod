using R2API;
using RoR2;
using RoR2.Achievements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using System.ComponentModel.Design;

namespace Krod.Items.Tier3
{
    [RegisterAchievement(
        "FIND_MISTERBOINKY_ASCENDED",
        "KrodItems.MisterBoinkyAscended",
        null,
        10,
        null
    )]
    public class FoundBoinkyAscended : BaseAchievement
    {
        public Inventory currentInventory;
        public Run.FixedTimeStamp grantAt;

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
            if (currentInventory.GetItemCount(KrodItems.MisterBoinkyAscended) > 0)
            {
                // introduce a delay instead of granting right away
                // the stage transition hides the grant notification 
                grantAt = Run.FixedTimeStamp.now + 5f;
                RoR2Application.onFixedUpdate += FixedUpdate;
            }
        }

        public void FixedUpdate()
        {
            if (Run.FixedTimeStamp.now > grantAt)
            {
                Grant();
                RoR2Application.onFixedUpdate -= FixedUpdate;
            }
        }
    }
    public static class MisterBoinkyAscended
    {
        public class Behavior : CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver
        {
            public void Awake()
            {
                enabled = false;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo != null)
                {
                    CharacterBody body = GetComponent<CharacterBody>();
                    if (!body) { return; }
                    if (damageInfo.rejected)
                    {
                        body.AddBuff(buffDef);
                        return;
                    }
                    HealthComponent c = body.healthComponent;
                    if (!c) { return; }
                    if (!damageInfo.rejected && (damageInfo.damage / c.fullCombinedHealth) > 0.25f)
                    {
                        damageInfo.rejected = true;
                        body.AddTimedBuff(RoR2Content.Buffs.Immune, 5 * stack);
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
                        PickupIndex idx = PickupCatalog.FindPickupIndex(KrodItems.MisterBoinkyAscended.itemIndex);
                        c.networkUser.localUser?.userProfile.DiscoverPickup(idx);
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
            public void Update()
            {
                if (!body || !body.inventory) { return; }
                int boinkyCount = body.inventory.GetItemCount(KrodItems.MisterBoinky);
                int rebornCount = body.inventory.GetItemCount(KrodItems.MisterBoinkyReborn);
                if (boinkyCount > 0)
                {
                    body.inventory.GiveItem(KrodItems.MisterBoinkyAscended, boinkyCount);
                    body.inventory.RemoveItem(KrodItems.MisterBoinky, boinkyCount);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinky.itemIndex,
                        KrodItems.MisterBoinkyAscended.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }
                if (rebornCount > 0)
                {
                    body.inventory.GiveItem(KrodItems.MisterBoinkyAscended, rebornCount);
                    body.inventory.RemoveItem(KrodItems.MisterBoinkyReborn, rebornCount);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinkyReborn.itemIndex,
                        KrodItems.MisterBoinkyAscended.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }
            }
        }

        public static BuffDef buffDef;
        public static UnlockableDef unlockableDef;
        public static void Awake()
        {
            KrodItems.MisterBoinkyAscended = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.MisterBoinkyAscended.canRemove = true;
            KrodItems.MisterBoinkyAscended.name = "MISTERBOINKY_ASCENDED_NAME";
            KrodItems.MisterBoinkyAscended.nameToken = "MISTERBOINKY_ASCENDED_NAME";
            KrodItems.MisterBoinkyAscended.pickupToken = "MISTERBOINKY_ASCENDED_PICKUP";
            KrodItems.MisterBoinkyAscended.descriptionToken = "MISTERBOINKY_ASCENDED_DESC";
            KrodItems.MisterBoinkyAscended.loreToken = "MISTERBOINKY_ASCENDED_LORE";
            KrodItems.MisterBoinkyAscended._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            KrodItems.MisterBoinkyAscended.tags = [ItemTag.WorldUnique];
            KrodItems.MisterBoinkyAscended.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/MisterBoinkyAscended.png");
            KrodItems.MisterBoinkyAscended.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = "KrodItems.MisterBoinkyAscended";
            unlockableDef.achievementIcon = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/MisterBoinkyAscendedAD.png");
            ContentAddition.AddUnlockableDef(unlockableDef);
            KrodItems.MisterBoinkyAscended.unlockableDef = unlockableDef;
            ItemAPI.Add(new CustomItem(KrodItems.MisterBoinkyAscended, new ItemDisplayRuleDict(null)));

            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.isDebuff = false;
            buffDef.canStack = true;
            buffDef.name = "Boinky Requital";
            buffDef.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/MisterBoinkyAscended.png");
            ContentAddition.AddBuffDef(buffDef);
        }
    }
}
