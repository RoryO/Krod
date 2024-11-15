using R2API;
using Rewired;
using RoR2;
using RoR2.DirectionalSearch;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Equipment
{
    public struct ScrappableSearchFilter : IGenericDirectionalSearchFilter<GenericPickupController>
    {
        public static PickupIndex whiteScrap = PickupCatalog.FindPickupIndex("ItemIndex.ScrapWhite");
        public static PickupIndex greenScrap = PickupCatalog.FindPickupIndex("ItemIndex.ScrapGreen");
        public static PickupIndex redScrap = PickupCatalog.FindPickupIndex("ItemIndex.ScrapRed");
        public static PickupIndex yellowScrap = PickupCatalog.FindPickupIndex("ItemIndex.ScrapYellow");
        public bool PassesFilter(GenericPickupController candidateInfo)
        {
            PickupDef d = PickupCatalog.GetPickupDef(candidateInfo.pickupIndex);
            PickupIndex idx = d.pickupIndex;
            if (idx == whiteScrap ||
                idx == greenScrap ||
                idx == redScrap ||
                idx == yellowScrap)
            {
                return false;
            }
            return PickupCatalog.FindScrapIndexForItemTier(d.itemTier) != PickupIndex.none;
        }
    }
    public class ScrappableSearch : BaseDirectionalSearch<GenericPickupController, PickupSearchSelector, ScrappableSearchFilter>
    {
        public ScrappableSearch(PickupSearchSelector selector, ScrappableSearchFilter candidateFilter) : base(selector, candidateFilter)
        {
        }
    }

    public class AileensGlassEye
    {
        public class AileensGlassEyeCracked
        {
            public static EquipmentDef def;
            public static GameObject visualizerPrefab;
            public static void Awake()
            {
                def = ScriptableObject.CreateInstance<EquipmentDef>();
                def.name = "AILEENS_EYE_CRACKED_NAME";
                def.nameToken = "AILEENS_EYE_CRACKED_NAME";
                def.pickupToken = "AILEENS_EYE_CRACKED_PICKUP";
                def.descriptionToken = "AILEENS_EYE_CRACKED_DESC";
                def.loreToken = "AILEENS_EYE_CRACKED_LORE";
                def.cooldown = 60;
                def.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
                def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
                visualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Recycle/RecyclerIndicator.prefab").WaitForCompletion();

                ItemAPI.Add(new CustomEquipment(def, new ItemDisplayRuleDict(null)));
                On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction1;
                On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
            }

            public static void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
            {
                if (targetingEquipmentIndex == def.equipmentIndex)
                {
                    ScrappableSearch searcher = new ScrappableSearch(default, default);
                    Ray aimRay = self.GetAimRay();
                    aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, self.gameObject, out var extraRaycastDistance);
                    searcher.searchOrigin = aimRay.origin;
                    searcher.searchDirection = aimRay.direction;
                    searcher.minAngleFilter = 0f;
                    searcher.maxAngleFilter = 10f;
                    searcher.minDistanceFilter = 0f;
                    searcher.maxDistanceFilter = 30f + extraRaycastDistance;
                    searcher.filterByDistinctEntity = false;
                    searcher.filterByLoS = true;
                    searcher.sortMode = SortMode.DistanceAndAngle;
                    GenericPickupController found = searcher.SearchCandidatesForSingleTarget(InstanceTracker.GetInstancesList<GenericPickupController>());
                    var foundTarget = new EquipmentSlot.UserTargetInfo(found);
                    self.currentTarget = foundTarget;
                    if (self.currentTarget.transformToIndicateAt)
                    {
                        self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
                        self.targetIndicator.active = true;
                        self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
                    }
                    else
                    {
                        self.targetIndicator.active = false;
                        self.targetIndicator.targetTransform = null;
                    }
                }
                else
                {
                    orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                }
            }
            public static bool EquipmentSlot_PerformEquipmentAction1(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
            {
                if (equipmentDef == def && self.currentTarget.transformToIndicateAt)
                {
                    Transform currentTransform = self.currentTarget.rootObject.transform;
                    PickupDef d = PickupCatalog.GetPickupDef(self.currentTarget.pickupController.pickupIndex);
                    ItemTier tier = d.itemTier;
                    PickupIndex scrap = PickupCatalog.FindScrapIndexForItemTier(tier);
                    if (NetworkServer.active)
                    {
                        self.currentTarget.pickupController.SyncPickupIndex(scrap);
                    }
                    return true;
                }
                else
                {
                    return orig(self, equipmentDef);
                }
            }
        }


        public static EquipmentDef def;
        public static InteractableSpawnCard isc;
        public static void Awake()
        {
            AileensGlassEyeCracked.Awake();
            def = ScriptableObject.CreateInstance<EquipmentDef>();
            def.name = "AILEENS_EYE_NAME";
            def.nameToken = "AILEENS_EYE_NAME";
            def.pickupToken = "AILEENS_EYE_PICKUP";
            def.descriptionToken = "AILEENS_EYE_DESC";
            def.loreToken = "AILEENS_EYE_LORE";
            def.cooldown = 60;
            def.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            isc = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset").WaitForCompletion();

            ItemAPI.Add(new CustomEquipment(def, new ItemDisplayRuleDict(null)));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == def)
            {
                CharacterBody body = self?.characterBody;
                if (body == null) { return true; }
                DirectorPlacementRule rule = new DirectorPlacementRule()
                {
                    placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                    preventOverhead = true,
                    spawnOnTarget = body.coreTransform
                };
                DirectorSpawnRequest dsc = new DirectorSpawnRequest(isc, rule, RoR2Application.rng);
                DirectorCore.instance.TrySpawnObject(dsc);
                body.inventory.SetEquipmentIndex(AileensGlassEyeCracked.def.equipmentIndex);
                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
    }
}
