using UnityEngine;
using RoR2;

namespace Krod
{
    public static class KUtils
    {
        public static GameObject SFXEffect(string name, string soundEventName)
        {
            GameObject obj = new(name,
                    typeof(EffectComponent),
                    typeof(VFXAttributes));
            Object.DontDestroyOnLoad(obj);
            EffectComponent ec = obj.GetComponent<EffectComponent>();
            ec.noEffectData = true;
            ec.soundName = soundEventName;
            return obj;
        }
    }
}
