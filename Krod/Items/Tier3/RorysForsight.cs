using R2API;
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

        public static void Awake()
        {
            KrodItems.RorysForsight = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.RorysForsight.canRemove = true;
            KrodItems.RorysForsight.name = "RORYS_FORSIGHT_NAME";
            KrodItems.RorysForsight.nameToken = "RORYS_FORSIGHT_NAME";
            KrodItems.RorysForsight.pickupToken = "RORYS_FORSIGHT_PICKUP";
            KrodItems.RorysForsight.descriptionToken = "RORYS_FORSIGHT_DESC";
            KrodItems.RorysForsight.loreToken = "RORYS_FORSIGHT_LORE";
            KrodItems.RorysForsight._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            KrodItems.RorysForsight.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/RorysForsight.png");
            KrodItems.RorysForsight.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.RorysForsight, new ItemDisplayRuleDict(null)));
            isAvailableBuff = ScriptableObject.CreateInstance<BuffDef>();
            isAvailableBuff.isDebuff = false;
            isAvailableBuff.canStack = false;
            isAvailableBuff.name = "Forsight Available";
            isAvailableBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/RorysForsightAvailable.png");
            ContentAddition.AddBuffDef(isAvailableBuff);

            cooldownBuff = ScriptableObject.CreateInstance<BuffDef>();
            cooldownBuff.isDebuff = false;
            cooldownBuff.canStack = false;
            cooldownBuff.name = "Forsight Coming...";
            cooldownBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/RorysForsightCooldown.png");
            ContentAddition.AddBuffDef(cooldownBuff);
        }

        public static void RevealContents(RoR2.UI.PingIndicator indicator)
        {
            if (!NetworkServer.active) { return; }
            CharacterMaster ms = indicator.pingOwner.GetComponent<CharacterMaster>();
            if (!ms) { return; }
            CharacterBody body = ms.GetBody();
            if (!body) { return; }

            if (indicator.pingType != RoR2.UI.PingIndicator.PingType.Interactable ||
                !body.HasBuff(isAvailableBuff))
            {
                return;
            }

            List<PickupDef> revealedItems = [];

            ChestBehavior chestBehavior = indicator.pingTarget.GetComponent<ChestBehavior>();
            ShopTerminalBehavior shopTerminalBehavior = indicator.pingTarget.GetComponent<ShopTerminalBehavior>();
            if (chestBehavior)
            {
                ChestForsightBehavior chestForsight = null;
                chestForsight = indicator.pingTarget.GetComponent<ChestForsightBehavior>();
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
                if (shopTerminalBehavior.hidden)
                {
                    revealedItems.Add(PickupCatalog.GetPickupDef(shopTerminalBehavior.CurrentPickupIndex()));
                    shopTerminalBehavior.SetHidden(false);
                }
            }
            if (revealedItems.Count == 0) { return; }

            int c = body.inventory.GetItemCount(KrodItems.RorysForsight) - 1;
            float pctReduction = (float)System.Math.Tanh(c * 0.2f);
            Log.Info($"pct reduction: {pctReduction}");
            body.RemoveBuff(isAvailableBuff);
            body.AddTimedBuff(cooldownBuff, 60 - (60 * pctReduction));
            if (!body.inventory) { return; }
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
                itemMessages[i] = "<color=#" + (hexColor != null ? hexColor : equipmentColor) + ">{" + (i + 1) + "}</color> ";
                paramTokens[i + 1] = Language.GetString(revealedItems[i].nameToken);
            }
            Chat.SimpleChatMessage m = new()
            {
                baseToken = tokenMsg.Append(System.String.Join(", ", itemMessages)).ToString(),
                paramTokens = paramTokens
            };
            Chat.SendBroadcastChat(m);
        }
    }
}
