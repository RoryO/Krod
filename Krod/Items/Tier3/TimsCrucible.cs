using Krod.Buffs;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier3
{
    public static class TimsCrucible
    {
        public static ItemDef def;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "TIMS_CRUCIBLE_NAME";
            def.nameToken = "TIMS_CRUCIBLE_NAME";
            def.pickupToken = "TIMS_CRUCIBLE_PICKUP";
            def.descriptionToken = "TIMS_CRUCIBLE_DESC";
            def.loreToken = "TIMS_CRUCIBLE_LORE";
            def.tags = [ItemTag.Utility];
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            def.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            On.RoR2.DotController.OnDotStackAddedServer += DotController_OnDotStackAddedServer;
            On.RoR2.DotController.OnDotStackRemovedServer += DotController_OnDotStackRemovedServer;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.OnEquipmentGained += CharacterBody_OnEquipmentGained;
            On.RoR2.CharacterBody.OnEquipmentLost += CharacterBody_OnEquipmentLost;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private static void CharacterBody_OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef.equipmentIndex == RoR2Content.Equipment.AffixRed.equipmentIndex)
            {
                self.RemoveBuff(Defs.TimIsOnFire);
            }
            orig(self, equipmentDef);
        }

        private static void CharacterBody_OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (equipmentDef.equipmentIndex == RoR2Content.Equipment.AffixRed.equipmentIndex &&
                (self?.inventory?.GetItemCount(def) ?? 0) > 0)
            {
                self.AddBuff(Defs.TimIsOnFire);
            }
            orig(self, equipmentDef);
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            Inventory i = self?.body?.inventory;
            if (i != null &&
                damageInfo.dotIndex == DotController.DotIndex.Burn &&
                i.GetItemCount(def) > 0)
            {
                damageInfo.damage = 0;
            }
            orig(self, damageInfo);
        }

        private static void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);
            int c = self?.inventory?.GetItemCount(def) ?? 0;
            if (c > 0)
            {
                string n = (skill.skillFamily as ScriptableObject).name; 
                if (n.Contains("Utility"))
                {
                    var d = DotController.FindDotController(self.gameObject);
                    if (d?.dotStackList.Find(e => e.dotIndex == DotController.DotIndex.Burn) == null)
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

        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            self.RecalculateStats();
        }

        private static void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            self.RecalculateStats();
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender?.inventory && sender.HasBuff(Defs.TimIsOnFire))
            {
                int m = sender.inventory.GetItemCount(def.itemIndex);
                args.attackSpeedMultAdd = 0.3f * m;
                args.armorAdd = 20 * m;
                args.moveSpeedMultAdd = 0.2f * m;
            }
        }

        private static void DotController_OnDotStackRemovedServer(On.RoR2.DotController.orig_OnDotStackRemovedServer orig, DotController self, object _dotStack)
        {            
            orig(self, _dotStack);
            if (_dotStack is DotController.DotStack dotStack && dotStack.dotIndex == DotController.DotIndex.Burn)
            {
                if(self.dotStackList.Find(e => e.dotIndex == DotController.DotIndex.Burn) == null)
                {
                    CharacterBody body = self?.victimBody;
                    if (body?.HasBuff(Defs.TimIsOnFire) ?? false)
                    {
                        body.RemoveBuff(Defs.TimIsOnFire);
                        body.RemoveBuff(RoR2Content.Buffs.AffixRed);
                    }
                }
            }
        }

        private static void DotController_OnDotStackAddedServer(On.RoR2.DotController.orig_OnDotStackAddedServer orig, DotController self, object _dotStack)
        {
            orig(self, _dotStack);
            if (_dotStack is DotController.DotStack dotStack && dotStack.dotIndex == DotController.DotIndex.Burn)
            {
                CharacterBody body = self.victimBody;
                if ((body?.inventory?.GetItemCount(def) ?? 0) > 0 && 
                    !body.HasBuff(Defs.TimIsOnFire))
                {
                    self.victimBody.AddBuff(Defs.TimIsOnFire);
                    self.victimBody.AddBuff(RoR2Content.Buffs.AffixRed);
                }
            }
        }
    }
}
