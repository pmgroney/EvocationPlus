using Kingmaker.Blueprints.Facts;
using UnityEngine;

namespace EvocationPlus.BlueprintUtils
{
    public static class BlueprintUnitFactUI
    {
        // Compile once; no per-call FieldInfo.SetValue anymore
        private static readonly FastSetter SetIconField =
            Helpers.CreateFieldSetter<BlueprintUnitFact>("m_Icon");

        public static void SetIcon(BlueprintUnitFact fact, Sprite icon)
        {
            if (fact == null) return;
            SetIconField(fact, icon);
        }
    }
}