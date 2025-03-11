using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Tier1
{
    public static class WeightedDice
    {
        public static BuffDef addLuckBuff;
        public static BuffDef removeLuckBuff;
        public class Behavior : CharacterBody.ItemBehavior
        {
            public float rerollStopwatch = 0;
            public void Awake()
            {
                enabled = false;
            }

            public void Reroll()
            {
                body.RecalculateStats();
                if (Util.CheckRoll(30f, body.master.luck, body.master))
                {
                    body.AddTimedBuff(addLuckBuff, 10 + (20 * stack));
                }
                else if (Util.CheckRoll(10f, body.master.luck * -1, body.master))
                {
                    body.AddTimedBuff(removeLuckBuff, 60 - (60 * .2f * (stack - 1)));
                }
                rerollStopwatch = 0;
            }

            public void Update()
            {
                if (NetworkServer.active)
                {
                    rerollStopwatch += Time.deltaTime;
                    if (rerollStopwatch > 60f && 
                        !body.HasBuff(addLuckBuff) &&
                        !body.HasBuff(removeLuckBuff))
                    {
                        Reroll();
                    }
                }
            }

            public void OnEnable()
            {
                Reroll();
            }
        }
        public static void Awake()
        {
            KrodItems.WeightedDice = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.WeightedDice.canRemove = true;
            KrodItems.WeightedDice.name = "WEIGHTED_DICE_NAME";
            KrodItems.WeightedDice.nameToken = "WEIGHTED_DICE_NAME";
            KrodItems.WeightedDice.pickupToken = "WEIGHTED_DICE_PICKUP";
            KrodItems.WeightedDice.descriptionToken = "WEIGHTED_DICE_DESC";
            KrodItems.WeightedDice.loreToken = "WEIGHTED_DICE_LORE";
            KrodItems.WeightedDice.tags = [ItemTag.Utility];
            KrodItems.WeightedDice._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            KrodItems.WeightedDice.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            KrodItems.WeightedDice.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/WeightedDice.png");
            ItemAPI.Add(new CustomItem(KrodItems.WeightedDice, new ItemDisplayRuleDict(null)));

            addLuckBuff = ScriptableObject.CreateInstance<BuffDef>();
            addLuckBuff.name = "Weighted Dice Good Roll";
            addLuckBuff.canStack = false;
            addLuckBuff.isDebuff = false;
            addLuckBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/WeightedDiceSuccess.png");
            ContentAddition.AddBuffDef(addLuckBuff);

            removeLuckBuff = ScriptableObject.CreateInstance<BuffDef>();
            removeLuckBuff.name = "Weighted Dice Bad Roll";
            removeLuckBuff.canStack = false;
            removeLuckBuff.isDebuff = false;
            removeLuckBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier1/WeightedDiceFailure.png");
            ContentAddition.AddBuffDef(removeLuckBuff);
        }
    }
}
