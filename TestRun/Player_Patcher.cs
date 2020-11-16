using HarmonyLib;
using UnityEngine;

namespace DeathRun  // This is usually the name of your mod.
{
    [HarmonyPatch(typeof(Player))]  // We're patching the Player class.
    [HarmonyPatch("Update")]        // The Player class's Update method specifically.
    public class Player_Update_Patch : MonoBehaviour
    {
        [HarmonyPostfix]      // Run this after the default game's Player Update method runs.
        public static void Postfix()
        {            
            // Keypresses put in any classes Update method that are called often will be listened for.            
            if (Input.GetKeyDown(KeyCode.F2))
            {
                IngameMenu.main.SaveGame();      // Runs the savegame function identically to the main menu
                IngameMenu.main.QuitSubscreen(); // Previous call can cause a 'ghost menu' to be brought up and invisible. This closes it.
            }
        }
    }
}
