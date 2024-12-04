using R2API;
using RoR2;
using System.Collections;
using System.Runtime.CompilerServices;
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
                    damageType = damageReport.damageInfo.damageType,
                };
                di.AddModdedDamageType(fishDamageType);
                yield return new WaitForSeconds(0.2f);
                if (damageReport.victimBody.healthComponent.alive)
                {
                    di.position = damageReport.victim.transform.position;
                    damageReport.victimBody.healthComponent.TakeDamage(di);
                    AkSoundEngine.PostEvent("KDoubleFishStrike", damageReport.attackerBody.gameObject);
                }
            }
        }
        public static DamageAPI.ModdedDamageType fishDamageType;
        public static void Awake()
        {
            KrodItems.DoubleFish = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.DoubleFish.canRemove = true;
            KrodItems.DoubleFish.name = "DOUBLEFISH_NAME";
            KrodItems.DoubleFish.nameToken = "DOUBLEFISH_NAME";
            KrodItems.DoubleFish.pickupToken = "DOUBLEFISH_PICKUP";
            KrodItems.DoubleFish.descriptionToken = "DOUBLEFISH_DESC";
            KrodItems.DoubleFish.loreToken = "DOUBLEFISH_LORE";
            KrodItems.DoubleFish.tags = [ItemTag.Damage];
            KrodItems.DoubleFish._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.DoubleFish.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/DoubleFish.png");
            KrodItems.DoubleFish.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.DoubleFish, new ItemDisplayRuleDict(null)));
            fishDamageType = DamageAPI.ReserveDamageType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ServerDamageDealt(DamageReport damageReport)
        {
            if (damageReport.damageInfo != null &&
                damageReport.damageInfo.attacker != null &&
                damageReport.victim != null &&
                damageReport.damageInfo.attacker != damageReport.victim && 
                damageReport.damageInfo.procCoefficient > 0f &&
                !damageReport.damageInfo.HasModdedDamageType(fishDamageType)) 
            {
                CharacterBody cb = damageReport.damageInfo.attacker.GetComponent<CharacterBody>();
                if (!cb || !cb.inventory) { return; }
                int c = cb.inventory.GetItemCount(KrodItems.DoubleFish);
                if (c > 0 && Util.CheckRoll(2.5f * c, cb.master))
                {
                    CharacterBody b = damageReport.victim.GetComponent<CharacterBody>();
                    if (!b) { return; }
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
