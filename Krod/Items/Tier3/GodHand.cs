using R2API;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier3
{
    public static class GodHand
    {
        public class Behavior : CharacterBody.ItemBehavior
        {
            public float primaryStopwatch = 0;
            public float secondaryStopwatch = 0;
            public float utilityStopwatch = 0;
            public float specialStopwatch = 0;
            public void Awake()
            {
                enabled = false;
            }

            public void ActivatedPrimary()
            {
                if (primaryStopwatch > 0) { return; }
                SkillLocator locator = body.skillLocator;
                int c = body.inventory.GetItemCount(KrodItems.GodHand);
                if (!locator) { return; }
                if (locator.secondary.stock != locator.secondary.maxStock)
                {
                    locator.secondary.rechargeStopwatch += (0.75f + (0.25f * c));
                }
                primaryStopwatch = 1f;
            }

            public void ActivatedSecondary()
            {
                if (secondaryStopwatch > 0) { return; }
                SkillLocator locator = body.skillLocator;
                if (!locator) { return; }
                int c = body.inventory.GetItemCount(KrodItems.GodHand);
                if (locator.utility.stock != locator.utility.maxStock)
                {
                    locator.utility.rechargeStopwatch += (0.5f + (0.5f * c));
                }
                secondaryStopwatch = 1f;
            }
            public void ActivatedUtility()
            {
                if (utilityStopwatch > 0) { return; }
                SkillLocator locator = body.skillLocator;
                if (!locator) { return; }
                int c = body.inventory.GetItemCount(KrodItems.GodHand);
                if (locator.secondary.stock != locator.secondary.maxStock)
                {
                    locator.secondary.rechargeStopwatch += (0.5f + (0.5f * c));
                }

                if (locator.utility.stock != locator.utility.maxStock)
                {
                    locator.utility.rechargeStopwatch += (0.5f + (0.5f * c));
                }

                utilityStopwatch = 1f;
            }
            public void ActivatedSpecial()
            {
                if (specialStopwatch > 0) { return; }
                SkillLocator locator = body.skillLocator;
                if (!locator) { return; }
                int c = body.inventory.GetItemCount(KrodItems.GodHand);

                locator.primary.Reset();
                locator.secondary.Reset();
                locator.utility.Reset();

                specialStopwatch = 3.0f - (c * 0.5f);
            }

            public void Update()
            {
                if (primaryStopwatch > 0)
                {
                    primaryStopwatch -= Time.deltaTime;
                }
                if (secondaryStopwatch > 0)
                {
                    secondaryStopwatch -= Time.deltaTime;
                }
                if (utilityStopwatch > 0)
                {
                    utilityStopwatch -= Time.deltaTime;
                }
                if (specialStopwatch > 0)
                {
                    specialStopwatch -= Time.deltaTime;
                }
                if (body.skillLocator.secondary.rechargeStopwatch > 0)
                {
                    Log.Info($"s: {body.skillLocator.secondary.rechargeStopwatch}");
                }
            }
        }
        public static void Awake()
        {
            KrodItems.GodHand = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.GodHand.canRemove = true;
            KrodItems.GodHand.name = "GOD_HAND_NAME";
            KrodItems.GodHand.nameToken = "GOD_HAND_NAME";
            KrodItems.GodHand.pickupToken = "GOD_HAND_PICKUP";
            KrodItems.GodHand.descriptionToken = "GOD_HAND_DESC";
            KrodItems.GodHand.loreToken = "GOD_HAND_LORE";
            KrodItems.GodHand.tags = [ItemTag.Damage];
            KrodItems.GodHand._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            KrodItems.GodHand.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/AlienGodHand.png");
            KrodItems.GodHand.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.GodHand, new ItemDisplayRuleDict(null)));
        }

        public static void OnSkillActivated(CharacterBody self, GenericSkill skill)
        {
            SkillLocator locator = self.skillLocator;
            Behavior itemBehavior = self.GetComponent<Behavior>();
            if (!locator || !itemBehavior) { return; }
            if (skill == locator.primary)
            {
                itemBehavior.ActivatedPrimary();
            }
            else if (skill == locator.secondary)
            {
                itemBehavior.ActivatedSecondary();
            }
            else if (skill == locator.utility)
            {
                itemBehavior.ActivatedUtility();
            }
            else if (skill == locator.special)
            {
                itemBehavior.ActivatedSpecial();
            }
        }
    }
}
