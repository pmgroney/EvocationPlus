using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine.Serialization;

namespace EvocationPlus.Patches
{
    public class AreaEffectScaleFxOnSpawn : AreaEffectSpawnLogic
    {
        [FormerlySerializedAs("Scale")] public float scale = 1f;

        protected override void OnAreaEffectSpawn(MechanicsContext context, AreaEffectEntityData areaEffect)
        {
            if (areaEffect == null) return;

            // We need the correct way to grab the spawned FX/view for an AreaEffectEntityData in Kingmaker.
            // Paste AreaEffectEntityData view fields next and I'll swap this line to the exact API.
            var view = areaEffect.View; // <-- likely, but depends on your AreaEffectEntityData class
            if (view == null) return;

            view.transform.localScale *= scale;
        }
    }
}