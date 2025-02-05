using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Tier3
{
    public static class RorysForsight
    {
        public static BuffDef isAvailableBuff;
        public static BuffDef cooldownBuff;
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
            if (NetworkServer.active)
            {
                CharacterMaster ms = indicator.pingOwner.GetComponent<CharacterMaster>();
                if (!ms) { return; }
                CharacterBody body = ms.GetBody();
                if (!body) { return; }

                if (indicator.pingType == RoR2.UI.PingIndicator.PingType.Interactable &&
                   body.HasBuff(isAvailableBuff))
                {
                    PickupDef revealedItem = null;

                    ChestBehavior chestBehavior = indicator.pingTarget.GetComponent<ChestBehavior>();
                    ShopTerminalBehavior shopTerminalBehavior = indicator.pingTarget.GetComponent<ShopTerminalBehavior>();
                    if (chestBehavior)
                    {
                        PickupIndex idx = chestBehavior.dropPickup;
                        revealedItem = PickupCatalog.GetPickupDef(idx);
                    }
                    else if (shopTerminalBehavior)
                    {
                        if (shopTerminalBehavior.hidden)
                        {
                            revealedItem = PickupCatalog.GetPickupDef(shopTerminalBehavior.CurrentPickupIndex());
                            shopTerminalBehavior.SetHidden(false);
                        }
                    }
                    if (revealedItem != null)
                    {
                        int c = body.inventory.GetItemCount(KrodItems.RorysForsight) - 1;
                        float pctReduction = (float)System.Math.Tanh(c * 0.2f);
                        Log.Info($"pct reduction: {pctReduction}");
                        body.RemoveBuff(isAvailableBuff);
                        body.AddTimedBuff(cooldownBuff, 60 - (60 * pctReduction));
                        if (!body.inventory) { return; }
                        string tokenMsg;
                        if (revealedItem.itemIndex != ItemIndex.None)
                        {
                            ItemTierDef tier = ItemTierCatalog.GetItemTierDef(revealedItem.itemTier);
                            string hexColor = ColorCatalog.GetColorHexString(tier.colorIndex);
                            tokenMsg = "<style=cIsDamage>{0} revealed:</style> <color=#" + hexColor + ">{1}</color>";
                            CharacterMasterNotificationQueue.PushItemNotification(ms, revealedItem.itemIndex);
                        }
                        else
                        {
                            string hexColor = ColorCatalog.GetColorHexString(ColorCatalog.ColorIndex.Equipment);
                            tokenMsg = "<style=cIsDamage>{0} revealed:</style> <color=#" + hexColor + ">{1}</color>";
                            CharacterMasterNotificationQueue.PushEquipmentNotification(ms, revealedItem.equipmentIndex);
                        }
                        Chat.SimpleChatMessage m = new()
                        {
                            baseToken = tokenMsg,
                            paramTokens = [
                                Util.GetBestMasterName(ms),
                                    Language.GetString(revealedItem.nameToken)
                            ]
                        };
                        Chat.SendBroadcastChat(m);
                    }
                }
            }
        }
    }
}
