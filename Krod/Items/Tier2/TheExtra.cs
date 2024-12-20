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
        public class TheExtraBehavior : CharacterBody.ItemBehavior
        {
            public float extraStopwatch;
            public void Awake()
            {
                enabled = false;
            }

            public void Update()
            {
                extraStopwatch += Time.deltaTime;
                if (extraStopwatch > 5f)
                {
                    extraStopwatch = 0;
                    if (!body || !body.teamComponent) { return; }
                    int stacks = body.inventory.GetItemCount(KrodItems.TheExtra);

                    // Similar to gas formula
                    float damage = (1+stacks) * body.damage * 0.75f;
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
                    }
                }
            }
        }

        public static void Awake()
        {
            KrodItems.TheExtra = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.TheExtra.canRemove = true;
            KrodItems.TheExtra.name = "THE_EXTRA_NAME";
            KrodItems.TheExtra.nameToken = "THE_EXTRA_NAME";
            KrodItems.TheExtra.pickupToken = "THE_EXTRA_PICKUP";
            KrodItems.TheExtra.descriptionToken = "THE_EXTRA_DESC";
            KrodItems.TheExtra.loreToken = "THE_EXTRA_LORE";
            KrodItems.TheExtra.tags = [ItemTag.Damage, ItemTag.AIBlacklist];
            KrodItems.TheExtra._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            KrodItems.TheExtra.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/TheExtra.png");
            KrodItems.TheExtra.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.TheExtra, new ItemDisplayRuleDict(null)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnInventoryChanged(CharacterBody self)
        {
            self.AddItemBehavior<TheExtraBehavior>(self.inventory.GetItemCount(KrodItems.TheExtra));
        }
    }
}
