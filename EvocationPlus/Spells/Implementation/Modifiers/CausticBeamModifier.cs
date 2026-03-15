using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Object = UnityEngine.Object;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class CausticBeamModifier : ISpellModifier
    {
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null)
                return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_CAUSTIC_BEAM_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_CAUSTIC_BEAM_DESC");

            // Icon donor
            CopyIconFrom(spell, library);

            // Descriptor: remove Fire, add Acid (clone component to avoid shared mutation)
            var sdcOld = spell.GetComponent<SpellDescriptorComponent>();
            if (sdcOld != null)
            {
                var sdcNew = Object.Instantiate(sdcOld);
                ReplaceComponentUtil.ReplaceComponent(spell, sdcOld, sdcNew);

                sdcNew.Descriptor &= ~SpellDescriptor.Fire;
                sdcNew.Descriptor |= SpellDescriptor.Acid;
            }

            // VFX donor
            var vfxSource = BlueprintLibrary.GetBlueprint(library, Guids.Spells.AcidVfxSpell) as BlueprintAbility;
            if (vfxSource == null)
            {
                Main.Mod.Logger.Log("causticBeam: donor spell not found for VFX (" + Guids.Spells.AcidVfxSpell + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileVisualOnly(spell, vfxSource, out var reason))
                    Main.Mod.Logger.Log("causticBeam: VFX copy failed: " + reason);
            }

            // Patch damage actions (central recursive walker)
            var runAction = spell.GetComponent<AbilityEffectRunAction>();
            if (runAction?.Actions == null)
            {
                Main.Mod.Logger.Log("causticBeam: AbilityEffectRunAction/Actions missing; skipping damage patch.");
                return; // last step
            }

            ActionListUtil.Patch(runAction.Actions, PatchAction);
        }

        private static void CopyIconFrom(BlueprintAbility target, LibraryScriptableObject library)
        {
            var id = BlueprintLibrary.NormalizeGuid(Guids.Spells.AcidVfxSpell);
            var source = BlueprintLibrary.GetBlueprint(library, id) as BlueprintAbility;
            if (source == null)
            {
                Main.Mod.Logger.Log("causticBeam: icon donor blueprint not a BlueprintAbility (id=" + id + ")");
                return;
            }

            if (source.Icon == null)
            {
                Main.Mod.Logger.Log("causticBeam: icon donor Icon is null (source=" + source.name + ")");
                return;
            }

            BlueprintUnitFactUI.SetIcon(target, source.Icon);
        }

        private static int PatchAction(GameAction action)
        {
            if (action is ContextActionDealDamage deal)
            {
                // Only convert FIRE energy damage to Acid (avoid mutating other damage riders).
                if (deal.DamageType.Type != DamageType.Energy ||
                    deal.DamageType.Energy != DamageEnergyType.Fire)
                {
                    return 0;
                }

                // Replace the DamageTypeDescription rather than mutating it in-place
                // (prevents collateral mutation if the underlying object is shared)
                var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
                dt.Type = DamageType.Energy;
                dt.Energy = DamageEnergyType.Acid;
                deal.DamageType = dt;
                return 1;
            }

            // Let ActionListUtil handle wrapper recursion (ConditionalSaved, etc.)
            return 0;
        }
    }
}