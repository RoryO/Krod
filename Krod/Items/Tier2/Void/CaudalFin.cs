using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.ExpansionManagement;
using System.Linq;

namespace Krod.Items.Tier2.Void
{
    public static class CaudalFin
    {
        public class Behavior : CharacterBody.ItemBehavior
        {
            public enum LaunchState
            {
                Walking,
                Sprinting,
                Charging,
                TakeoffReady,
                Launched,
            };
            public bool accelerate = false;
            public LaunchState launchState = LaunchState.Walking;

            public float launchStopwatch = 0;
            public static float launchTime = 0.75f;
            public void Awake()
            {
                enabled = false;
            }

            public void Update()
            {
                if (!body || !body.inputBank) { return; }
                if (body.isSprinting)
                {
                    if (launchState == LaunchState.Walking)
                    {
                        launchState = LaunchState.Sprinting;
                    }
                    if (launchState == LaunchState.Sprinting && body.characterMotor.isGrounded && body.inputBank.jump.down)
                    {
                        launchState = LaunchState.Charging;
                        accelerate = true;
                        launchStopwatch = 0;
                        body.RecalculateStats();
                    }
                    if (launchState == LaunchState.Charging)
                    {
                        if (launchStopwatch > launchTime)
                        {
                            launchState = LaunchState.TakeoffReady;
                        }
                        else
                        {
                            launchStopwatch += Time.deltaTime;
                        }
                    }
                }
                else if (launchState == LaunchState.Launched && 
                    body.characterMotor.isGrounded &&
                    !body.inputBank.jump.down)
                {
                    launchState = LaunchState.Walking;
                    launchStopwatch = 0;
                    accelerate = false;
                    body.RecalculateStats();
                }
            }
        }
        public static void Awake()
        {
            KrodItems.CaudalFin = ScriptableObject.CreateInstance<ItemDef>();
            KrodItems.CaudalFin.canRemove = true;
            KrodItems.CaudalFin.name = "CAUDAL_FIN_NAME";
            KrodItems.CaudalFin.nameToken = "CAUDAL_FIN_NAME";
            KrodItems.CaudalFin.pickupToken = "CAUDAL_FIN_PICKUP";
            KrodItems.CaudalFin.descriptionToken = "CAUDAL_FIN_DESC";
            KrodItems.CaudalFin.loreToken = "CAUDAL_FIN_LORE";
            KrodItems.CaudalFin.tags = [ItemTag.Utility, ItemTag.SprintRelated];
            KrodItems.CaudalFin._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier2Def.asset").WaitForCompletion();
            KrodItems.CaudalFin.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/Items/Tier2/CaudalFin.png");
            KrodItems.CaudalFin.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            ItemAPI.Add(new CustomItem(KrodItems.CaudalFin, new ItemDisplayRuleDict(null)));

            ItemDef feather = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/Feather/Feather.asset").WaitForCompletion();

            ItemRelationshipProvider provider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            provider.relationshipType = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();
            provider.relationships = [
                new ItemDef.Pair
                {
                    itemDef1 = feather,
                    itemDef2 = KrodItems.CaudalFin
                }
            ];
            ContentAddition.AddItemRelationshipProvider(provider);
        }

        public static void OnInventoryChanged(CharacterBody body)
        {
            int c = body.inventory.GetItemCount(KrodItems.CaudalFin);
            body.AddItemBehavior<Behavior>(body.inventory.GetItemCount(KrodItems.CaudalFin));
        }
        public static bool ProcessJump(EntityStates.GenericCharacterMain characterMain)
        {
            CharacterBody body = characterMain.characterBody;
            if (!body || 
                !body.isSprinting || 
                !body.inventory ||
                !body.characterMotor)
            { 
                return false; 
            }

            int c = body.inventory.GetItemCount(KrodItems.CaudalFin);
            if (c == 0) { return false; }

            Behavior b = body.GetComponent<Behavior>();
            if (!b) { return false; }

            if (characterMain.isAuthority && 
                body.characterMotor.isGrounded
            )
            {
                if (body.inputBank.jump.down)
                {
                    if (b.launchState == Behavior.LaunchState.Sprinting || b.launchState == Behavior.LaunchState.Charging)
                    {
                        return true;
                    }
                }
                else if (body.inputBank.jump.justReleased)
                {
                    b.launchState = Behavior.LaunchState.Launched;
                    b.launchStopwatch = 0;
                    EntityStates.GenericCharacterMain.ApplyJumpVelocity(body.characterMotor, body, 2, 2.3f + (0.2f * c));
                    return true;
                }
                /*
                if (NetworkServer.active)
                {
                    //body.characterMotor.Motor.ForceUnground();
                    //BlastAttack blastAttack = new()
                    //{
                    //    attacker = body.gameObject,
                    //    radius = 14,
                    //    procCoefficient = 0,
                    //    position = body.corePosition,
                    //    crit = false,
                    //    baseDamage = body.baseDamage,
                    //    falloffModel = BlastAttack.FalloffModel.None,
                    //    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    //    damageType = DamageType.Stun1s,
                    //    baseForce = 2200,
                    //    teamIndex = body.teamComponent.teamIndex,
                    //};
                    //blastAttack.damageType.damageSource = DamageSource.Special;
                    //blastAttack.Fire();
                }
                */
            }
            return false;
        }
    }
}
