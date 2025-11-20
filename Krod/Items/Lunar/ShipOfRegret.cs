using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Lunar
{
    public static class ShipOfRegret
    {
        public static BuffDef buffDef;
        public class Behavior : CharacterBody.ItemBehavior
        {
            public uint regretAccumulated = 0;
            public uint largeChestCost = 0;
            public float addFundsStopwatch = 0;
            public void Awake()
            {
                enabled = false;
                GameObject b = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Chest2/Chest2.prefab").WaitForCompletion();
                PurchaseInteraction c = b.GetComponent<PurchaseInteraction>();
                largeChestCost = (uint)Run.instance.GetDifficultyScaledCost(c.cost);
            }

            public void AddRegret(uint n)
            {
                if (!body.inventory) { return; }
                if (n == 0) { return; }

                regretAccumulated += 6 * n * (uint)body.inventory.GetItemCountEffective(KrodItems.ShipOfRegret);
                Log.Info($"add: total regret {regretAccumulated}");
                if (regretAccumulated > largeChestCost)
                {
                    int c = Mathf.FloorToInt(regretAccumulated / largeChestCost);
                    Log.Info($"adding {c} regret buff");
                    for (int i = 0; i < c; ++i)
                    {
                        body.AddBuff(buffDef);
                    }
                    regretAccumulated = regretAccumulated % largeChestCost;
                }
            }

            public void RemoveRegret(uint n)
            {
                if (!NetworkServer.active) { return; }
                if (n == 0) { return; }
                if (TeleporterInteraction.instance && TeleporterInteraction.instance.isCharged)
                {
                    Log.Info("accelerated regret removal");
                    n *= 4;
                }
                int c = (int)regretAccumulated - Mathf.FloorToInt((n / PlayerCharacterMasterController.instances.Count));
                if (c < 0)
                {
                    if (body.HasBuff(buffDef))
                    {
                        float f = Mathf.Abs(c) / (float)largeChestCost;
                        int stacksToRemove = Mathf.CeilToInt(f);

                        for (int i = 0; i < stacksToRemove; ++i)
                        {
                            body.RemoveBuff(buffDef);
                        }
                    }
                    regretAccumulated = 0;
                }
                else
                {
                    Log.Info("removing regret accumulation");
                    regretAccumulated -= n;
                }
                Log.Info($"remove: total regret {regretAccumulated}");
            }

            public void Update()
            {
                addFundsStopwatch += Time.deltaTime;
                if (addFundsStopwatch > 60)
                {
                    body.master.GiveMoney(largeChestCost);
                    addFundsStopwatch = 0;
                }
            }
        }
        public static void Awake()
        {
            KrodItems.ShipOfRegret = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.ShipOfRegret.canRemove = true;
            KrodItems.ShipOfRegret.name = "SHIP_OF_REGRET_NAME";
            KrodItems.ShipOfRegret.nameToken = "SHIP_OF_REGRET_NAME";
            KrodItems.ShipOfRegret.pickupToken = "SHIP_OF_REGRET_PICKUP";
            KrodItems.ShipOfRegret.descriptionToken = "SHIP_OF_REGRET_DESC";
            KrodItems.ShipOfRegret.loreToken = "SHIP_OF_REGRET_LORE";
            KrodItems.ShipOfRegret.tags = [];
            KrodItems.ShipOfRegret._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/LunarTierDef.asset").WaitForCompletion();
            KrodItems.ShipOfRegret.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Lunar/ShipOfRegret.png");
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.ShipOfRegret.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Lunar/ShipOfRegret.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            ItemAPI.Add(new CustomItem(KrodItems.ShipOfRegret, new ItemDisplayRuleDict(null)));
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Lunar/ShipOfRegret.png");
            buffDef.name = "Regret";
            buffDef.isDebuff = true;
            buffDef.canStack = true;
            ContentAddition.AddBuffDef(buffDef);
        }
        public static void GiveMoney(CharacterMaster master, uint amount)
        {
            CharacterBody body = master.GetBody();
            if (!body) { return; }
            Behavior b = body.GetComponent<Behavior>();
            if (!b) { return; }
            b.RemoveRegret(amount);
        }
    }
}
