﻿using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;

namespace Krod.Items.Tier3
{
    public static class RorysForsight
    {
        public static BuffDef isAvailableBuff;
        public static BuffDef cooldownBuff;

        public class ChestForsightBehavior : MonoBehaviour
        {
            public List<CharacterBody> revealersWithSaleStar = [];
            public List<List<PickupDef>> revealedItems = [];
            public CharacterBody lastPingedBy;

            public List<PickupDef> RevealedItemsForCharacter(CharacterBody character)
            {
                var idx = revealersWithSaleStar.FindIndex(e => e == character);
                if (idx == -1)
                {
                    return [];
                }
                else
                {
                    return revealedItems[idx];
                }
            }

            public void AddRevealedItemsForCharacter(CharacterBody character, List<PickupDef> items)
            {
                int idx = revealersWithSaleStar.FindIndex(e => e == character);
                if (idx != -1)
                {
                    revealedItems[idx] = items;
                }
                else
                {
                    revealersWithSaleStar.Add(character);
                    revealedItems.Add(items);
                }
            }
        }
        public class ChanceShrineForsightBehavior : MonoBehaviour
        {
            public struct RevealedPurchaseCost
            {
                public int cost;
                public PickupDef item;
            }

            public List<RevealedPurchaseCost> revealedCosts = [];
            public int lastPurchasedIndex = 0;
        }

        public static void Awake()
        {
            KrodItems.RorysForesight = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.RorysForesight.canRemove = true;
            KrodItems.RorysForesight.name = "RORYS_FORESIGHT_NAME";
            KrodItems.RorysForesight.nameToken = "RORYS_FORESIGHT_NAME";
            KrodItems.RorysForesight.pickupToken = "RORYS_FORESIGHT_PICKUP";
            KrodItems.RorysForesight.descriptionToken = "RORYS_FORESIGHT_DESC";
            KrodItems.RorysForesight.loreToken = "RORYS_FORESIGHT_LORE";
            KrodItems.RorysForesight._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            KrodItems.RorysForesight.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/RorysForsight.png");
            KrodItems.RorysForesight.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.RorysForesight, new ItemDisplayRuleDict(null)));
            isAvailableBuff = ScriptableObject.CreateInstance<BuffDef>();
            isAvailableBuff.isDebuff = false;
            isAvailableBuff.canStack = false;
            isAvailableBuff.name = "Foresight Available";
            isAvailableBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/RorysForsightAvailable.png");
            ContentAddition.AddBuffDef(isAvailableBuff);

            cooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            cooldownBuff.isDebuff = false;
            cooldownBuff.canStack = false;
            cooldownBuff.name = "Foresight Coming...";
            cooldownBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/RorysForsightCooldown.png");
            ContentAddition.AddBuffDef(cooldownBuff);
        }
        public static void AddShrineStack(ShrineChanceBehavior self, Interactor activator, ChanceShrineForsightBehavior b)
        {
            // y compiler error tho
            // ShrineChanceBehavior.onShrineChancePurchaseGlobal(true, activator);
            PickupDropletController.CreatePickupDroplet(
                b.revealedCosts[b.lastPurchasedIndex].item.pickupIndex,
                self.dropletOrigin.position,
                self.dropletOrigin.forward * 20f
            );

            EffectManager.SpawnEffect(self.effectPrefabShrineRewardNormal, new EffectData
            {
                origin = self.transform.position,
                rotation = Quaternion.identity,
                scale = 1f,
                color = self.colorShrineRewardNormal
            }, transmit: true);

            self.successfulPurchaseCount++;
            b.lastPurchasedIndex++;
            self.waitingForRefresh = true;
            self.refreshTimer = 2f;

            if (self.successfulPurchaseCount >= self.maxPurchaseCount)
            {
                self.symbolTransform.gameObject.SetActive(value: false);
                self.CallRpcSetPingable(value: false);
            }
        }

        public static void RevealContents(RoR2.UI.PingIndicator indicator)
        {
            if (!NetworkServer.active) { return; }
            CharacterMaster ms = indicator.pingOwner.GetComponent<CharacterMaster>();
            if (!ms) { return; }
            CharacterBody body = ms.GetBody();
            if (!body || !body.inventory) { return; }

            if (indicator.pingType != RoR2.UI.PingIndicator.PingType.Interactable ||
                !body.HasBuff(isAvailableBuff))
            {
                return;
            }

            ChestBehavior chestBehavior = indicator.pingTarget.GetComponent<ChestBehavior>();
            OptionChestBehavior optionChestBehavior = indicator.pingTarget.GetComponent<OptionChestBehavior>();
            ShopTerminalBehavior shopTerminalBehavior = indicator.pingTarget.GetComponent<ShopTerminalBehavior>();
            ShrineChanceBehavior shrineBehavior = indicator.pingTarget.GetComponent<ShrineChanceBehavior>();

            bool didRevealObject = chestBehavior != null ||
                shopTerminalBehavior != null ||
                shrineBehavior != null ||
                optionChestBehavior != null;

            bool isShrine = shrineBehavior != null;

            if (isShrine)
            {
                ChanceShrineForsightBehavior b = indicator.pingTarget.GetComponent<ChanceShrineForsightBehavior>();
                if (!b)
                {
                    b = indicator.pingTarget.AddComponent<ChanceShrineForsightBehavior>();
                }
                int chanceDolls = body.inventory.GetItemCount(DLC2Content.Items.ExtraShrineItem);
                int purchasesRemaining = shrineBehavior.maxPurchaseCount - shrineBehavior.successfulPurchaseCount;
                if (purchasesRemaining == 0) { return; }
                int currentCost = shrineBehavior.purchaseInteraction.Networkcost;
                int costMultiplier = 1;
                while (b.revealedCosts.Count < purchasesRemaining)
                {
                    if (shrineBehavior.dropTable)
                    {
                        if (shrineBehavior.rng.nextNormalizedFloat > shrineBehavior.failureChance)
                        {
                            if (shrineBehavior.chanceDollDropTable && chanceDolls > 0)
                            {
                                if (Util.CheckRoll(30 + chanceDolls * 10, body.master))
                                {
                                    b.revealedCosts.Add(new()
                                    {
                                        cost = currentCost,
                                        item = shrineBehavior.chanceDollDropTable.GenerateDrop(shrineBehavior.rng).pickupDef
                                    });
                                }
                                else
                                {
                                    b.revealedCosts.Add(new()
                                    {
                                        cost = currentCost,
                                        item = shrineBehavior.dropTable.GenerateDrop(shrineBehavior.rng).pickupDef
                                    });
                                }
                            }
                            else
                            {
                                b.revealedCosts.Add(new()
                                {
                                    cost = currentCost,
                                    item = shrineBehavior.dropTable.GenerateDrop(shrineBehavior.rng).pickupDef
                                });
                            }
                        }
                    }
                    else
                    {
                        PickupIndex none = PickupIndex.none;
                        PickupIndex value = shrineBehavior.rng.NextElementUniform(Run.instance.availableTier1DropList);
                        PickupIndex value2 = shrineBehavior.rng.NextElementUniform(Run.instance.availableTier2DropList);
                        PickupIndex value3 = shrineBehavior.rng.NextElementUniform(Run.instance.availableTier3DropList);
                        PickupIndex value4 = shrineBehavior.rng.NextElementUniform(Run.instance.availableEquipmentDropList);
                        WeightedSelection<PickupIndex> weightedSelection = new WeightedSelection<PickupIndex>();
                        weightedSelection.AddChoice(none, shrineBehavior.failureWeight);
                        weightedSelection.AddChoice(value, shrineBehavior.tier1Weight);
                        weightedSelection.AddChoice(value2, shrineBehavior.tier2Weight);
                        weightedSelection.AddChoice(value3, shrineBehavior.tier3Weight);
                        weightedSelection.AddChoice(value4, shrineBehavior.equipmentWeight);
                        PickupIndex foundIdx = weightedSelection.Evaluate(shrineBehavior.rng.nextNormalizedFloat);
                        if (foundIdx != none)
                        {
                            b.revealedCosts.Add(new()
                            {
                                cost = currentCost,
                                item = foundIdx.pickupDef
                            });
                        }
                    }
                    costMultiplier++;
                    currentCost = (int)(shrineBehavior.purchaseInteraction.cost * shrineBehavior.costMultiplierPerPurchase * costMultiplier);
                }

                shrineBehavior.purchaseInteraction.Networkcost = b.revealedCosts[b.lastPurchasedIndex].cost;

                StringBuilder tokenMsg = new("<style=cIsDamage>{0} revealed:</style> ");
                List<string> itemMessages = [];
                List<string> paramTokens = [];
                paramTokens.Add(Util.GetBestMasterName(ms));
                string equipmentColor = ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Equipment);
                int tokenCount = 1;
                for (int i = b.lastPurchasedIndex; i < b.revealedCosts.Count; i = i + 1)
                {
                    PickupDef item = b.revealedCosts[i].item;
                    string hexColor = null;
                    if (item.itemIndex != ItemIndex.None)
                    {
                        ItemTierDef tier = ItemTierCatalog.GetItemTierDef(item.itemTier);
                        hexColor = ColorCatalog.GetColorHexString(tier.colorIndex);
                        CharacterMasterNotificationQueue.PushItemNotification(ms, item.itemIndex);
                    }
                    else
                    {
                        CharacterMasterNotificationQueue.PushEquipmentNotification(ms, item.equipmentIndex);
                    }
                    itemMessages.Add("{" + (tokenCount) + "}: <color=#" + (hexColor != null ? hexColor : equipmentColor) + ">{" + (tokenCount + 1) + "}</color>");
                    paramTokens.Add("$" + TextSerialization.ToStringNumeric(b.revealedCosts[i].cost));
                    paramTokens.Add(Language.GetString(item.nameToken));
                    tokenCount += 2;
                }
                Chat.SimpleChatMessage msg = new()
                {
                    baseToken = tokenMsg.Append(System.String.Join(", ", itemMessages)).ToString(),
                    paramTokens = [.. paramTokens]
                };
                Chat.SendBroadcastChat(msg);
            }
            else
            {
                List<PickupDef> revealedItems = [];
                if (chestBehavior)
                {
                    ChestForsightBehavior chestForsight = indicator.pingTarget.GetComponent<ChestForsightBehavior>();
                    if (!chestForsight)
                    {
                        chestForsight = indicator.pingTarget.AddComponent<ChestForsightBehavior>();
                    }
                    PickupIndex idx = chestBehavior.dropPickup;
                    revealedItems = chestForsight.RevealedItemsForCharacter(body);
                    if (revealedItems.Count == 0)
                    {
                        revealedItems.Add(PickupCatalog.GetPickupDef(idx));
                        int saleStars = body.inventory.GetItemCount(DLC2Content.Items.LowerPricedChests);
                        if (saleStars > 0)
                        {
                            // derp
                        }
                        chestForsight.AddRevealedItemsForCharacter(body, revealedItems);
                    }
                }
                else if (shopTerminalBehavior)
                {
                    // is there any better way to do this?
                    if (indicator.pingTarget.ToString().Contains("Duplicator"))
                    {
                        return;
                    }
                    if (shopTerminalBehavior.hidden)
                    {
                        revealedItems.Add(PickupCatalog.GetPickupDef(shopTerminalBehavior.CurrentPickupIndex()));
                        shopTerminalBehavior.SetHidden(false);
                    }
                }
                else if (optionChestBehavior)
                {
                    for (int i = 0; i < optionChestBehavior.generatedDrops.Length; i = i + 1)
                    {
                        revealedItems.Add(PickupCatalog.GetPickupDef(optionChestBehavior.generatedDrops[i]));
                    }
                }
                StringBuilder tokenMsg = new("<style=cIsDamage>{0} revealed:</style> ");
                string[] itemMessages = new string[revealedItems.Count];
                string[] paramTokens = new string[revealedItems.Count + 1];
                paramTokens[0] = Util.GetBestMasterName(ms);
                string equipmentColor = ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Equipment);
                for (int i = 0; i < revealedItems.Count; i = i + 1)
                {
                    string hexColor = null;
                    if (revealedItems[i].itemIndex != ItemIndex.None)
                    {
                        ItemTierDef tier = ItemTierCatalog.GetItemTierDef(revealedItems[i].itemTier);
                        hexColor = ColorCatalog.GetColorHexString(tier.colorIndex);
                        CharacterMasterNotificationQueue.PushItemNotification(ms, revealedItems[i].itemIndex);
                    }
                    else
                    {
                        CharacterMasterNotificationQueue.PushEquipmentNotification(ms, revealedItems[i].equipmentIndex);
                    }
                    itemMessages[i] = "<color=#" + (hexColor != null ? hexColor : equipmentColor) + ">{" + (i + 1) + "}</color>";
                    paramTokens[i + 1] = Language.GetString(revealedItems[i].nameToken);
                }
                Chat.SimpleChatMessage m = new()
                {
                    baseToken = tokenMsg.Append(System.String.Join(", ", itemMessages)).ToString(),
                    paramTokens = paramTokens
                };
                Chat.SendBroadcastChat(m);
            }

            if (didRevealObject)
            {
                int c = body.inventory.GetItemCount(KrodItems.RorysForesight) - 1;
                float pctReduction = (float)System.Math.Tanh(c * 0.2f);
                body.RemoveBuff(isAvailableBuff);
                body.AddTimedBuff(cooldownBuff, 60 - (60 * pctReduction));
            }
        }
    }
}
