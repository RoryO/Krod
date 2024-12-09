using R2API;
using RoR2;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public class AncientRecordingSystemDelayedHit : MonoBehaviour
        {
            public void Start() { }
            public HealthComponent healthComponent;

            public void HitWith(DamageInfo d)
            {
                StartCoroutine(DelayedHit(d));
            }
            public IEnumerator DelayedHit(DamageInfo d) 
            { 
                yield return new WaitForSeconds(2f);
                if (healthComponent && 
                    healthComponent.alive &&
                    healthComponent.body) {
                    d.position = healthComponent.body.transform.position;
                    healthComponent.TakeDamage(d);
                    string eventName = d.damage switch
                    {
                        < 10_000 => "KCrowd1",
                        (> 10_000 and < 100_000) => "KCrowd2",
                        (> 100_000 and < 500_000) => "KCrowd3",
                        (> 500_000) => "KCrowd4",
                        _ => "KCrowd1"
                    };
                    Log.Info(eventName);
                    AkSoundEngine.PostEvent(eventName, d.attacker.gameObject);
                }
            }
        }
        public static void Awake()
        {
            KrodEquipment.AncientRecordingSystem = ScriptableObject.CreateInstance<EquipmentDef>();
            KrodEquipment.AncientRecordingSystem.name = "ANCIENT_RECORDING_NAME";
            KrodEquipment.AncientRecordingSystem.nameToken = "ANCIENT_RECORDING_NAME";
            KrodEquipment.AncientRecordingSystem.pickupToken = "ANCIENT_RECORDING_PICKUP";
            KrodEquipment.AncientRecordingSystem.descriptionToken = "ANCIENT_RECORDING_DESC";
            KrodEquipment.AncientRecordingSystem.loreToken = "ANCIENT_RECORDING_LORE";
            KrodEquipment.AncientRecordingSystem.cooldown = 60;
            KrodEquipment.AncientRecordingSystem.canDrop = true;
            KrodEquipment.AncientRecordingSystem.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Equipment/AncientRecordingSystem.png");
            KrodEquipment.AncientRecordingSystem.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomEquipment(KrodEquipment.AncientRecordingSystem, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateTargets(EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            if (!userShouldAnticipateTarget) { return; }
            self.ConfigureTargetFinderForEnemies();
            HurtBox source = self.targetFinder.GetResults().FirstOrDefault();
            self.currentTarget = new EquipmentSlot.UserTargetInfo(source);
            // odd way of doing this. copying exactly what the internal method does. keep getting NREs, this way might fix it.
            bool present = self.currentTarget.transformToIndicateAt;
            if (present)
            {
                self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
            }
            self.targetIndicator.active = present;
            self.targetIndicator.targetTransform = present ? self.currentTarget.transformToIndicateAt : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PerformEquipmentAction(EquipmentSlot self, EquipmentDef equipmentDef)
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
            self.currentTarget.hurtBox.healthComponent.GetComponent<SetStateOnHurt>()?.SetStun(2f);

            var d = self.currentTarget.hurtBox.healthComponent.gameObject.AddComponent<AncientRecordingSystemDelayedHit>();
            d.healthComponent = self.currentTarget.hurtBox.healthComponent;
            d.HitWith(di);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnTakeDamageServer(CharacterBody self, DamageReport damageReport)
        {
            CharacterBody body = (damageReport.attackerBody ? damageReport.attackerBody : null);
            if (body != null && body.inventory != null && body.inventory.currentEquipmentIndex == KrodEquipment.AncientRecordingSystem.equipmentIndex)
            {
                AncientRecordingSystemBehavior b = body.gameObject.GetComponent<AncientRecordingSystemBehavior>();
                if (b != null)
                {
                    b.runningTotalDamage += damageReport.damageDealt;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnInventoryChanged(CharacterBody self)
        {
            if (self.inventory.currentEquipmentIndex == KrodEquipment.AncientRecordingSystem.equipmentIndex)
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
