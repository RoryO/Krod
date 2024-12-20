using R2API;
using RoR2;
using RoR2.Projectile;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Tier2
{
    // we want 95% of PrimarySkillShuriken We just don't want the auto generation of buffs,
    // which come from FixedUpdate
    // subclassing PrimarySkillShuruken and overriding FixedUpdate gets wonky
    // something from base OnInventoryChanged removes the component
    public class NinjaShowerScrubBehavior : CharacterBody.ItemBehavior
    {
        public float cooldownStopwatch = 0f;

        private SkillLocator skillLocator;

        private GameObject projectilePrefab;

        private InputBankTest inputBank;

        private void Awake()
        {
            base.enabled = false;
        }

        private void Start()
        {
            projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ShurikenProjectile");
        }

        private void OnEnable()
        {
            if ((bool)body)
            {
                body.onSkillActivatedServer += OnSkillActivated;
                skillLocator = body.GetComponent<SkillLocator>();
                inputBank = body.GetComponent<InputBankTest>();
            }
        }

        private void OnDisable()
        {
            if (body)
            {
                body.onSkillActivatedServer -= OnSkillActivated;
                while (body.HasBuff(DLC1Content.Buffs.PrimarySkillShurikenBuff))
                {
                    body.RemoveBuff(DLC1Content.Buffs.PrimarySkillShurikenBuff);
                }
            }
            inputBank = null;
            skillLocator = null;
        }

        private void OnSkillActivated(GenericSkill skill)
        {
            if (skillLocator?.primary == skill && 
                body.GetBuffCount(DLC1Content.Buffs.PrimarySkillShurikenBuff) > 0)
            {
                body.RemoveBuff(DLC1Content.Buffs.PrimarySkillShurikenBuff);
                // try base first
                var b = body.GetComponent<PrimarySkillShurikenBehavior>();
                if (b) { b.FireShuriken(); }
                else { FireShuriken(); }
            }
        }

        private void FireShuriken()
        {
            Ray aimRay = GetAimRay();
            ProjectileManager.instance.FireProjectile(projectilePrefab, 
                aimRay.origin,
                Util.QuaternionSafeLookRotation(aimRay.direction) * GetRandomRollPitch(), 
                gameObject, 
                body.damage * (4f * stack), 
                0f, 
                Util.CheckRoll(body.crit, body.master), 
                DamageColorIndex.Item
            );
        }

        private Ray GetAimRay()
        {
            if (inputBank)
            {
                return new Ray(inputBank.aimOrigin, inputBank.aimDirection);
            }
            return new Ray(transform.position, transform.forward);
        }

        protected Quaternion GetRandomRollPitch()
        {
            Quaternion quaternion = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
            Quaternion quaternion2 = Quaternion.AngleAxis(0f + Random.Range(0f, 1f), Vector3.left);
            return quaternion * quaternion2;
        }
        public void Update()
        {
            if (cooldownStopwatch > 0f)
            {
                cooldownStopwatch -= Time.deltaTime;
            }
        }

        public void TriggerDistribution()
        {
            if (cooldownStopwatch > 0f) { return; }
            if (!body.teamComponent) { return; }
            var f = TeamComponent.GetTeamMembers(body.teamComponent.teamIndex);

            for (int i = 0; i < f.Count; i++)
            {
                CharacterBody b = f[i].body;
                if (b == null) { continue; }
                if (Vector3.Distance(body.transform.position, b.transform.position) > 20f) { continue; }
                NinjaShowerScrubBehavior behavior = b.GetComponent<NinjaShowerScrubBehavior>();
                if (behavior == null) {
                    b.AddItemBehavior<NinjaShowerScrubBehavior>(stack);
                }
                b.SetBuffCount(DLC1Content.Buffs.PrimarySkillShurikenBuff.buffIndex, stack);
            }
            cooldownStopwatch = 1f;
        }
    }
    public static class NinjaShowerScrub
    {
        public static void Awake()
        {
            KrodItems.NinjaShowerScrub = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.NinjaShowerScrub.canRemove = true;
            KrodItems.NinjaShowerScrub.name = "NINJA_SHOWER_NAME";
            KrodItems.NinjaShowerScrub.nameToken = "NINJA_SHOWER_NAME";
            KrodItems.NinjaShowerScrub.pickupToken = "NINJA_SHOWER_PICKUP";
            KrodItems.NinjaShowerScrub.descriptionToken = "NINJA_SHOWER_DESC";
            KrodItems.NinjaShowerScrub.loreToken = "NINJA_SHOWER_LORE";
            KrodItems.NinjaShowerScrub.tags = [ItemTag.Damage];
            KrodItems.NinjaShowerScrub._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.NinjaShowerScrub.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/NinjaShowerScrub.png");
            KrodItems.NinjaShowerScrub.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.NinjaShowerScrub, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnInventoryChanged(CharacterBody self)
        {
            if (NetworkServer.active && self)
            {
                Inventory inventory = self.inventory ? self.inventory : null;
                if (inventory)
                {
                    int c = self.inventory.GetItemCount(KrodItems.NinjaShowerScrub);
                    self.AddItemBehavior<NinjaShowerScrubBehavior>(c);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport == null || 
                damageReport.damageInfo == null || 
                damageReport.attackerBody == null ||
                damageReport.attackerBody.inventory == null ||
                !damageReport.damageInfo.crit)
            {
                return;
            }
            if (damageReport.attackerBody.inventory.GetItemCount(KrodItems.NinjaShowerScrub) > 0)
            {
                damageReport.attackerBody.GetComponent<NinjaShowerScrubBehavior>()?.TriggerDistribution();
            }
        }
    }
}
