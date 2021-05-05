/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * Nonviolent challenge - makes knife do no damage
 */
namespace DeathRun.Patchers
{
    using HarmonyLib;
    using Items;
    using Common;
    using UnityEngine;

    [HarmonyPatch(typeof(Knife))]
    [HarmonyPatch("OnToolUseAnim")]
    internal class KnifeUsePatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Knife __instance, GUIHand hand)
        {
			if (!Config.NONVIOLENT.Equals(DeathRun.config.nonViolent))
            {
				return true;
			}           

			Vector3 position = default(Vector3);
			GameObject gameObject = null;
			UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref gameObject, ref position, true);
			if (gameObject == null)
			{
				InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
				if (component != null && component.GetMostRecent() != null)
				{
					gameObject = component.GetMostRecent().gameObject;
				}
			}
			if (gameObject)
			{
				LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
				if (Knife.IsValidTarget(liveMixin))
				{
					if (liveMixin)
					{
						bool wasAlive = liveMixin.IsAlive();
						TechType t = CraftData.GetTechType(gameObject);
						if ((t == TechType.GenericJeweledDisk) ||
							(t == TechType.BlueJeweledDisk) ||
							(t == TechType.GreenJeweledDisk) ||
							(t == TechType.PurpleJeweledDisk) ||
							(t == TechType.RedJeweledDisk))
						{
							liveMixin.TakeDamage(__instance.damage, position, __instance.damageType, null);
						}
						__instance.GiveResourceOnDamage(gameObject, liveMixin.IsAlive(), wasAlive);
					}
					global::Utils.PlayFMODAsset(__instance.attackSound, __instance.transform, 20f);
					//VFXSurface component2 = gameObject.GetComponent<VFXSurface>();
					//Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
					//VFXSurfaceTypeManager.main.Play(component2, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);
				}
				else
				{
					gameObject = null;
				}
			}
			if (gameObject == null && hand.GetActiveTarget() == null)
			{
				if (Player.main.IsUnderwater())
				{
					global::Utils.PlayFMODAsset(__instance.underwaterMissSound, __instance.transform, 20f);
					return false;
				}
				global::Utils.PlayFMODAsset(__instance.surfaceMissSound, __instance.transform, 20f);
			}
			return false;
		}
	}
}