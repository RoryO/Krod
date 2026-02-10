using R2API;
using RoR2;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Krod.Items.Tier2
{
    public static class TheExtra
    {
        public class Behavior : CharacterBody.ItemBehavior
        {
            public float extraStopwatch;
            public void Awake()
            {
                enabled = false;
            }

            public void Update()
            {
                if (!body.outOfCombat)
                {
                    extraStopwatch += Time.deltaTime;
                    if (extraStopwatch > 8f)
                    {
                        extraStopwatch = 0;
                        if (!body || !body.teamComponent) { return; }
                        int stacks = body.inventory.GetItemCountEffective(KrodItems.TheExtra);

                        // Similar to gas formula
                        float damage = (1 + stacks) * body.damage * 0.75f;
                        Log.Info($"d: {damage}");

                        SphereSearch sphereSearch = new()
                        {
                            mask = LayerIndex.entityPrecise.mask,
                            origin = body.transform.position,
                            radius = 25 + (stacks * 5),
                            queryTriggerInteraction = QueryTriggerInteraction.Collide
                        };

                        List<Collider> results = [];
                        sphereSearch.RefreshCandidates()
                            .FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex))
                            .RefreshCandidates()
                            .GetColliders(results);
                        int actual = 0;
                        for (int i = 0; i < results.Count; ++i)
                        {
                            CharacterBody found = Util.HurtBoxColliderToBody(results[i]);
                            GameObject gameObject = (found ? found.gameObject : null);
                            if (!gameObject ||
                                body.gameObject == gameObject ||
                                found.teamComponent.teamIndex == body.teamComponent.teamIndex)
                            {
                                continue;
                            }
                            InflictDotInfo dotInfo = new()
                            {
                                attackerObject = body.gameObject,
                                victimObject = gameObject,
                                dotIndex = DotController.DotIndex.Burn,
                                totalDamage = damage,
                                maxStacksFromAttacker = (uint)stacks + 1,
                                damageMultiplier = 1f,
                            };
                            StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                            DotController.InflictDot(ref dotInfo);
                            actual++;
                        }
                        if (actual > 0)
                        {
                            EffectManager.SpawnEffect(blastSFX, new()
                            {
                                origin = body.transform.position,
                                scale = 1f
                            }, true);
                        }
                    }
                }
                else if (extraStopwatch > 0)
                {
                    extraStopwatch = 0;
                }
            }
        }

        public static GameObject blastSFX;
        public static void Awake()
        {
            KrodItems.TheExtra = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.TheExtra.canRemove = true;
            KrodItems.TheExtra.name = "THE_EXTRA_NAME";
            KrodItems.TheExtra.nameToken = "THE_EXTRA_NAME";
            KrodItems.TheExtra.pickupToken = "THE_EXTRA_PICKUP";
            KrodItems.TheExtra.descriptionToken = "THE_EXTRA_DESC";
            KrodItems.TheExtra.loreToken = "THE_EXTRA_LORE";
            KrodItems.TheExtra.tags = [ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.CanBeTemporary];
            KrodItems.TheExtra._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.TheExtra.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/TheExtra.png");
#pragma warning disable CS0618 // Type or member is obsolete
            KrodItems.TheExtra.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Assets/Items/Tier2/TheExtra.prefab");
#pragma warning restore CS0618 // Type or member is obsolete
            KrodItems.TheExtra.requiredExpansion = KrodContent.expansionDef;
            ItemAPI.Add(new CustomItem(KrodItems.TheExtra, new ItemDisplayRuleDict([])));

            blastSFX = KUtils.SFXEffect("The Extra Blast", "KTheExtraBlast");
            ContentAddition.AddEffect(blastSFX);
        }
    }
}
