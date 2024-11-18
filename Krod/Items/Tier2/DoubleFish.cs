using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Items.Tier2
{
    public static class DoubleFish
    {
        public class DoubleFishBehavior : MonoBehaviour
        {
            public void Start() { }
            public void HitAgain(DamageReport damageReport) {
                StartCoroutine(DelayedHit(damageReport));
            }

            public IEnumerator DelayedHit(DamageReport damageReport) {
                DamageInfo di = new DamageInfo()
                {
                    damage = damageReport.damageDealt,
                    attacker = damageReport.attacker,
                    inflictor = damageReport.damageInfo.inflictor,
                    force = damageReport.damageInfo.force,
                    crit = damageReport.damageInfo.crit,
                    procChainMask = damageReport.damageInfo.procChainMask,
                    procCoefficient = damageReport.damageInfo.procCoefficient * 0.5f,
                    position = damageReport.victim.transform.position,
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = damageReport.damageInfo.damageType
                };
                di.AddModdedDamageType(fishDamageType);
                yield return new WaitForSeconds(0.2f);
                if (damageReport.victimBody.healthComponent.alive)
                {
                    di.position = damageReport.victim.transform.position;
                    damageReport.victimBody.healthComponent.TakeDamage(di);
                }
            }
        }
        public static ItemDef def;
        public static DamageAPI.ModdedDamageType fishDamageType;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "DOUBLEFISH_NAME";
            def.nameToken = "DOUBLEFISH_NAME";
            def.pickupToken = "DOUBLEFISH_PICKUP";
            def.descriptionToken = "DOUBLEFISH_DESC";
            def.loreToken = "DOUBLEFISH_LORE";
            def.tags = [ItemTag.Damage];
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            def.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            fishDamageType = DamageAPI.ReserveDamageType();
            On.RoR2.GlobalEventManager.ServerDamageDealt += GlobalEventManager_ServerDamageDealt;
        }

        private static void GlobalEventManager_ServerDamageDealt(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport damageReport)
        {
            orig(damageReport);
            if (damageReport.damageInfo.attacker != damageReport.victim && 
                damageReport.damageInfo.procCoefficient > 0f &&
                !damageReport.damageInfo.HasModdedDamageType(fishDamageType)) 
            {
                CharacterBody cb = damageReport?.damageInfo?.attacker?.GetComponent<CharacterBody>();
                if (cb == null) { return; }
                int c = cb.inventory.GetItemCount(def);
                if (c > 0 && Util.CheckRoll(2.5f * c, cb.master))
                {
                    CharacterBody b = damageReport.victim.GetComponent<CharacterBody>();
                    var db = b.gameObject.GetComponent<DoubleFishBehavior>();
                    if (db == null)
                    {
                        db = b.gameObject.AddComponent<DoubleFishBehavior>();
                        db.HitAgain(damageReport);
                    }
                    else
                    {
                        db.HitAgain(damageReport);
                    }
                }
            }
        }
    }
}
