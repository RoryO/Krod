using RoR2;
using R2API;
using Krod.Equipment;
using Krod.Items.Tier1;
using UnityEngine.Networking;
using Krod.Items.Tier2;
using Krod.Items.Tier3;
using Krod.Items.Tier2.Void;
using UnityEngine;
using Krod.Items.Lunar;
using Krod.Items.Boss;
using UnityEngine.AddressableAssets;
using System;

namespace Krod
{
    public static class Hooks
    {
        public static void Awake()
        {
            On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;

            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnEquipmentGained += CharacterBody_OnEquipmentGained;
            On.RoR2.CharacterBody.OnEquipmentLost += CharacterBody_OnEquipmentLost;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.CharacterBody.OnTakeDamageServer += CharacterBody_OnTakeDamageServer;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += CharacterMaster_GetDeployableSameSlotLimit;
            On.RoR2.CharacterMaster.GiveMoney += CharacterMaster_GiveMoney;

            On.RoR2.DeathRewards.OnKilledServer += DeathRewards_OnKilledServer;

            On.RoR2.DotController.OnDotStackAddedServer += DotController_OnDotStackAddedServer;
            On.RoR2.DotController.OnDotStackRemovedServer += DotController_OnDotStackRemovedServer;

            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;

            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.GlobalEventManager.ServerDamageDealt += GlobalEventManager_ServerDamageDealt;

            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;

            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += PurchaseInteraction_CanBeAffordedByInteractor;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;

            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;

            On.RoR2.SceneExitController.Begin += SceneExitController_Begin;

            On.RoR2.ShrineColossusAccessBehavior.OnInteraction += ShrineColossusAccessBehavior_OnInteraction;

            On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
            On.RoR2.ShrineChanceBehavior.FixedUpdate += ShrineChanceBehavior_FixedUpdate;

            On.RoR2.ShrineBloodBehavior.AddShrineStack += ShrineBloodBehavior_AddShrineStack;

            On.RoR2.ShopTerminalBehavior.GetInfo += ShopTerminalBehavior_GetInfo;

            On.RoR2.Items.MultiShopCardUtils.OnPurchase += MultiShopCardUtils_OnPurchase;

            On.RoR2.UI.PingIndicator.RebuildPing += PingIndicator_RebuildPing;

            RoR2Application.onLoad += Ror2Application_onLoad;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

#if DEBUG
            On.RoR2.RoR2Application.UpdateCursorState += RoR2Application_UpdateCursorState;
            On.RoR2.MPEventSystemManager.Update += MPEventSystemManager_Update;
#endif
        }

        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo == null || (damageInfo.damage > 0 && !damageInfo.rejected))
            {
                Woodhat.Behavior b = self.body.GetComponent<Woodhat.Behavior>();
                if (b && b.ready)
                {
                    int woodArmor = self.body.GetBuffCount(Woodhat.buffDef);
                    if (woodArmor > 0)
                    {
                        float d = damageInfo.damage;
                        float n = d * (float)0.5;
                        damageInfo.damage = n;
                        self.body.RemoveBuff(Woodhat.buffDef);
                    }
                    b.ready = false;
                }
            }
            orig(self, damageInfo);
        }

        private static void ShrineColossusAccessBehavior_OnInteraction(On.RoR2.ShrineColossusAccessBehavior.orig_OnInteraction orig, ShrineColossusAccessBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            CharacterBody body = interactor.GetComponent<CharacterBody>();
            if (body && body.inventory)
            {
                int c = body.inventory.GetItemCountEffective(KrodItems.MisterBoinkyConsumed);
                if (c > 0)
                {
                    body.inventory.RemoveItemPermanent(KrodItems.MisterBoinkyConsumed, c);
                    body.inventory.GiveItemPermanent(KrodItems.MisterBoinkyReborn.itemIndex, c);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinkyConsumed.itemIndex,
                        KrodItems.MisterBoinkyReborn.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                    PurchaseInteraction.CreateItemTakenOrb(body.transform.position, 
                        self.gameObject, 
                        KrodItems.MisterBoinkyConsumed.itemIndex);
                }
            }
        }

        private static void ShrineBloodBehavior_AddShrineStack(On.RoR2.ShrineBloodBehavior.orig_AddShrineStack orig, ShrineBloodBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            CharacterBody body = interactor.GetComponent<CharacterBody>();
            if (body && body.inventory)
            {
                int c = body.inventory.GetItemCountEffective(KrodItems.MisterBoinkyAscended);
                if (c > 0)
                {
                    body.inventory.RemoveItemTemp(KrodItems.MisterBoinkyAscended.itemIndex, c);
                    body.inventory.GiveItemPermanent(KrodItems.MisterBoinkyTranscended, (int)Math.Ceiling(c / 2.0));
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        body.master,
                        KrodItems.MisterBoinkyAscended.itemIndex,
                        KrodItems.MisterBoinkyTranscended.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                    PurchaseInteraction.CreateItemTakenOrb(body.transform.position, 
                        self.gameObject, 
                        KrodItems.MisterBoinkyAscended.itemIndex);
                }
            }
        }

        private static void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            // Similar to lockbox spawn
            if (!SceneInfo.instance.countsAsStage && !SceneInfo.instance.sceneDef.allowItemsToSpawnObjects)
            {
                return;
            }
            bool maybeCreateReviveShrine = false;
            bool maybeCreateBloodShrine = false;
            for (int i = 0; i < CharacterMaster.readOnlyInstancesList.Count; i++)
            {
                CharacterMaster cm = CharacterMaster.readOnlyInstancesList[i];
                if (!cm.inventory) { return; }
                if (cm.inventory.GetItemCountEffective(KrodItems.MisterBoinkyConsumed) > 0)
                {
                    maybeCreateReviveShrine = true;
                    break;
                }
                if (cm.inventory.GetItemCountEffective(KrodItems.MisterBoinkyAscended) > 0)
                {
                    maybeCreateBloodShrine = true;
                    break;
                }

            }
            if (maybeCreateReviveShrine)
            {
                SpawnCard sc = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC2/iscShrineColossusAccess.asset").WaitForCompletion();
                if (!sc)
                {
                    Log.Error("no rebirth shrine isc");
                }
                ShrineColossusAccessBehavior existingShrine = InstanceTracker.FirstOrNull<ShrineColossusAccessBehavior>();
                if (sc && existingShrine == null)
                {
                    DirectorPlacementRule dpr = new()
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    };
                    DirectorSpawnRequest dsc = new(sc, dpr, self.rng);
                    DirectorCore.instance.TrySpawnObject(dsc);
                }
            }
            if (maybeCreateBloodShrine)
            {
                SpawnCard sc = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/ShrineBlood/iscShrineBlood.asset").WaitForCompletion();
                if (!sc)
                {
                    Log.Error("no blood shrine isc");
                }
                ShrineBloodBehavior existingShrine = InstanceTracker.FirstOrNull<ShrineBloodBehavior>();
                if (sc && existingShrine == null)
                {
                    DirectorPlacementRule dpr = new()
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    };
                    DirectorSpawnRequest dsc = new(sc, dpr, self.rng);
                    DirectorCore.instance.TrySpawnObject(dsc);
                }
            }
        }

        private static void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == WeightedDice.addLuckBuff && self.isPlayerControlled)
            {
                Util.PlaySound("KDiceSuccess", self.gameObject);
            }
            if (buffDef == WeightedDice.removeLuckBuff && self.isPlayerControlled)
            {
                Util.PlaySound("KDiceFail", self.gameObject);
            }
        }

        private static void ShrineChanceBehavior_FixedUpdate(On.RoR2.ShrineChanceBehavior.orig_FixedUpdate orig, ShrineChanceBehavior self)
        {
            var b = self.GetComponent<RorysForsight.ChanceShrineForsightBehavior>();
            if (b && self.waitingForRefresh)
            {
                self.refreshTimer -= Time.fixedDeltaTime;
                if (self.refreshTimer <= 0 && self.successfulPurchaseCount < self.maxPurchaseCount)
                {
                    self.purchaseInteraction.SetAvailable(true);
                    self.purchaseInteraction.Networkcost = b.revealedCosts[b.lastPurchasedIndex].cost;
                    self.waitingForRefresh = false;
                }
            }
            else
            {
                orig(self);
            }
        }

        private static void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            var b = self.GetComponent<RorysForsight.ChanceShrineForsightBehavior>();
            if (b)
            {
                RorysForsight.AddShrineStack(self, activator, b);
            }
            else
            {
                orig(self, activator);
            }
        }

        private static void MPEventSystemManager_Update(On.RoR2.MPEventSystemManager.orig_Update orig, MPEventSystemManager self)
        {
            orig(self);
            if (Cursor.lockState == CursorLockMode.Confined)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private static void RoR2Application_UpdateCursorState(On.RoR2.RoR2Application.orig_UpdateCursorState orig)
        {
            orig();
            if (Cursor.lockState == CursorLockMode.Confined)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (NetworkServer.active)
            {
                if (buffDef == RorysForsight.cooldownBuff &&
                   self &&
                   self.inventory &&
                   self.inventory.GetItemCountEffective(KrodItems.RorysForesight) > 0)
                {
                    self.AddBuff(RorysForsight.isAvailableBuff);
                }
                if (buffDef == WeightedDice.addLuckBuff ||
                    buffDef == WeightedDice.removeLuckBuff)
                {
                    WeightedDice.Behavior b = self.GetComponent<WeightedDice.Behavior>();
                    if (b)
                    {
                        b.Reroll();
                    }
                }
                if (buffDef == DiscountCoffee.buff)
                {
                    EffectManager.SpawnEffect(DiscountCoffee.coffeeEndEffect, new EffectData()
                    {
                        origin = self.gameObject.transform.position,
                        scale = 1f,
                    }, true);
                }
            }
        }

        private static void PingIndicator_RebuildPing(On.RoR2.UI.PingIndicator.orig_RebuildPing orig, RoR2.UI.PingIndicator self)
        {
            orig(self);
            RorysForsight.RevealContents(self);
        }

        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (!self || !self.master) { return; }
            self.master.luck = 0;
            self.master.luck += self.inventory.GetItemCountEffective(RoR2Content.Items.Clover);
            self.master.luck -= self.inventory.GetItemCountEffective(RoR2Content.Items.LunarBadLuck);
            if (self.HasBuff(WeightedDice.addLuckBuff))
            {
                self.master.luck += 1;
            }

            if (self.HasBuff(WeightedDice.removeLuckBuff))
            {
                self.master.luck -= 1;
            }
        }

        private static void SceneExitController_Begin(On.RoR2.SceneExitController.orig_Begin orig, SceneExitController self)
        {
            if (NetworkServer.active)
            {
                bool shouldPreventExit = false;
                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
                {
                    CharacterBody body = PlayerCharacterMasterController.instances[i].body;
                    if (body && body.HasBuff(ShipOfRegret.buffDef))
                    {
                        Chat.SimpleChatMessage msg = new()
                        {
                            baseToken = "Cannot leave, {0} holds regret",
                            paramTokens = [
                                Util.EscapeRichTextForTextMeshPro(
                                    Util.GetBestBodyName(body.gameObject)
                                )
                            ]
                        };
                        Chat.SendBroadcastChat(msg);
                        shouldPreventExit = true;
                    }
                }
                if (shouldPreventExit)
                {
                    self.SetState(SceneExitController.ExitState.Idle);
                    GenericInteraction genericInteraction = self.GetComponent<GenericInteraction>();
                    if (genericInteraction)
                    {
                        genericInteraction.SetInteractabilityAvailable();
                    }
                    return;
                }
                else { orig(self); }
            }
            else { orig(self); }
        }

        private static void DeathRewards_OnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport)
        {
            CharacterBody body = damageReport.attackerBody;
            if (!body)
            {
                orig(self, damageReport);
                return;
            }
            Inventory inventory = body.inventory;
            if (inventory)
            {
                int c = inventory.GetItemCountEffective(KrodItems.ShipOfRegret);
                ShipOfRegret.Behavior b = body.GetComponent<ShipOfRegret.Behavior>();
                if (c == 0 || !b)
                {
                    orig(self, damageReport);
                    return;
                }
                if (damageReport.victimBody.isElite)
                {
                    self.goldReward = (uint)Mathf.CeilToInt(self.goldReward * c * 3);
                }
                else
                {
                    b.AddRegret(self.goldReward);
                    self.goldReward = 0;
                }
                orig(self, damageReport);
            }
            else
            {
                orig(self, damageReport);
            }
        }

        private static void CharacterMaster_GiveMoney(On.RoR2.CharacterMaster.orig_GiveMoney orig, CharacterMaster self, uint amount)
        {
            ShipOfRegret.GiveMoney(self, amount);
            orig(self, amount);
        }

        private static void Ror2Application_onLoad()
        {
            PrismaticCoral.SetUpPayCostDelegates();
        }

        private static bool PurchaseInteraction_CanBeAffordedByInteractor(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
        {
            return orig(self, activator);
        }

        private static void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, EntityStates.GenericCharacterMain self)
        {
            if (!CaudalFin.ProcessJump(self))
            {
                orig(self);
            }
        }

        private static void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            JeremiahsAccident.OnCharacterDeath(self, damageReport);
            orig(self, damageReport);
        }

        private static RoR2.UI.InspectInfo ShopTerminalBehavior_GetInfo(On.RoR2.ShopTerminalBehavior.orig_GetInfo orig, ShopTerminalBehavior self)
        {
            return orig(self);
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            TimsCrucible.TakeDamage(self, damageInfo);
            orig(self, damageInfo);
        }

        private static void CharacterBody_OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if (NetworkServer.active)
            {
                TimsCrucible.OnEquipmentLost(self, equipmentDef);
            }
            if (equipmentDef.equipmentIndex == KrodEquipment.JeremiahsAccident.equipmentIndex)
            {
                JeremiahsAccident.OnEquipmentLost(self, equipmentDef);
            }
        }

        private static void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            TimsCrucible.OnEquipmentGained(self, equipmentDef);
            if (equipmentDef.equipmentIndex == KrodEquipment.JeremiahsAccident.equipmentIndex)
            {
                JeremiahsAccident.OnEquipmentGained(self, equipmentDef);
            }
        }

        private static void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);
            TimsCrucible.OnSkillActivated(self, skill);
            GodHand.OnSkillActivated(self, skill);
        }


        private static void DotController_OnDotStackRemovedServer(On.RoR2.DotController.orig_OnDotStackRemovedServer orig, DotController self, DotController.DotStack dotStack)
        {
            orig(self, dotStack);
            TimsCrucible.OnDotStackRemovedServer(self, dotStack);
        }

        private static void DotController_OnDotStackAddedServer(On.RoR2.DotController.orig_OnDotStackAddedServer orig, DotController self, DotController.DotStack dotStack)
        {
            orig(self, dotStack);
            TimsCrucible.OnDotStackAddedServer(self, dotStack);
        }

        private static void GlobalEventManager_ServerDamageDealt(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport damageReport)
        {
            orig(damageReport);
            DoubleFish.ServerDamageDealt(damageReport);
            NinjaShowerScrub.ServerDamageDealt(damageReport);
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, UnityEngine.GameObject victim)
        {
            orig(self, damageInfo, victim);
            LooseCards.OnHitEnemy(damageInfo, victim);
        }

        // does not work with a series of method calls
        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender || !sender.inventory) { return; }

            if (sender.HasBuff(DiscountCoffee.buff))
            {
                int c = sender.inventory.GetItemCountEffective(KrodItems.DiscountCoffee);
                args.attackSpeedMultAdd += c * 0.15f;
                args.sprintSpeedAdd += c * 0.25f;
            }

            if (sender.isSprinting)
            {
                int numMotorcycle = sender.inventory.GetItemCountEffective(KrodItems.ToyMotorcycle);
                if (numMotorcycle > 0)
                {
                    int c = 0;
                    foreach (var i in ItemCatalog.tier2ItemList)
                    {
                        c += sender.inventory.GetItemCountEffective(i);
                    }
                    int white = sender.inventory.GetItemCountEffective(RoR2Content.Items.ScrapWhite);
                    int green = sender.inventory.GetItemCountEffective(RoR2Content.Items.ScrapGreen);
                    int red = sender.inventory.GetItemCountEffective(RoR2Content.Items.ScrapRed);
                    int yellow = sender.inventory.GetItemCountEffective(RoR2Content.Items.ScrapYellow);

                    args.moveSpeedMultAdd += .05f + (c * (.05f * numMotorcycle)) +
                        (white * 0.03f) +
                        (green * .1f) +
                        (red * 0.5f) +
                        (yellow * 0.5f);
                }
                int fins = sender.inventory.GetItemCountEffective(KrodItems.CaudalFin);
                if (fins > 0)
                {
                    CaudalFin.Behavior behavior = sender.GetComponent<CaudalFin.Behavior>();

                    if (behavior && behavior.accelerate)
                    {
                        args.moveSpeedMultAdd += 0.5f * fins;
                    }
                }
            }

            if (sender.HasBuff(TimsCrucible.buffDef))
            {
                int m = sender.inventory.GetItemCountEffective(KrodItems.TimsCrucible);
                args.attackSpeedMultAdd += 0.3f * m;
                args.armorAdd += 20 * m;
                args.moveSpeedMultAdd += 0.2f * m;
            }

            if (sender.inventory.GetItemCountEffective(KrodItems.NinjaShowerScrub) > 0)
            {
                args.critAdd += 5f;
            }

            int requitalStacks = sender.GetBuffCount(MisterBoinkyAscended.buffDef);
            if (requitalStacks > 0)
            {
                args.damageMultAdd += requitalStacks * .1f;
            }

            args.armorAdd += 4 * sender.inventory.GetItemCountEffective(KrodItems.MisterBoinkyConsumed);
        }

        private static void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            DiscountCoffee.OnInteractionBegin(self, activator);
        }

        private static void MultiShopCardUtils_OnPurchase(On.RoR2.Items.MultiShopCardUtils.orig_OnPurchase orig, CostTypeDef.PayCostContext context, int moneyCost)
        {
            CharacterMaster master = context.activatorMaster;
            Inventory inventory = (master.inventory ? master.inventory : null);
            if (master &&
                inventory &&
                master.inventory.currentEquipmentIndex != DLC1Content.Equipment.MultiShopCard.equipmentIndex &&
                inventory.GetItemCountEffective(KrodItems.ArcadeToken) > 0 &&
                context.purchasedObject)
            {
                ArcadeToken.OnPurchase(context);
            }
            else
            {
                orig(context, moneyCost);
            }
        }

        private static int CharacterMaster_GetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            if (slot == DeployableSlot.MinorConstructOnKill)
            {
                return JeffsServiceMedal.GetDeployableSameSlotLimit(self, slot);
            }
            else
            {
                return orig(self, slot);
            }
        }

        private static void CharacterBody_OnTakeDamageServer(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            AncientRecordingSystem.OnTakeDamageServer(self, damageReport);
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.inventory)
            {
                if (NetworkServer.active)
                {
                    self.AddItemBehavior<DiscountCoffee.Behavior>(self.inventory.GetItemCountEffective(KrodItems.DiscountCoffee));
                    self.AddItemBehavior<LooseCards.Behavior>(self.inventory.GetItemCountEffective(KrodItems.LooseCards));
                    self.AddItemBehavior<MisterBoinky.Behavior>(self.inventory.GetItemCountEffective(KrodItems.MisterBoinky));
                    self.AddItemBehavior<MisterBoinkyReborn.Behavior>(self.inventory.GetItemCountEffective(KrodItems.MisterBoinkyReborn));
                    self.AddItemBehavior<MisterBoinkyAscended.Behavior>(self.inventory.GetItemCountEffective(KrodItems.MisterBoinkyAscended));
                    self.AddItemBehavior<MisterBoinkyTranscended.Behavior>(self.inventory.GetItemCountEffective(KrodItems.MisterBoinkyTranscended));
                    self.AddItemBehavior<TheExtra.Behavior>(self.inventory.GetItemCountEffective(KrodItems.TheExtra));
                    self.AddItemBehavior<NinjaShowerScrub.Behavior>(self.inventory.GetItemCountEffective(KrodItems.NinjaShowerScrub));
                    self.AddItemBehavior<ShipOfRegret.Behavior>(self.inventory.GetItemCountEffective(KrodItems.ShipOfRegret));
                    self.AddItemBehavior<WeightedDice.Behavior>(self.inventory.GetItemCountEffective(KrodItems.WeightedDice));
                    self.AddItemBehavior<Woodhat.Behavior>(self.inventory.GetItemCountEffective(KrodItems.Woodhat));
                    if (self.inventory.GetItemCountEffective(KrodItems.RorysForesight) > 0)
                    {
                        if (!self.HasBuff(RorysForsight.cooldownBuff) &&
                           !self.HasBuff(RorysForsight.isAvailableBuff))
                        {
                            self.AddBuff(RorysForsight.isAvailableBuff);
                        }
                    }
                    else
                    {
                        self.RemoveBuff(RorysForsight.cooldownBuff);
                        self.RemoveBuff(RorysForsight.isAvailableBuff);
                    }
                    if (self.inventory.GetItemCountEffective(KrodItems.CaudalFin) > 0)
                    {
                        self.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                    }
                }
                AncientRecordingSystem.OnInventoryChanged(self);
                self.AddItemBehavior<CaudalFin.Behavior>(self.inventory.GetItemCountEffective(KrodItems.CaudalFin));
                self.AddItemBehavior<GodHand.Behavior>(self.inventory.GetItemCountEffective(KrodItems.GodHand));
            }
        }

        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef.equipmentIndex == KrodEquipment.AileensGlassEye.equipmentIndex)
            {
                return AileensGlassEye.PerformEquipmentAction(self, equipmentDef);
            }
            else if (equipmentDef.equipmentIndex == KrodEquipment.AileensGlassEyeCracked.equipmentIndex)
            {
                return AileensGlassEye.AileensGlassEyeCracked.PerformEquipmentAction(self, equipmentDef);
            }
            else if (equipmentDef.equipmentIndex == KrodEquipment.AncientRecordingSystem.equipmentIndex)
            {
                return AncientRecordingSystem.PerformEquipmentAction(self, equipmentDef);
            }
            else if (equipmentDef.equipmentIndex == KrodEquipment.JeffsServiceMedal.equipmentIndex)
            {
                return JeffsServiceMedal.PerformEquipmentAction(self, equipmentDef);
            }
            else if (equipmentDef.equipmentIndex == KrodEquipment.JeremiahsAccident.equipmentIndex)
            {
                return JeremiahsAccident.PerformEquipmentAction(self, equipmentDef);
            }
            else if (equipmentDef.equipmentIndex == DLC2Content.Equipment.HealAndRevive.equipmentIndex)
            {
                if (self.characterBody.inventory)
                {
                    int c = self.characterBody.inventory.GetItemCountEffective(KrodItems.MisterBoinkyConsumed);
                    if (c > 0)
                    {
                        self.characterBody.inventory.RemoveItemPermanent(KrodItems.MisterBoinkyConsumed, c);
                        self.characterBody.inventory.GiveItemPermanent(KrodItems.MisterBoinkyReborn.itemIndex, c);
                        CharacterMasterNotificationQueue.SendTransformNotification(
                            self.characterBody.master,
                            KrodItems.MisterBoinkyConsumed.itemIndex,
                            KrodItems.MisterBoinkyReborn.itemIndex,
                            CharacterMasterNotificationQueue.TransformationType.Default
                        );
                    }
                    return orig(self, equipmentDef);
                }
                else
                {
                    return orig(self, equipmentDef);
                }
            }
            else
            {
                return orig(self, equipmentDef);
            }

        }

        public static void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            if (targetingEquipmentIndex == KrodEquipment.AileensGlassEyeCracked.equipmentIndex)
            {
                AileensGlassEye.AileensGlassEyeCracked.UpdateTargets(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            }
            else if (targetingEquipmentIndex == KrodEquipment.AncientRecordingSystem.equipmentIndex)
            {
                AncientRecordingSystem.UpdateTargets(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            }
            else
            {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            }

        }
    }
}
