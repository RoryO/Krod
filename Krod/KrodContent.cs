using R2API;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Krod
{
    public static class KrodContent
    {
        public static ExpansionDef expansionDef;
        public static void Awake()
        {
            expansionDef = ScriptableObject.CreateInstance<ExpansionDef>();
            expansionDef.nameToken = "KROD_EXPANSION_NAME";
            expansionDef.descriptionToken = "KROD_EXPANSION_DESCRIPTION";
            expansionDef.iconSprite = Assets.bundle.LoadAsset<Sprite>("Assets/krod-expansion-enabled.png");
            expansionDef.disabledIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
            ContentAddition.AddExpansionDef(expansionDef);
        }
    }
}
