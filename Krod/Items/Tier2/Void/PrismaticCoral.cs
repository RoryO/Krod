#if DEBUG
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2.Void
{
    public static class PrismaticCoral
    {
        public static CostTypeDef.PayCostDelegate origPayWhiteItemDelegate;
        public static CostTypeDef.PayCostDelegate origPayGreenItemDelegate;
        public static CostTypeDef.PayCostDelegate origPayRedItemDelegate;
        public static CostTypeDef.PayCostDelegate origPayBossItemDelegate;

        public static CostTypeDef.IsAffordableDelegate origAffordableWhiteDelegate;
        public static CostTypeDef.IsAffordableDelegate origAffordableGreenDelegate;
        public static CostTypeDef.IsAffordableDelegate origAffordableRedDelegate;
        public static CostTypeDef.IsAffordableDelegate origAffordableBossDelegate;
        public static void Awake()
        {
            KrodItems.PrismaticCoral = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.PrismaticCoral.canRemove = true;
            KrodItems.PrismaticCoral.name = "PRISMATIC_CORAL_NAME";
            KrodItems.PrismaticCoral.nameToken = "PRISMATIC_CORAL_NAME";
            KrodItems.PrismaticCoral.pickupToken = "PRISMATIC_CORAL_PICKUP";
            KrodItems.PrismaticCoral.descriptionToken = "PRISMATIC_CORAL_DESC";
            KrodItems.PrismaticCoral.loreToken = "PRISMATIC_CORAL_LORE";
            KrodItems.PrismaticCoral.tags = [
                ItemTag.Utility, 
                ItemTag.PriorityScrap
            ];
            KrodItems.PrismaticCoral._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier2Def.asset").WaitForCompletion();
            KrodItems.PrismaticCoral.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/PrismaticCoral.png");
            KrodItems.PrismaticCoral.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.PrismaticCoral, new ItemDisplayRuleDict(null)));

            ItemDef regen = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/RegeneratingScrap/RegeneratingScrap.asset").WaitForCompletion();
            ItemDef consumed = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/RegeneratingScrap/RegeneratingScrapConsumed.asset").WaitForCompletion();

            ItemRelationshipProvider provider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            provider.relationshipType = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();
            provider.relationships = [
                new ItemDef.Pair
                {
                    itemDef1 = regen,
                    itemDef2 = KrodItems.PrismaticCoral
                },
                new ItemDef.Pair
                {
                    itemDef1 = consumed,
                    itemDef2 = KrodItems.PrismaticCoral
                }
            ];

            ContentAddition.AddItemRelationshipProvider(provider);
        }
        public static void SetUpPayCostDelegates()
        {
            origPayWhiteItemDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.WhiteItem].payCost;
            origAffordableWhiteDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.WhiteItem].isAffordable;

            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.WhiteItem].payCost = PayWhiteCostWithCoral;
            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.WhiteItem].isAffordable = IsAffordableWhiteDelegate;

            origPayGreenItemDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.GreenItem].payCost;
            origAffordableGreenDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.GreenItem].isAffordable;

            origPayRedItemDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.RedItem].payCost;
            origAffordableRedDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.RedItem].isAffordable;

            origPayBossItemDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.BossItem].payCost;
            origAffordableBossDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.BossItem].isAffordable;
        }

        public static void PayWhiteCostWithCoral(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
        {
            CharacterBody body = context.activator.GetComponent<CharacterBody>();
            if (!body) { return; }
            Inventory inv = body.inventory;
            if (!inv) { return; }
            int numCoral = inv.GetItemCount(KrodItems.PrismaticCoral);
            if (numCoral == 0)
            {
                origPayWhiteItemDelegate(costTypeDef, context);
            }
        }

        public static bool IsAffordableWhiteDelegate(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
        {
            CharacterBody body = context.activator.GetComponent<CharacterBody>();
            if (!body) { return false; }
            Inventory inv = body.inventory;
            if (!inv) { return false; }
            int numCoral = inv.GetItemCount(KrodItems.PrismaticCoral);
            if (numCoral == 0)
            {
                return origAffordableWhiteDelegate(costTypeDef, context);
            }
            else
            {
                return inv.HasAtLeastXTotalItemsOfTier(costTypeDef.itemTier, context.cost - numCoral);
            }

        }

        // CostTypeDef.payCost delegate might work instead. The original delegates are 'hidden'
        // as they are defined within CostTypeCatalog.Init method and not accessable static methods.

        /*
        public static (bool, CostTypeDef.PayCostResults) CanBePaidWithCoral(PurchaseInteraction purchaseInteraction, Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            List<PayCoralCost> rvalue = new List<PayCoralCost>();

            if (!body) { return (false, rvalue); }
            Inventory inv = body.inventory;
            if (!inv) { return (false, rvalue); }
            int numCoral = inv.GetItemCount(KrodItems.PrismaticCoral);
            if (numCoral == 0)
            {
                return (false, rvalue);
            }
            if (
                purchaseInteraction.costType != CostTypeIndex.WhiteItem ||
                purchaseInteraction.costType != CostTypeIndex.GreenItem ||
                purchaseInteraction.costType != CostTypeIndex.RedItem ||
                purchaseInteraction.costType != CostTypeIndex.BossItem)
            {
                return (false, rvalue);
            }

            ItemDef scrapDef = ScrapItemForPrinter(purchaseInteraction.costType);

            int numScrapHeld = inv.GetItemCount(scrapDef);
            if (numScrapHeld >= purchaseInteraction.cost)
            {
                return (false, rvalue);
            }
            if ((numScrapHeld + numCoral) < purchaseInteraction.cost)
            {
                return (false, rvalue);
            }

            int coralRequired = numCoral;
            if (numScrapHeld > 0)
            {
                coralRequired -= numScrapHeld;
                for (int i = 0; i < numScrapHeld; i++)
                {
                }
            }
            for (int i = 0; i < coralRequired; ++i)
            {
            }
            return (true, rvalue);
        }

        public static ItemDef ScrapItemForPrinter(CostTypeIndex idx) => idx switch
        {
            CostTypeIndex.WhiteItem => RoR2Content.Items.ScrapWhite,
            CostTypeIndex.GreenItem => RoR2Content.Items.ScrapGreen,
            CostTypeIndex.RedItem => RoR2Content.Items.ScrapRed,
            CostTypeIndex.BossItem => RoR2Content.Items.ScrapYellow,
            _ => throw new System.ArgumentException($"Unknown cost type {idx}"),
        };
        */
    }
}
#endif
