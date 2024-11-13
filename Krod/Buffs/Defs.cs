using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod.Buffs
{
    public static class Defs
    {
        public static BuffDef TimIsOnFire;

        public static void Awake()
        {
            TimIsOnFire = ScriptableObject.CreateInstance<BuffDef>();
            TimIsOnFire.canStack = false;
            TimIsOnFire.isDebuff = false;
            TimIsOnFire.isCooldown = false;
            TimIsOnFire.name = "TimIsOnFire";
            TimIsOnFire.buffColor = Color.white;
            TimIsOnFire.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            if (!ContentAddition.AddBuffDef(TimIsOnFire))
            {
                Log.Error("Could not add buff def");
            }
        }
    }
}
