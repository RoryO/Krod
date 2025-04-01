using R2API;
using RoR2;
using RoR2.DirectionalSearch;
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

    public static class AileensGlassEye
    {
        public class AileensGlassEyeCracked
        {
            public static GameObject visualizerPrefab;
            public static void Awake()
            {
                KrodEquipment.AileensGlassEyeCracked = ScriptableObject.CreateInstance<EquipmentDef>();
                KrodEquipment.AileensGlassEyeCracked.name = "AILEENS_EYE_CRACKED_NAME";
                KrodEquipment.AileensGlassEyeCracked.nameToken = "AILEENS_EYE_CRACKED_NAME";
                KrodEquipment.AileensGlassEyeCracked.pickupToken = "AILEENS_EYE_CRACKED_PICKUP";
                KrodEquipment.AileensGlassEyeCracked.descriptionToken = "AILEENS_EYE_CRACKED_DESC";
                KrodEquipment.AileensGlassEyeCracked.loreToken = "AILEENS_EYE_CRACKED_LORE";
                KrodEquipment.AileensGlassEyeCracked.cooldown = 60;
                KrodEquipment.AileensGlassEyeCracked.canDrop = false;
                KrodEquipment.AileensGlassEyeCracked.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Equipment/AileensEyeCracked.png");
                KrodEquipment.AileensGlassEyeCracked.pickupModelPrefab =  Assets.bundle.LoadAsset<GameObject>("Assets/Equipment/AileensGlassEyeCracked.prefab");
                KrodEquipment.AileensGlassEyeCracked.canBeRandomlyTriggered = false;
                visualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Recycle/RecyclerIndicator.prefab").WaitForCompletion();

                ItemAPI.Add(new CustomEquipment(KrodEquipment.AileensGlassEyeCracked, new ItemDisplayRuleDict(null)));
            }

            public static void UpdateTargets(EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
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
            public static bool PerformEquipmentAction(EquipmentSlot self, EquipmentDef equipmentDef)
            {
                if (!self || 
                    !self.currentTarget.transformToIndicateAt)
                {
                    return false;
                }
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
        }


        public static InteractableSpawnCard isc;
        public static GameObject createScrapperEffect;
        public static void Awake()
        {
            AileensGlassEyeCracked.Awake();

            KrodEquipment.AileensGlassEye = ScriptableObject.CreateInstance<EquipmentDef>();
            KrodEquipment.AileensGlassEye.name = "AILEENS_EYE_NAME";
            KrodEquipment.AileensGlassEye.nameToken = "AILEENS_EYE_NAME";
            KrodEquipment.AileensGlassEye.pickupToken = "AILEENS_EYE_PICKUP";
            KrodEquipment.AileensGlassEye.descriptionToken = "AILEENS_EYE_DESC";
            KrodEquipment.AileensGlassEye.loreToken = "AILEENS_EYE_LORE";
            KrodEquipment.AileensGlassEye.cooldown = 60;
            KrodEquipment.AileensGlassEye.canDrop = true;
            KrodEquipment.AileensGlassEye.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Equipment/AileensEye.png");
            KrodEquipment.AileensGlassEye.pickupModelPrefab =  Assets.bundle.LoadAsset<GameObject>("Assets/Equipment/AileensGlassEye.prefab");
            ItemAPI.Add(new CustomEquipment(KrodEquipment.AileensGlassEye, new ItemDisplayRuleDict(null)));

            createScrapperEffect = new("Create Scrapper Effect", 
                typeof(EffectComponent), 
                typeof(VFXAttributes));
            Object.DontDestroyOnLoad(createScrapperEffect);
            EffectComponent ec = createScrapperEffect.GetComponent<EffectComponent>();
            ec.noEffectData = true;
            ec.soundName = "KCreateScrapper";
            ContentAddition.AddEffect(createScrapperEffect);
        }

        public static bool PerformEquipmentAction(EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (!createScrapperEffect)
            {
                Log.Error("where is the effect");
            }
            if (!self || !self.characterBody) { return false; }
            CharacterBody body = self.characterBody;
            DirectorPlacementRule rule = new DirectorPlacementRule()
            {
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                preventOverhead = true,
                spawnOnTarget = body.coreTransform
            };
            DirectorSpawnRequest dsc = new(isc, rule, RoR2Application.rng);
            GameObject created = DirectorCore.instance.TrySpawnObject(dsc);
            body.inventory.SetEquipmentIndex(KrodEquipment.AileensGlassEyeCracked.equipmentIndex);
            EffectManager.SpawnEffect(createScrapperEffect, new EffectData()
            {
                origin = created.gameObject.transform.position,
                scale = 1f,
            }, true);
            return true;
        }
    }
}
