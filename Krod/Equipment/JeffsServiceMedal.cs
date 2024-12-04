using R2API;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Runtime.CompilerServices;

namespace Krod.Equipment
{
    public class JeffsServiceMedal
    {
        public static void Awake()
        {
            KrodEquipment.JeffsServiceMedal = ScriptableObject.CreateInstance<EquipmentDef>();
            KrodEquipment.JeffsServiceMedal.name = "JEFFS_SERVICE_NAME";
            KrodEquipment.JeffsServiceMedal.nameToken = "JEFFS_SERVICE_NAME";
            KrodEquipment.JeffsServiceMedal.pickupToken = "JEFFS_SERVICE_PICKUP";
            KrodEquipment.JeffsServiceMedal.descriptionToken = "JEFFS_SERVICE_DESC";
            KrodEquipment.JeffsServiceMedal.loreToken = "JEFFS_SERVICE_LORE";
            KrodEquipment.JeffsServiceMedal.cooldown = 180;
            KrodEquipment.JeffsServiceMedal.canDrop = true;
            KrodEquipment.JeffsServiceMedal.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Equipment/JeffsServiceMedal.png");
            KrodEquipment.JeffsServiceMedal.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomEquipment(KrodEquipment.JeffsServiceMedal, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetDeployableSameSlotLimit(CharacterMaster self, DeployableSlot slot)
        {
            if (self.inventory)
            {
                int c = self.inventory.GetItemCount(DLC1Content.Items.MinorConstructOnKill) * 4;
                if (self.inventory.GetEquipment(self.inventory.activeEquipmentSlot).equipmentIndex == KrodEquipment.JeffsServiceMedal.equipmentIndex)
                {
                    c += 4;
                }
                return c;
            }
            else
            {
                return 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool PerformEquipmentAction(EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (!self || !self.characterBody) { return false; }
            var transform = self.characterBody.transform;
            for (int i = 0; i < 4; i++)
            {
                Vector3 f = new Vector3();
                switch (i)
                {
                    case 0:
                        {
                            f = transform.forward * 20f;
                            break;
                        }
                    case 1:
                        {
                            f = transform.forward * -20f;
                            break;
                        }
                    case 2:
                        {
                            f = transform.right * 20f;
                            break;
                        }
                    case 3:
                        {
                            f = transform.right * -20f;
                            break;
                        }
                }
                Vector3 forward = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * Quaternion.AngleAxis(-80f, Vector3.right) * Vector3.forward;
                FireProjectileInfo fireProjectileInfo = default;
                fireProjectileInfo.projectilePrefab = GlobalEventManager.CommonAssets.minorConstructOnKillProjectile;
                fireProjectileInfo.position = transform.position + transform.forward + f;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(forward);
                fireProjectileInfo.procChainMask = default;
                fireProjectileInfo.owner = self.characterBody.gameObject;
                fireProjectileInfo.damage = 0f;
                fireProjectileInfo.crit = false;
                fireProjectileInfo.force = 0f;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            return true;
        }
    }
}
