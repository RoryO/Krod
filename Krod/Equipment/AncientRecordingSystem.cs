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
        public class Behavior : MonoBehaviour
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
                    GameObject fx = d.damage switch
                    {
                        < 10_000 => effect1,
                        (> 10_000 and < 100_000) => effect2,
                        (> 100_000 and < 500_000) => effect3,
                        (> 500_000) => effect4,
                        _ => effect1
                    };
                    EffectManager.SpawnEffect(fx, new()
                    {
                        origin = d.attacker.transform.position,
                        scale = 1f
                    }, true);
                }
            }
        }

        public static DamageAPI.ModdedDamageType customDamageType;
        public static GameObject effect1;
        public static GameObject effect2;
        public static GameObject effect3;
        public static GameObject effect4;
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
#pragma warning disable CS0618 // Type or member is obsolete
            KrodEquipment.AncientRecordingSystem.pickupModelPrefab =  Assets.bundle.LoadAsset<GameObject>("Assets/Equipment/AncientRecordingEquipment.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            ItemAPI.Add(new CustomEquipment(KrodEquipment.AncientRecordingSystem, new ItemDisplayRuleDict(null)));
            customDamageType = DamageAPI.ReserveDamageType();

            effect1 = KUtils.SFXEffect("Recorder FX Effect 1", "KCrowd1");
            ContentAddition.AddEffect(effect1);

            effect2 = KUtils.SFXEffect("Recorder FX Effect 2", "KCrowd2");
            ContentAddition.AddEffect(effect2);

            effect3 = KUtils.SFXEffect("Recorder FX Effect 3", "KCrowd3");
            ContentAddition.AddEffect(effect3);

            effect4 = KUtils.SFXEffect("Recorder FX Effect 4", "KCrowd4");
            ContentAddition.AddEffect(effect4);
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
                //self.targetIndicator.visualizerPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Equipment/AncientRecordingSystemIndicator.prefab");
                self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
                self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
                self.targetIndicator.active = true;
            }
            else
            {
                self.targetIndicator.active = false;
                self.targetIndicator.targetTransform = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PerformEquipmentAction(EquipmentSlot self, EquipmentDef equipmentDef)
        {
            CharacterBody body = self.characterBody;
            if (body == null) { return false; }
            Behavior b = body.gameObject.GetComponent<Behavior>();
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
            di.AddModdedDamageType(customDamageType);
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
            if (body != null && 
                body.inventory != null && 
                body.inventory.currentEquipmentIndex == KrodEquipment.AncientRecordingSystem.equipmentIndex &&
                !damageReport.damageInfo.HasModdedDamageType(customDamageType))
            {
                Behavior b = body.gameObject.GetComponent<Behavior>();
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
                Behavior b = self.gameObject.GetComponent<Behavior>();
                if (b == null)
                {
                    self.gameObject.AddComponent<Behavior>();
                }
            }
        }
    }
}
