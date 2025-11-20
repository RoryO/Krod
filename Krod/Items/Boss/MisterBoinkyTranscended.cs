using R2API;
using RoR2;
using RoR2.Achievements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Krod.Items.Boss
{
    [RegisterAchievement(
        "FIND_MISTERBOINKY_TRANSCENDED",
        "KrodItems.MisterBoinkyTranscended",
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
            if (currentInventory.GetItemCountEffective(KrodItems.MisterBoinkyTranscended) > 0)
            {
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
    public static class MisterBoinkyTranscended
    {
        public class Behavior : CharacterBody.ItemBehavior, IOnIncomingDamageServerReceiver
        {
            public float tranquilityStopwatch = 0;
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
                    if (!body || !body.inventory || !c) { return; }

                    if ((damageInfo.damage / c.fullCombinedHealth) > 0.25f)
                    {
                        damageInfo.rejected = true;
                        body.AddTimedBuff(RoR2Content.Buffs.Immune, 10 * stack);
                        body.AddTimedBuff(radiantBuff, 10 * stack);
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
                        PickupIndex idx = PickupCatalog.FindPickupIndex(KrodItems.MisterBoinkyTranscended.itemIndex);
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
                int boinkyCount = body.inventory.GetItemCountEffective(KrodItems.MisterBoinky);
                int rebornCount = body.inventory.GetItemCountEffective(KrodItems.MisterBoinkyReborn);
                int ascendedCount = body.inventory.GetItemCountEffective(KrodItems.MisterBoinkyAscended);
                if (boinkyCount > 0)
                {
                    body.inventory.GiveItemPermanent(KrodItems.MisterBoinkyTranscended, boinkyCount);
                    body.inventory.RemoveItemPermanent(KrodItems.MisterBoinky, boinkyCount);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinky.itemIndex,
                        KrodItems.MisterBoinkyTranscended.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }
                if (rebornCount > 0)
                {
                    body.inventory.GiveItemPermanent(KrodItems.MisterBoinkyTranscended, rebornCount);
                    body.inventory.RemoveItemPermanent(KrodItems.MisterBoinkyReborn, rebornCount);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinkyReborn.itemIndex,
                        KrodItems.MisterBoinkyTranscended.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }
                if (ascendedCount > 0)
                {
                    body.inventory.GiveItemPermanent(KrodItems.MisterBoinkyTranscended, ascendedCount);
                    body.inventory.RemoveItemPermanent(KrodItems.MisterBoinkyAscended, ascendedCount);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinkyAscended.itemIndex,
                        KrodItems.MisterBoinkyTranscended.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }
                tranquilityStopwatch += Time.deltaTime;
                if (tranquilityStopwatch > 1f)
                {
                    tranquilityStopwatch = 0;
                    if (body.HasBuff(radiantBuff)) {
                        SphereSearch sphereSearch = new()
                        {
                            mask = LayerIndex.entityPrecise.mask,
                            origin = body.transform.position,
                            radius = 30,
                            queryTriggerInteraction = QueryTriggerInteraction.Collide
                        };
                        List<Collider> results = [];
                        sphereSearch.RefreshCandidates()
                            .FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex))
                            .FilterCandidatesByDistinctHurtBoxEntities()
                            .GetColliders(results);
                        for (int i = 0; i < results.Count; ++i)
                        {
                            CharacterBody found = Util.HurtBoxColliderToBody(results[i]);
                            GameObject gameObject = (found ? found.gameObject : null);
                            if (!gameObject ||
                                body.gameObject == gameObject ||
                                found.teamComponent.teamIndex == body.teamComponent.teamIndex)
                            {
                                continue;
                            }
                            found.AddBuff(tranquilityBuff);
                            int c = found.GetBuffCount(tranquilityBuff);
                            if (c > 10 && !found.isBoss)
                            {
                                found.master.TrueKill();
                            }
                            else if (found.isBoss && c > 30)
                            {
                                found.master.TrueKill();
                            }
                        }
                    }
                }
            }
        }

        public static BuffDef radiantBuff;
        public static BuffDef tranquilityBuff;
        public static UnlockableDef unlockableDef;
        public static void Awake()
        {
            KrodItems.MisterBoinkyTranscended = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.MisterBoinkyTranscended.canRemove = true;
            KrodItems.MisterBoinkyTranscended.name = "MISTERBOINKY_TRANSCENDED_NAME";
            KrodItems.MisterBoinkyTranscended.nameToken = "MISTERBOINKY_TRANSCENDED_NAME";
            KrodItems.MisterBoinkyTranscended.pickupToken = "MISTERBOINKY_TRANSCENDED_PICKUP";
            KrodItems.MisterBoinkyTranscended.descriptionToken = "MISTERBOINKY_TRANSCENDED_DESC";
            KrodItems.MisterBoinkyTranscended.loreToken = "MISTERBOINKY_TRANSCENDED_LORE";
            KrodItems.MisterBoinkyTranscended._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/BossTierDef.asset").WaitForCompletion();
            KrodItems.MisterBoinkyTranscended.tags = [ItemTag.WorldUnique];
            KrodItems.MisterBoinkyTranscended.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Boss/MisterBoinkyTranscended.png");
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.MisterBoinkyTranscended.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
#pragma warning restore CS0618 // Type or member is obsolete
            unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = "KrodItems.MisterBoinkyTranscended";
            unlockableDef.achievementIcon = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Boss/MisterBoinkyTranscendedAD.png");
            ContentAddition.AddUnlockableDef(unlockableDef);
            KrodItems.MisterBoinkyTranscended.unlockableDef = unlockableDef;
            ItemAPI.Add(new CustomItem(KrodItems.MisterBoinkyTranscended, new ItemDisplayRuleDict(null)));

            radiantBuff = ScriptableObject.CreateInstance<BuffDef>();
            radiantBuff.isDebuff = false;
            radiantBuff.canStack = false;
            radiantBuff.name = "Radiant";
            radiantBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Boss/RadiantBD.png");
            ContentAddition.AddBuffDef(radiantBuff);

            tranquilityBuff = ScriptableObject.CreateInstance<BuffDef>();
            tranquilityBuff.isDebuff = false;
            tranquilityBuff.canStack = true;
            tranquilityBuff.name = "Sedatum";
            tranquilityBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Boss/TranquilityBD.png");
            ContentAddition.AddBuffDef(tranquilityBuff);
        }
    }
}
