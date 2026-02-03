using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

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
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.PrismaticCoral.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier2/PrismaticCoral.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            KrodItems.PrismaticCoral.requiredExpansion = KrodContent.expansionDef;
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

        public static void CallOriginalPayDelegate(CostTypeDef.PayCostContext context, CostTypeDef.PayCostResults results)
        {
                switch(context.costTypeDef.itemTier)
                {
                    case ItemTier.Tier1:
                        origPayWhiteItemDelegate(context, results);
                        break;
                    case ItemTier.Tier2:
                        origPayGreenItemDelegate(context, results);
                        break;
                    case ItemTier.Tier3:
                        origPayRedItemDelegate(context, results);
                        break;
                    case ItemTier.Boss:
                        origPayBossItemDelegate(context, results);
                        break;
                    default:
                        throw new System.Exception($"Unsupported tier {context.costTypeDef.itemTier}");
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

        public static void PayCostWithCoral(CostTypeDef.PayCostContext context, CostTypeDef.PayCostResults results)
        {
            CharacterBody body = context.activator.GetComponent<CharacterBody>();
            if (!body) { return; }
            Inventory inv = body.inventory;
            if (!inv) { return; }
            int numCoral = inv.GetItemCountEffective(KrodItems.PrismaticCoral);
            if (numCoral == 0)
            {
                CallOriginalPayDelegate(context, results);
                return;
            }

            ItemDef scrap = ScrapItemForTier(context.costTypeDef.itemTier);
            int numScrapHeld = inv.GetItemCountEffective(scrap);

            if (numScrapHeld >= context.cost)
            {
                CallOriginalPayDelegate(context, results);
                return;
            }

            int totalCostPaid = 0;
            List<ItemIndex> takenItems = [];

            while (numScrapHeld > 0)
            {
                takenItems.Add(scrap.itemIndex);
                numScrapHeld--;
                totalCostPaid++;
            }

            while (numCoral > 0)
            {
                takenItems.Add(KrodItems.PrismaticCoral.itemIndex);
                numCoral--;
                totalCostPaid++;
                if (totalCostPaid >= context.cost) { break; }
            }

            if (totalCostPaid < context.cost)
            {
                var selection = new WeightedSelection<ItemIndex>();
                foreach (var idx in ItemCatalog.allItems)
                {
                    if (ItemCatalog.GetItemDef(idx).tier == context.costTypeDef.itemTier &&
                        (idx != scrap.itemIndex || idx != context.avoidedItemIndex)
                    )
                    {
                        int c = inv.GetItemCountEffective(idx);
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
                    takenItems.Add(itemIdx);
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
            foreach (ItemIndex idx in takenItems)
            {
                Inventory.ItemTransformation itemTransformation = default;
                itemTransformation.originalItemIndex = idx;
                itemTransformation.newItemIndex = ItemIndex.None;
                itemTransformation.maxToTransform = 1;
                itemTransformation.forbidTempItems = true;
                if (itemTransformation.TryTransform(inv, out var r))
                {
                    results.AddTakenItems(in r.takenItem);
                }
            }
        }

        public static bool IsAffordableDelegate(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
        {
            CharacterBody body = context.activator.GetComponent<CharacterBody>();
            if (!body) { return false; }
            Inventory inv = body.inventory;
            if (!inv) { return false; }
            int numCoral = inv.GetItemCountEffective(KrodItems.PrismaticCoral);
            if (numCoral == 0)
            {
                return CallOriginalAffordableDelegate(costTypeDef, context);
            }
            else
            {
                return numCoral >= context.cost ||
                       inv.HasAtLeastXTotalRemovablePermanentItemsOfTier(costTypeDef.itemTier, context.cost - numCoral);
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
