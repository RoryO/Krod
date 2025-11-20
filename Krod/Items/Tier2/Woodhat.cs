using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2
{
    public static class Woodhat
    {
        public class Behavior : CharacterBody.ItemBehavior
        {
            public float additionStopwatch = 0;
            public float damageStopwatch = 0;
            public bool ready = true;
            
            public void Awake()
            {
                enabled = false;
            }

            public void Update()
            {
                additionStopwatch += Time.deltaTime;
                if (ready == false)
                {
                    damageStopwatch += Time.deltaTime;
                    if (damageStopwatch > 0.1f)
                    {
                        ready = true;
                        damageStopwatch = 0;
                    }
                }
                if (additionStopwatch > 2f)
                {
                    additionStopwatch = 0;
                    if (body.isSprinting) { return; }
                    Inventory inv = body.inventory;
                    if (!inv) { return; }
                    int c = inv.GetItemCountEffective(KrodItems.Woodhat);
                    int b = body.GetBuffCount(buffDef);
                    if (b < (c * 2))
                    {
                        body.AddBuff(buffDef);
                    }
                }
            }

        }

        public static BuffDef buffDef;
        public static void Awake()
        {
            KrodItems.Woodhat = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.Woodhat.canRemove = true;
            KrodItems.Woodhat.name = "WOODHAT_NAME";
            KrodItems.Woodhat.nameToken = "WOODHAT_NAME";
            KrodItems.Woodhat.pickupToken = "WOODHAT_PICKUP";
            KrodItems.Woodhat.descriptionToken = "WOODHAT_DESC";
            KrodItems.Woodhat.loreToken = "WOODHAT_LORE";
            KrodItems.Woodhat._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.Woodhat.tags = [ItemTag.Utility];
            KrodItems.Woodhat.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/WoodHat.png");
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.Woodhat.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier2/WoodHat.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            ItemAPI.Add(new CustomItem(KrodItems.Woodhat, new ItemDisplayRuleDict(null)));

            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.isDebuff = false;
            buffDef.canStack = true;
            buffDef.name = "Wood Armor";
            buffDef.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/WoodHatBD.png");
            ContentAddition.AddBuffDef(buffDef);
        }
    }
}
