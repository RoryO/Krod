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

namespace Krod
{
    public static class Hooks
    {
        public static void Awake()
        {
            On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;

            On.RoR2.CharacterBody.OnEquipmentGained += CharacterBody_OnEquipmentGained;
            On.RoR2.CharacterBody.OnEquipmentLost += CharacterBody_OnEquipmentLost;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.CharacterBody.OnTakeDamageServer += CharacterBody_OnTakeDamageServer;

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

            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += PurchaseInteraction_CanBeAffordedByInteractor;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;

            On.RoR2.SceneExitController.Begin += SceneExitController_Begin;

            On.RoR2.ShopTerminalBehavior.GetInfo += ShopTerminalBehavior_GetInfo;

            On.RoR2.Items.MultiShopCardUtils.OnPurchase += MultiShopCardUtils_OnPurchase;

            RoR2Application.onLoad += Ror2Application_onLoad;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
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
                if (shouldPreventExit) {
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
                int c = inventory.GetItemCount(KrodItems.ShipOfRegret);
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
            //PrismaticCoral.SetUpPayCostDelegates();
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
            TimsCrucible.OnEquipmentLost(self, equipmentDef);
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

        private static void DotController_OnDotStackRemovedServer(On.RoR2.DotController.orig_OnDotStackRemovedServer orig, DotController self, object dotStack)
        {
            orig(self, dotStack);
            TimsCrucible.OnDotStackRemovedServer(self, dotStack);
        }

        private static void DotController_OnDotStackAddedServer(On.RoR2.DotController.orig_OnDotStackAddedServer orig, DotController self, object dotStack)
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
                int c = sender.inventory.GetItemCount(KrodItems.DiscountCoffee);
                args.attackSpeedMultAdd += c * 0.15f;
                args.sprintSpeedAdd += c * 0.25f;
            }

            if (sender.isSprinting)
            {
                if (sender.inventory.GetItemCount(KrodItems.ToyMotorcycle) > 0)
                {
                    int c = 0;
                    foreach (var i in ItemCatalog.tier2ItemList)
                    {
                        c += sender.inventory.GetItemCount(i);
                    }
                    int white = sender.inventory.GetItemCount(RoR2Content.Items.ScrapWhite);
                    int green = sender.inventory.GetItemCount(RoR2Content.Items.ScrapGreen);
                    int red = sender.inventory.GetItemCount(RoR2Content.Items.ScrapRed);
                    int yellow = sender.inventory.GetItemCount(RoR2Content.Items.ScrapYellow);

                    args.moveSpeedMultAdd += .05f + (c * .05f) + (white * 0.03f) + (green * .1f) + (red * 0.5f) + (yellow * 0.5f);
                }
                int fins = sender.inventory.GetItemCount(KrodItems.CaudalFin);
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
                int m = sender.inventory.GetItemCount(KrodItems.TimsCrucible);
                args.attackSpeedMultAdd += 0.3f * m;
                args.armorAdd += 20 * m;
                args.moveSpeedMultAdd += 0.2f * m;
            }

            if (sender.inventory.GetItemCount(KrodItems.NinjaShowerScrub) > 0)
            {
                args.critAdd += 5f;
            }

            args.armorAdd += 4 * sender.inventory.GetItemCount(KrodItems.MisterBoinkyConsumed);
            args.armorAdd += 10 * sender.inventory.GetItemCount(KrodItems.Woodhat);
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
                inventory.GetItemCount(KrodItems.ArcadeToken) > 0 &&
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

                    AncientRecordingSystem.OnInventoryChanged(self);
                    self.AddItemBehavior<DiscountCoffee.Behavior>(self.inventory.GetItemCount(KrodItems.DiscountCoffee));
                    self.AddItemBehavior<LooseCards.Behavior>(self.inventory.GetItemCount(KrodItems.LooseCards));
                    self.AddItemBehavior<MisterBoinky.Behavior>(self.inventory.GetItemCount(KrodItems.MisterBoinky));
                    self.AddItemBehavior<TheExtra.Behavior>(self.inventory.GetItemCount(KrodItems.TheExtra));
                    self.AddItemBehavior<NinjaShowerScrub.Behavior>(self.inventory.GetItemCount(KrodItems.NinjaShowerScrub));
                    self.AddItemBehavior<GodHand.Behavior>(self.inventory.GetItemCount(KrodItems.GodHand));
                    self.AddItemBehavior<ShipOfRegret.Behavior>(self.inventory.GetItemCount(KrodItems.ShipOfRegret));
                }
                self.AddItemBehavior<CaudalFin.Behavior>(self.inventory.GetItemCount(KrodItems.CaudalFin));
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
