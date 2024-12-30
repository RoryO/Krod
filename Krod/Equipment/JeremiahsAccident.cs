using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Runtime.CompilerServices;
using System;
using EntityStates.Toolbot;
using System.Numerics;
using UnityEngine.Networking;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace Krod.Equipment
{
    public static class JeremiahsAccident
    {

        public class JeremiahsAccidentBehavior : MonoBehaviour
        {
            public uint? droneEventId;
            public void OnDisable()
            {
                if (droneEventId.HasValue)
                {
                    AkSoundEngine.StopPlayingID(droneEventId.Value);
                    droneEventId = null;
                }
            }
        }

        public static DamageAPI.ModdedDamageType damageType;

        public static void Awake()
        {
            KrodEquipment.JeremiahsAccident = ScriptableObject.CreateInstance<EquipmentDef>();
            KrodEquipment.JeremiahsAccident.name = "JEREMIAHS_ACCIDENT_NAME";
            KrodEquipment.JeremiahsAccident.nameToken = "JEREMIAHS_ACCIDENT_NAME";
            KrodEquipment.JeremiahsAccident.pickupToken = "JEREMIAHS_ACCIDENT_PICKUP";
            KrodEquipment.JeremiahsAccident.descriptionToken = "JEREMIAHS_ACCIDENT_DESC";
            KrodEquipment.JeremiahsAccident.loreToken = "JEREMIAHS_ACCIDENT_LORE";
            KrodEquipment.JeremiahsAccident.cooldown = 20;
            KrodEquipment.JeremiahsAccident.canDrop = true;
            KrodEquipment.JeremiahsAccident.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Equipment/JeremiahsAccident.png");
            KrodEquipment.JeremiahsAccident.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomEquipment(KrodEquipment.JeremiahsAccident, new ItemDisplayRuleDict(null)));
            damageType = DamageAPI.ReserveDamageType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PerformEquipmentAction(EquipmentSlot self, EquipmentDef equipmentDef)
        {
            CharacterBody body = self.characterBody;
            if (!body) { return false; }
            Ray ra = body.inputBank.GetAimRay();
            BulletAttack at = new()
            {
                damage = body.damage * 8,
                aimVector = ra.direction,
                origin = ra.origin,
                owner = body.gameObject,
                falloffModel = BulletAttack.FalloffModel.None,
                hitCallback = HitCallback,
                stopperMask = LayerIndex.world.mask,
                tracerEffectPrefab = BaseNailgunState.tracerEffectPrefab,
                procCoefficient = 0,
            };
            at.AddModdedDamageType(damageType);
            at.Fire();
            return true;
        }

        private static bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if (!hitInfo.hitHurtBox) { return false; }
            HealthComponent hc = hitInfo.hitHurtBox.healthComponent;
            if (!hc) { return false; }

            CharacterBody cb = hc.body;
            if (!cb)
            { 
                return false;
            }
            if (!cb.isBoss && cb.isElite && (cb.characterMotor && !cb.characterMotor.isGrounded))
            {
                cb.master.TrueKill();
                return true;
            }
            else
            {
                return BulletAttack.defaultHitCallback(bulletAttack, ref hitInfo);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnEquipmentGained(CharacterBody self, EquipmentDef _equipmentDef)
        {
            JeremiahsAccidentBehavior c = self.gameObject.GetComponent<JeremiahsAccidentBehavior>();
            if (!c)
            {
                c = self.gameObject.AddComponent<JeremiahsAccidentBehavior>();
            }
            c.droneEventId = AkSoundEngine.PostEvent("KRailDrone", self.gameObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnEquipmentLost(CharacterBody self, EquipmentDef equipmentDef)
        {
            JeremiahsAccidentBehavior c = self.gameObject.GetComponent<JeremiahsAccidentBehavior>();
            if (c && c.droneEventId.HasValue)
            {
                AkSoundEngine.StopPlayingID(c.droneEventId.Value);
                c.droneEventId = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnCharacterDeath(GlobalEventManager self, DamageReport damageReport)
        {
            if(!DamageAPI.HasModdedDamageType(damageReport.damageInfo, damageType)) { return; }
            float overkillAmount = Math.Abs(damageReport.combinedHealthBeforeDamage - damageReport.damageDealt);

            if (overkillAmount > (damageReport.victimBody.healthComponent.fullCombinedHealth * 0.5))
            {
                // lifted from shatterspleen
                Util.PlaySound("Play_bleedOnCritAndExplode_explode", damageReport.victimBody.gameObject);
                Vector3 pos = damageReport.victimBody.transform.position;
                float baseDamage = Util.OnKillProcDamage(damageReport.attackerBody.damage, 4f) + damageReport.victimBody.maxHealth * 0.15f;
                GameObject o = UnityEngine.Object.Instantiate(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeBlastEffect, pos, Quaternion.identity);
                DelayBlast delayBlast = o.GetComponent<DelayBlast>();
                delayBlast.position = pos;
                delayBlast.baseDamage = baseDamage;
                delayBlast.baseForce = 0f;
                delayBlast.radius = 16f;
                delayBlast.attacker = damageReport.damageInfo.attacker;
                delayBlast.inflictor = null;
                delayBlast.crit = Util.CheckRoll(damageReport.attackerBody.crit, damageReport.attackerMaster);
                delayBlast.maxTimer = 0f;
                delayBlast.damageColorIndex = DamageColorIndex.Item;
                delayBlast.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                o.GetComponent<TeamFilter>().teamIndex = damageReport.attackerTeamIndex;
                NetworkServer.Spawn(o);
            }
        }
    }
}
