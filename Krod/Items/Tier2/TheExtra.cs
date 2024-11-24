using R2API;
using RoR2;
using System.Collections.Generic;
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
                    int stacks = body.inventory.GetItemCount(def);

                    // Similar to gas formula
                    float damage = stacks * body.damage * 0.75f * 2;

                    SphereSearch sphereSearch = new()
                    {
                        mask = LayerIndex.entityPrecise.mask,
                        origin = body.transform.position,
                        radius = 15 + (stacks * 5),
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
                        InflictDotInfo dotInfo = new ()
                        {
                            attackerObject = body.gameObject,
                            victimObject = gameObject,
                            dotIndex = DotController.DotIndex.Burn,
                            totalDamage = damage,
                            maxStacksFromAttacker = (uint)stacks,
                            duration = 3f,
                        };
                        StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                        DotController.InflictDot(ref dotInfo);
                    }
                }
            }
        }

        public static ItemDef def;
        public static void Awake()
        {
            def = ScriptableObject.CreateInstance<ItemDef>();
            def.canRemove = true;
            def.name = "THE_EXTRA_NAME";
            def.nameToken = "THE_EXTRA_NAME";
            def.pickupToken = "THE_EXTRA_PICKUP";
            def.descriptionToken = "THE_EXTRA_DESC";
            def.loreToken = "THE_EXTRA_LORE";
            def.tags = [ItemTag.Damage];
            def._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            def.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/TheExtra.png");
            def.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(def, new ItemDisplayRuleDict(null)));
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (NetworkServer.active && self && self.inventory)
            {
                self.AddItemBehavior<TheExtraBehavior>(self.inventory.GetItemCount(def));
            }
            orig(self);
        }
    }
}
