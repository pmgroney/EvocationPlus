using System;
using System.Reflection;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;

namespace EvocationPlus.Patches.Spellbooks
{
    internal static class ArchetypeSpellbookUtil
    {
        internal static void SetReplaceSpellbook(BlueprintArchetype archetype, BlueprintSpellbook spellbook)
        {
            if (archetype == null) throw new ArgumentNullException(nameof(archetype));
            if (spellbook == null) throw new ArgumentNullException(nameof(spellbook));

            var t = typeof(BlueprintArchetype);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // 1) Prefer property if it exists
            var prop = t.GetProperty("ReplaceSpellbook", flags);
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(BlueprintSpellbook))
            {
                prop.SetValue(archetype, spellbook, null);
                Main.Mod.Logger.Log("EVP: Set archetype ReplaceSpellbook via property.");
                return;
            }

            // 2) Try common field names across Owlcat versions
            var field =
                t.GetField("ReplaceSpellbook", flags) ??
                t.GetField("m_ReplaceSpellbook", flags) ??
                t.GetField("m_ReplaceSpellbookBlueprint", flags);

            if (field == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED to set ReplaceSpellbook. No known field/property found.");
                return;
            }

            if (field.FieldType != typeof(BlueprintSpellbook))
            {
                Main.Mod.Logger.Log("EVP: FAILED to set ReplaceSpellbook. Field type mismatch: " + field.FieldType);
                return;
            }

            field.SetValue(archetype, spellbook);
        }
    }
}