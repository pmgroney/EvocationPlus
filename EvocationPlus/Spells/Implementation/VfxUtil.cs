using System.Reflection;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using UnityEngine;

namespace EvocationPlus.Spells.Implementation
{
    /// <summary>
    ///     Projectile VFX helpers for Pathfinder: Kingmaker.
    ///     Key constraint (KM): ProjectileLink is just a string AssetId; you cannot point it at
    ///     arbitrary runtime GameObjects. Therefore, custom projectile looks should be done by
    ///     reusing existing projectile AssetIds (donor spells) while preserving projectile count.
    /// </summary>
    public static class VfxUtil
    {
        private static readonly FieldInfo DeliverProjectilesField =
            AccessTools.Field(typeof(AbilityDeliverProjectile), "m_Projectiles");

        /// <summary>
        ///     Copies the donor spell's first projectile View.AssetId onto the target spell's projectiles,
        ///     while preserving the target's projectile count and per-slot projectile objects.
        ///     This is the safest way to get a different look in Kingmaker.
        /// </summary>
        public static bool TryCopyProjectileViewAssetIdPreserveCount(
            BlueprintAbility target,
            BlueprintAbility donor,
            out string reason,
            bool instantCast = false)
        {
            reason = null;

            if (target == null)
            {
                reason = "target null";
                return false;
            }

            if (donor == null)
            {
                reason = "donor null";
                return false;
            }

            var tDeliver = target.GetComponent<AbilityDeliverProjectile>();
            if (tDeliver == null)
            {
                reason = "target missing AbilityDeliverProjectile";
                return false;
            }

            var dDeliver = donor.GetComponent<AbilityDeliverProjectile>();
            if (dDeliver == null)
            {
                reason = "donor missing AbilityDeliverProjectile";
                return false;
            }

            var donorProj = dDeliver.Projectiles != null && dDeliver.Projectiles.Length > 0
                ? dDeliver.Projectiles[0]
                : null;

            if (donorProj == null)
            {
                reason = "donor has no usable projectile[0]";
                return false;
            }

            var donorAssetId = GetProjectileViewAssetId(donorProj);
            if (string.IsNullOrEmpty(donorAssetId))
            {
                reason = "donor projectile View.AssetId missing/empty";
                return false;
            }

            if (tDeliver.Projectiles == null || tDeliver.Projectiles.Length == 0)
            {
                reason = "target has no Projectiles[]";
                return false;
            }

            // IMPORTANT: clone target projectiles so we don't mutate shared assets used by other spells
            var clonedArr = new BlueprintProjectile[tDeliver.Projectiles.Length];
            var changed = 0;
            
            for (var i = 0; i < tDeliver.Projectiles.Length; i++)
            {
                var p = tDeliver.Projectiles[i];
                if (p == null)
                {
                    clonedArr[i] = null;
                    continue;
                }

                var pClone = Object.Instantiate(p);
                pClone.name = $"{p.name}_EVP_VfxClone";
                clonedArr[i] = pClone;
                // Optional instant projectile behavior
                if (instantCast)
                {
                    pClone.Speed = 1000f;   
                    pClone.MinTime = 0f;
                }
                else
                {
                    // Preserve donor timing for visual authenticity
                    pClone.Speed = donorProj.Speed;
                    pClone.MinTime = donorProj.MinTime;
                }
                if (SetProjectileViewAssetId(pClone, donorAssetId))
                    changed++;
            }

            // Assign cloned projectiles back onto the ability (setter or backing field)
            try
            {
                tDeliver.Projectiles = clonedArr;
            }
            catch
            {
                if (DeliverProjectilesField == null)
                {
                    reason = "Could not set Projectiles[]: no setter and backing field m_Projectiles not found.";
                    return false;
                }

                DeliverProjectilesField.SetValue(tDeliver, clonedArr);
            }

            if (changed == 0)
            {
                reason = "failed to set View.AssetId on any projectile clones";
                return false;
            }

            return true;
        }

        public static bool TryMakeProjectilesInvisiblePreserveCount(BlueprintAbility target, out string reason)
        {
            reason = null;
            if (target == null)
            {
                reason = "target null";
                return false;
            }

            var tDeliver = target.GetComponent<AbilityDeliverProjectile>();
            if (tDeliver == null)
            {
                reason = "target missing AbilityDeliverProjectile";
                return false;
            }

            if (tDeliver.Projectiles == null || tDeliver.Projectiles.Length == 0)
            {
                reason = "target has no Projectiles[]";
                return false;
            }

            var clonedArr = new BlueprintProjectile[tDeliver.Projectiles.Length];
            var changed = 0;

            for (var i = 0; i < tDeliver.Projectiles.Length; i++)
            {
                var p = tDeliver.Projectiles[i];
                if (p == null)
                {
                    clonedArr[i] = null;
                    continue;
                }

                var pClone = Object.Instantiate(p);
                pClone.name = $"{p.name}_EVP_InvisibleClone";

                // Do NOT clear View/CastFx — projectile must still exist to register hit

                changed += ClearNestedWeakLinkAssetId(pClone, "ProjectileHit", "HitFx") ? 1 : 0;
                changed += ClearNestedWeakLinkAssetId(pClone, "ProjectileHit", "HitSnapFx") ? 1 : 0;
                changed += ClearNestedWeakLinkAssetId(pClone, "ProjectileHit", "MissFx") ? 1 : 0;
                changed += ClearNestedWeakLinkAssetId(pClone, "ProjectileHit", "MissDecalFx") ? 1 : 0;

                // Make it very fast to feel instant
                pClone.Speed = 5000f;
                pClone.MinTime = 0f;


                clonedArr[i] = pClone;
            }

            try
            {
                tDeliver.Projectiles = clonedArr;
            }
            catch
            {
                var f = AccessTools.Field(tDeliver.GetType(), "m_Projectiles");
                if (f == null)
                {
                    reason = "Could not set Projectiles[]: no setter and backing field m_Projectiles not found.";
                    return false;
                }

                f.SetValue(tDeliver, clonedArr);
            }

            if (changed == 0)
            {
                reason = "no projectile visual links were cleared (structure mismatch?)";
                return false;
            }

            return true;
        }

        public static bool TryCopyProjectileVisualOnly(BlueprintAbility target, BlueprintAbility source,
            out string reason, bool instantCast = false)
        {
            return TryCopyProjectileViewAssetIdPreserveCount(target, source, out reason, instantCast);
        }

        private static bool ClearNestedWeakLinkAssetId(object owner, string parentField, string childField)
        {
            var pf = AccessTools.Field(owner.GetType(), parentField);
            if (pf == null) return false;

            var parent = pf.GetValue(owner);
            if (parent == null) return false;

            var cf = AccessTools.Field(parent.GetType(), childField);
            if (cf == null) return false;

            var link = cf.GetValue(parent);
            if (link == null) return false;

            var assetIdField = AccessTools.Field(link.GetType(), "AssetId");
            if (assetIdField == null) return false;

            assetIdField.SetValue(link, "");
            return true;
        }

        /// <summary>
        ///     Reads BlueprintProjectile.View.AssetId (Kingmaker.ResourceLinks.ProjectileLink).
        /// </summary>
        public static string GetProjectileViewAssetId(BlueprintProjectile projectile)
        {
            if (projectile == null) return null;

            var viewField = AccessTools.Field(projectile.GetType(), "View");
            if (viewField == null) return null;

            var viewLink = viewField.GetValue(projectile);
            if (viewLink == null) return null;

            var assetIdField = AccessTools.Field(viewLink.GetType(), "AssetId");
            return assetIdField?.GetValue(viewLink) as string;
        }

        /// <summary>
        ///     Sets BlueprintProjectile.View.AssetId. Returns false if structure differs or is missing.
        /// </summary>
        public static bool SetProjectileViewAssetId(BlueprintProjectile projectile, string assetId)
        {
            if (projectile == null) return false;
            if (string.IsNullOrEmpty(assetId)) return false;

            var viewField = AccessTools.Field(projectile.GetType(), "View");
            if (viewField == null) return false;

            var viewLink = viewField.GetValue(projectile);
            if (viewLink == null) return false;

            var assetIdField = AccessTools.Field(viewLink.GetType(), "AssetId");
            if (assetIdField == null) return false;

            assetIdField.SetValue(viewLink, assetId);
            return true;
        }
    }
}