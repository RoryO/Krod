using EntityStates.AffixVoid;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Equipment
{
    public class AncientRecordingSystem
    {
        public class AncientRecordingSystemBehavior : MonoBehaviour
        {
            public float stopwatch = 0;
            public float bestDamage = 0;
            public float runningTotalDamage = 0;
            public void Awake()
            {
                enabled = true;
            }

            public void Update()
            {
                if (stopwatch > 1f)
                {
                    bestDamage = Mathf.Max(bestDamage, runningTotalDamage);
                    stopwatch = 0;
                    runningTotalDamage = 0;
                }
                else
                {
                    stopwatch += Time.deltaTime;
                }
            }
        }
        public static EquipmentDef def;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<EquipmentDef>();
            def.name = "ANCIENT_RECORDING_NAME";
            def.nameToken = "ANCIENT_RECORDING_NAME";
            def.pickupToken = "ANCIENT_RECORDING_PICKUP";
            def.descriptionToken = "ANCIENT_RECORDING_DESC";
            def.loreToken = "ANCIENT_RECORDING_LORE";
            def.cooldown = 60;
            def.canDrop = true;
            def.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Equipment/AncientRecordingSystem.png");
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomEquipment(def, new ItemDisplayRuleDict(null)));
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.CharacterBody.OnTakeDamageServer += CharacterBody_OnTakeDamageServer;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlot_UpdateTargets;
        }

        private static void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            if (targetingEquipmentIndex == def.equipmentIndex && userShouldAnticipateTarget)
            {
                self.ConfigureTargetFinderForEnemies();
                HurtBox source = self.targetFinder.GetResults().FirstOrDefault();
                self.currentTarget = new EquipmentSlot.UserTargetInfo(source);
                if (self.currentTarget.transformToIndicateAt)
                {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
                    self.targetIndicator.active = true;
                    self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
                }
            }
            else
            {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            }
        }

        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == def)
            {
                CharacterBody body = self.characterBody;
                if (body == null) { return false; }
                AncientRecordingSystemBehavior b = body.gameObject.GetComponent<AncientRecordingSystemBehavior>();
                if (b == null) { return false; }
                Log.Info($"dazzling for {b.bestDamage}");
                if (!self.currentTarget.transformToIndicateAt) { return false; }
                DamageInfo di = new()
                {
                    attacker = self.characterBody.gameObject,
                    inflictor = self.characterBody.gameObject,
                    damage = b.bestDamage,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    position = self.currentTarget.transformToIndicateAt.position,
                    procCoefficient = 0
                };
                self.currentTarget.hurtBox.healthComponent.TakeDamage(di);
                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        private static void CharacterBody_OnTakeDamageServer(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            CharacterBody body = (damageReport.attackerBody ? damageReport.attackerBody : null);
            if (body != null && body.inventory != null && body.inventory.currentEquipmentIndex == def.equipmentIndex)
            {
                AncientRecordingSystemBehavior b = body.gameObject.GetComponent<AncientRecordingSystemBehavior>();
                if (b != null)
                {
                    b.runningTotalDamage += damageReport.damageDealt;
                }
            }
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self)
            {
                Inventory inv = (self.inventory ? self.inventory : null);
                if (inv != null &&
                    inv.currentEquipmentIndex == def.equipmentIndex)
                {
                    AncientRecordingSystemBehavior b = self.gameObject.GetComponent<AncientRecordingSystemBehavior>();
                    if (b == null)
                    {
                        self.gameObject.AddComponent<AncientRecordingSystemBehavior>();
                    }
                }
            }
        }
    }
}
