using R2API;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Tier3
{
    public static class TimsCrucible
    {
        public static BuffDef buffDef;
        public static void Awake()
        {
            KrodItems.TimsCrucible = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.TimsCrucible.canRemove = true;
            KrodItems.TimsCrucible.name = "TIMS_CRUCIBLE_NAME";
            KrodItems.TimsCrucible.nameToken = "TIMS_CRUCIBLE_NAME";
            KrodItems.TimsCrucible.pickupToken = "TIMS_CRUCIBLE_PICKUP";
            KrodItems.TimsCrucible.descriptionToken = "TIMS_CRUCIBLE_DESC";
            KrodItems.TimsCrucible.loreToken = "TIMS_CRUCIBLE_LORE";
            KrodItems.TimsCrucible.tags = [ItemTag.Utility, ItemTag.CanBeTemporary];
            KrodItems.TimsCrucible._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            KrodItems.TimsCrucible.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier3/TimsCrucible.png");
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.TimsCrucible.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier3/TimsCrucible.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            KrodItems.TimsCrucible.requiredExpansion = KrodContent.expansionDef;
            ItemAPI.Add(new CustomItem(KrodItems.TimsCrucible, new ItemDisplayRuleDict(null)));
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.canStack = false;
            buffDef.isDebuff = false;
            buffDef.isCooldown = false;
            buffDef.name = "TimIsOnFire";
            buffDef.buffColor = Color.white;
            buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            ContentAddition.AddBuffDef(buffDef);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnEquipmentLost(CharacterBody self, EquipmentDef equipmentDef)
        {
            if (self != null && equipmentDef.equipmentIndex == RoR2Content.Equipment.AffixRed.equipmentIndex)
            {
                self.RemoveBuff(buffDef);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnEquipmentGained(CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef.equipmentIndex == RoR2Content.Equipment.AffixRed.equipmentIndex &&
                self != null &&
                self.inventory != null &&
                self.inventory.GetItemCountEffective(KrodItems.TimsCrucible) > 0 &&
                NetworkServer.active)
            {
                self.AddBuff(buffDef);
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
                if (self.body.inventory.GetItemCountEffective(KrodItems.TimsCrucible) > 0 &&
                    damageInfo.dotIndex == DotController.DotIndex.Burn)
                {
                    damageInfo.rejected = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnSkillActivated(CharacterBody self, GenericSkill skill)
        {
            if (self != null && 
                self.inventory != null &&
                NetworkServer.active)
            {
                int c = self.inventory.GetItemCountEffective(KrodItems.TimsCrucible);
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
        public static void OnDotStackRemovedServer(DotController self, DotController.DotStack dotStack)
        {            
            if (dotStack.dotIndex == DotController.DotIndex.Burn)
            {
                if(self.dotStackList.Find(e => e.dotIndex == DotController.DotIndex.Burn) == null)
                {
                    CharacterBody body = self.victimBody;
                    if (body != null && body.HasBuff(buffDef))
                    {
                        body.RemoveBuff(buffDef);
                        body.RemoveBuff(RoR2Content.Buffs.AffixRed);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnDotStackAddedServer(DotController self, DotController.DotStack dotStack)
        {
            if (dotStack.dotIndex == DotController.DotIndex.Burn)
            {
                CharacterBody body = self.victimBody;
                if (body != null && 
                    body.inventory != null && 
                    body?.inventory?.GetItemCountEffective(KrodItems.TimsCrucible) > 0 && 
                    !body.HasBuff(buffDef) &&
                    NetworkServer.active)
                {
                    self.victimBody.AddBuff(buffDef);
                    self.victimBody.AddBuff(RoR2Content.Buffs.AffixRed);
                }
            }
        }
    }
}
