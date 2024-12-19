using Krod.Buffs;
using R2API;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier3
{
    public static class TimsCrucible
    {
        public static void Awake()
        {
            KrodItems.TimsCrucible = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.TimsCrucible.canRemove = true;
            KrodItems.TimsCrucible.name = "TIMS_CRUCIBLE_NAME";
            KrodItems.TimsCrucible.nameToken = "TIMS_CRUCIBLE_NAME";
            KrodItems.TimsCrucible.pickupToken = "TIMS_CRUCIBLE_PICKUP";
            KrodItems.TimsCrucible.descriptionToken = "TIMS_CRUCIBLE_DESC";
            KrodItems.TimsCrucible.loreToken = "TIMS_CRUCIBLE_LORE";
            KrodItems.TimsCrucible.tags = [ItemTag.Utility];
            KrodItems.TimsCrucible._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            KrodItems.TimsCrucible.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            KrodItems.TimsCrucible.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.TimsCrucible, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnEquipmentLost(CharacterBody self, EquipmentDef equipmentDef)
        {
            if (self != null && equipmentDef.equipmentIndex == RoR2Content.Equipment.AffixRed.equipmentIndex)
            {
                self.RemoveBuff(Defs.TimIsOnFire);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnEquipmentGained(CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef.equipmentIndex == RoR2Content.Equipment.AffixRed.equipmentIndex &&
                self != null &&
                self.inventory != null &&
                self.inventory.GetItemCount(KrodItems.TimsCrucible) > 0)
            {
                self.AddBuff(Defs.TimIsOnFire);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TakeDamage(HealthComponent self, DamageInfo damageInfo)
        {
            if (self != null && 
                damageInfo != null &&
                self.body != null && 
                self.body.inventory != null)
            {
                if (self.body.inventory.GetItemCount(KrodItems.TimsCrucible) > 0 &&
                    damageInfo.dotIndex == DotController.DotIndex.Burn)
                {
                    damageInfo.rejected = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnSkillActivated(CharacterBody self, GenericSkill skill)
        {
            if (self != null && self.inventory != null)
            {
                int c = self.inventory.GetItemCount(KrodItems.TimsCrucible);
                if (c > 0)
                {
                    string n = (skill.skillFamily as ScriptableObject).name;
                    if (n.Contains("Utility"))
                    {
                        var d = DotController.FindDotController(self.gameObject);
                        if (d == null || 
                            d.dotStackList.Find(e => e.dotIndex == DotController.DotIndex.Burn) == null)
                        {
                            InflictDotInfo idi = new()
                            {
                                attackerObject = self.gameObject,
                                victimObject = self.gameObject,
                                duration = 2.5f + Mathf.Min((c * 0.5f), 5f),
                                dotIndex = DotController.DotIndex.Burn,
                            };
                            DotController.InflictDot(ref idi);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender != null && 
                sender.inventory !=null && 
                sender.HasBuff(Defs.TimIsOnFire))
            {
                int m = sender.inventory.GetItemCount(KrodItems.TimsCrucible);
                args.attackSpeedMultAdd += 0.3f * m;
                args.armorAdd += 20 * m;
                args.moveSpeedMultAdd += 0.2f * m;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnDotStackRemovedServer(DotController self, object _dotStack)
        {            
            if (_dotStack is DotController.DotStack dotStack && dotStack.dotIndex == DotController.DotIndex.Burn)
            {
                if(self.dotStackList.Find(e => e.dotIndex == DotController.DotIndex.Burn) == null)
                {
                    CharacterBody body = self.victimBody;
                    if (body != null && body.HasBuff(Defs.TimIsOnFire))
                    {
                        body.RemoveBuff(Defs.TimIsOnFire);
                        body.RemoveBuff(RoR2Content.Buffs.AffixRed);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnDotStackAddedServer(DotController self, object _dotStack)
        {
            if (_dotStack is DotController.DotStack dotStack && dotStack.dotIndex == DotController.DotIndex.Burn)
            {
                CharacterBody body = self.victimBody;
                if (body != null && 
                    body.inventory != null && 
                    body?.inventory?.GetItemCount(KrodItems.TimsCrucible) > 0 && 
                    !body.HasBuff(Defs.TimIsOnFire))
                {
                    self.victimBody.AddBuff(Defs.TimIsOnFire);
                    self.victimBody.AddBuff(RoR2Content.Buffs.AffixRed);
                }
            }
        }
    }
}
