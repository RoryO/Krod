using R2API;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace Krod.Equipment
{
    public class JeffsServiceMedal
    {
        public static EquipmentDef def;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<EquipmentDef>();
            def.name = "JEFFS_SERVICE_NAME";
            def.nameToken = "JEFFS_SERVICE_NAME";
            def.pickupToken = "JEFFS_SERVICE_PICKUP";
            def.descriptionToken = "JEFFS_SERVICE_DESC";
            def.loreToken = "JEFFS_SERVICE_LORE";
            def.cooldown = 180;
            def.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomEquipment(def, new ItemDisplayRuleDict(null)));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += CharacterMaster_GetDeployableSameSlotLimit;
        }

        private static int CharacterMaster_GetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            if (slot == DeployableSlot.MinorConstructOnKill)
            {
                int c = self.inventory.GetItemCount(DLC1Content.Items.MinorConstructOnKill) * 4;
                if (self.inventory.GetEquipment(self.inventory.activeEquipmentSlot).equipmentIndex == def.equipmentIndex)
                {
                    c += 4;
                }
                return c;
            }
            else
            {
                return orig(self, slot);
            }
        }

        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef.equipmentIndex == def.equipmentIndex)
            {
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
            return orig(self, equipmentDef);
        }
    }
}
