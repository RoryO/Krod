using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.Networking;

namespace Krod.Items.Tier2
{
    public static class TheExtra
    {
        public class TheExtraBehavior : CharacterBody.ItemBehavior
        {
            public float stopwatch;
            public void Awake()
            {
                enabled = false;
            }

            public void Update()
            {
                stopwatch += Time.deltaTime;
                if (stopwatch > 5f)
                {
                    stopwatch = 0;
                    if (!body || !body.teamComponent) { return; }
                    int stacks = body.inventory.GetItemCount(def);

                    // Similar to gas formula
                    float damage = stacks * body.damage * 0.75f;

                    TeamDef team = TeamCatalog.GetTeamDef(body.teamComponent.teamIndex);
                    // Internal warnings about resizing the array, which then does this, so lets just do this anyway
                    Collider[] colliders = new Collider[100];
                    int numFound = HGPhysics.OverlapSphere(out colliders, body.transform.position, 
                        15 + (stacks * 5), 
                        LayerIndex.entityPrecise.mask, 
                        QueryTriggerInteraction.Collide);
                    GameObject[] gameObjects = new GameObject[colliders.Length];
                    for (int i = 0; i < numFound; ++i)
                    {
                        CharacterBody found = Util.HurtBoxColliderToBody(colliders[i]);
                        GameObject gameObject = (found ? found.gameObject : null);
                        if (!gameObject || found.teamComponent.teamIndex == body.teamComponent.teamIndex) 
                        { 
                            continue; 
                        }
                        InflictDotInfo dotInfo = new InflictDotInfo()
                        {
                            attackerObject = body.gameObject,
                            victimObject = gameObject,
                            dotIndex = DotController.DotIndex.Burn,
                            totalDamage = damage,
                            maxStacksFromAttacker = (uint)stacks
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
        }
    }
}
