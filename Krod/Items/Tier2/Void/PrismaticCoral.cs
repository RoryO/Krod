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
            ];
            KrodItems.PrismaticCoral._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier2Def.asset").WaitForCompletion();
            KrodItems.PrismaticCoral.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/PrismaticCoral.png");
            KrodItems.PrismaticCoral.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier2/PrismaticCoral.prefab");
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

            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.WhiteItem].payCost = PayCostWithCoral;
            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.WhiteItem].isAffordable = IsAffordableDelegate;

            origPayGreenItemDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.GreenItem].payCost;
            origAffordableGreenDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.GreenItem].isAffordable;

            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.GreenItem].payCost = PayCostWithCoral;
            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.GreenItem].isAffordable = IsAffordableDelegate;

            origPayRedItemDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.RedItem].payCost;
            origAffordableRedDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.RedItem].isAffordable;

            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.RedItem].payCost = PayCostWithCoral;
            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.RedItem].isAffordable = IsAffordableDelegate;

            origPayBossItemDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.BossItem].payCost;
            origAffordableBossDelegate = CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.BossItem].isAffordable;

            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.BossItem].payCost = PayCostWithCoral;
            CostTypeCatalog.costTypeDefs[(int)CostTypeIndex.BossItem].isAffordable = IsAffordableDelegate;
        }

        public static void CallOriginalPayDelegate(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
        {
                switch(costTypeDef.itemTier)
                {
                    case ItemTier.Tier1:
                        origPayWhiteItemDelegate(costTypeDef, context);
                        break;
                    case ItemTier.Tier2:
                        origPayGreenItemDelegate(costTypeDef, context);
                        break;
                    case ItemTier.Tier3:
                        origPayRedItemDelegate(costTypeDef, context);
                        break;
                    case ItemTier.Boss:
                        origPayBossItemDelegate(costTypeDef, context);
                        break;
                    default:
                        throw new System.Exception($"Unsupported tier {costTypeDef.itemTier}");
                }
        }
        public static bool CallOriginalAffordableDelegate(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
        {
                switch(costTypeDef.itemTier)
                {
                    case ItemTier.Tier1:
                        return origAffordableWhiteDelegate(costTypeDef, context);
                    case ItemTier.Tier2:
                        return origAffordableGreenDelegate(costTypeDef, context);
                    case ItemTier.Tier3:
                        return origAffordableRedDelegate(costTypeDef, context);
                    case ItemTier.Boss:
                        return origAffordableBossDelegate(costTypeDef, context);
                    default:
                        throw new System.Exception($"Unsupported tier {costTypeDef.itemTier}");
                }
        }

        public static void PayCostWithCoral(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
        {
            CharacterBody body = context.activator.GetComponent<CharacterBody>();
            if (!body) { return; }
            Inventory inv = body.inventory;
            if (!inv) { return; }
            int numCoral = inv.GetItemCount(KrodItems.PrismaticCoral);
            if (numCoral == 0)
            {
                CallOriginalPayDelegate(costTypeDef, context);
                return;
            }

            ItemDef scrap = ScrapItemForTier(costTypeDef.itemTier);
            int numScrapHeld = inv.GetItemCount(scrap);

            if (numScrapHeld >= context.cost)
            {
                CallOriginalPayDelegate(costTypeDef, context);
                return;
            }

            int totalCostPaid = 0;
            while (numScrapHeld > 0)
            {
                context.results.itemsTaken.Add(scrap.itemIndex);
                numScrapHeld--;
                totalCostPaid++;
            }

            while (numCoral > 0)
            {
                context.results.itemsTaken.Add(KrodItems.PrismaticCoral.itemIndex);
                numCoral--;
                totalCostPaid++;
                if (totalCostPaid >= context.cost) { break; }
            }

            if (totalCostPaid < context.cost)
            {
                var selection = new WeightedSelection<ItemIndex>();
                foreach (var idx in ItemCatalog.allItems)
                {
                    if (ItemCatalog.GetItemDef(idx).tier == costTypeDef.itemTier &&
                        (idx != scrap.itemIndex || idx != context.avoidedItemIndex)
                    )
                    {
                        int c = inv.GetItemCount(idx);
                        if (c == 0) { continue; }
                        selection.AddChoice(idx, c);
                    }
                }

                while (totalCostPaid < context.cost)
                {
                    int choiceIdx = selection.EvaluateToChoiceIndex(context.rng.nextNormalizedFloat);
                    var choice = selection.GetChoice(choiceIdx);
                    ItemIndex itemIdx = choice.value;
                    float weight = choice.weight - 1;
                    context.results.itemsTaken.Add(itemIdx);
                    if (weight <= 0)
                    {
                        selection.RemoveChoice(choiceIdx);
                    }
                    else
                    {
                        selection.ModifyChoiceWeight(choiceIdx, weight);
                    }
                    totalCostPaid++;
                }
            }
            foreach (ItemIndex idx in context.results.itemsTaken)
            {
                inv.RemoveItem(idx);
            }
        }

        public static bool IsAffordableDelegate(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
        {
            CharacterBody body = context.activator.GetComponent<CharacterBody>();
            if (!body) { return false; }
            Inventory inv = body.inventory;
            if (!inv) { return false; }
            int numCoral = inv.GetItemCount(KrodItems.PrismaticCoral);
            if (numCoral == 0)
            {
                return CallOriginalAffordableDelegate(costTypeDef, context);
            }
            else
            {
                return inv.HasAtLeastXTotalItemsOfTier(costTypeDef.itemTier, context.cost - numCoral);
            }
        }

        public static ItemDef ScrapItemForTier(ItemTier tier) => tier switch
        {
            ItemTier.Tier1 => RoR2Content.Items.ScrapWhite,
            ItemTier.Tier2 => RoR2Content.Items.ScrapGreen,
            ItemTier.Tier3 => RoR2Content.Items.ScrapRed,
            ItemTier.Boss => RoR2Content.Items.ScrapYellow,
            _ => throw new System.ArgumentException($"Unknown tier {tier}"),
        };
    }
}
