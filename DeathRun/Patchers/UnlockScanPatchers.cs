/**
 * DeathRun mod - Cattlesquat "but standing on the shoulders of giants"
 * 
 * This section taken directly from Seraphim Risen's NitrogenMod
 */
namespace DeathRun.Patchers
{
    using Common;
    using HarmonyLib;
    using Items;
    using System.Collections.Generic;
    using UnityEngine;

    // This was for finding out what was going on with Exosuit and LaserCutter which were "not like the other children".
    // It turns out LaserCutter uses "LaserCutterFragment" unlike most of the others
    // And whereas setting fragments for "Exosuit" controls the number of little subfragments it "claims" you can make an exosuit out of (but there aren't nearly enough)
    // Setting "ExosuitFragment" controls the number of FULL prawn suits you need to scan (vanilla:4, of the 5 total in Aurora, and there are 3 in various wrecks outside).
    /*
    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch("Scan")]
    internal class UnlockScanResult
    {
        private static readonly Dictionary<TechType, PDAScanner.EntryData> BlueprintToFragment = new Dictionary<TechType, PDAScanner.EntryData>();
        public static bool Prefix()
        {
            TechType techType = PDAScanner.scanTarget.techType;
            GameObject gameObject = PDAScanner.scanTarget.gameObject;
            PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);

            // Populate BlueprintToFragment for reverse lookup
            foreach (KeyValuePair<TechType, PDAScanner.EntryData> entry0 in PDAScanner.mapping)
            {
                TechType blueprintTechType = entry0.Value.blueprint;

                BlueprintToFragment[blueprintTechType] = entry0.Value;
            }

            PDAScanner.Entry entry;
            if (!PDAScanner.GetPartialEntryByKey(techType, out entry))
            {
                entry = PDAScanner.Add(techType, 0);
            }

            ErrorMessage.AddMessage("Tech:" + techType + "   Object:" + gameObject + "   entryData:"+entryData+ " total"+entryData.totalFragments + "  unlocked:" + entry.unlocked + " (" + entry.techType + ")   uid:"+ PDAScanner.scanTarget.uid + "   has:"+ PDAScanner.scanTarget.hasUID);
            CattleLogger.Message("Tech:" + techType + "   Object:" + gameObject + "   entryData:" + entryData + " total" + entryData.totalFragments + "  unlocked:" + entry.unlocked + " (" + entry.techType + ")   uid:" + PDAScanner.scanTarget.uid + "   has:" + PDAScanner.scanTarget.hasUID);

            if (PDAScanner.mapping.TryGetValue(techType, out PDAScanner.EntryData entry2))
            {
                ErrorMessage.AddMessage("PDAScanner - " + entry2.totalFragments);
                CattleLogger.Message("PDAScanner - " + entry2.totalFragments);
            }

            if (BlueprintToFragment.TryGetValue(techType, out PDAScanner.EntryData entry3))
            {
                ErrorMessage.AddMessage("Blueprint - " + entry3.totalFragments);
                CattleLogger.Message("Blueprint - " + entry3.totalFragments);
            }

            return true;
        }
    }
    */


    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch("Unlock")]
    internal class UnlockScanPatchers
    {
        // Code here is adapted from Kylinator25's Alien Rifle unlock patch https://github.com/kylinator25/SubnauticaMods/blob/master/AlienRifle/PDAScannerUnlockPatch.cs
        public static bool Prefix(PDAScanner.EntryData entryData)
        {
            if (entryData.key == TechType.SpineEel) // River prowler
            {
                if (!KnownTech.Contains(ReinforcedSuitsCore.ReinforcedStillSuit))
                {
                    KnownTech.Add(ReinforcedSuitsCore.ReinforcedStillSuit, true);
                    DeathRun.saveData.playerSave.setCue("ReinforcedStillSuit", 10);
                    //ErrorMessage.AddMessage("Added blueprint for reinforced still suit to database");
                }
                if (!KnownTech.Contains(ReinforcedSuitsCore.ReinforcedSuit2ID))
                {
                    DeathRun.saveData.playerSave.setCue("ReinforcedSuit2", 5);
                    KnownTech.Add(ReinforcedSuitsCore.ReinforcedSuit2ID, true);
                    //ErrorMessage.AddMessage("Added blueprint for reinforced dive suit mark 2 to database");
                }
            }
            if (entryData.key == TechType.LavaLizard)
            {
                if (!KnownTech.Contains(ReinforcedSuitsCore.ReinforcedSuit3ID))
                {
                    DeathRun.saveData.playerSave.setCue("ReinforcedSuit3", 5);
                    KnownTech.Add(ReinforcedSuitsCore.ReinforcedSuit3ID, true);
                    //ErrorMessage.AddMessage("Added blueprint for reinforced dive suit mark 3 to database");
                }
            }
            if (entryData.key == TechType.LavaLarva && DeathRun.config.enableSpecialtyTanks)
            {
                if (!KnownTech.Contains(O2TanksCore.ChemosynthesisTankID))
                {
                    DeathRun.saveData.playerSave.setCue("ChemosynthesisTank", 5);
                    KnownTech.Add(O2TanksCore.ChemosynthesisTankID, true);
                    //ErrorMessage.AddMessage("Added blueprint for chemosynthesis oxygen tank to database");
                }
            }

            if ((entryData.key == TechType.CaveSkeleton) || (entryData.key == TechType.GhostRayBlue) || (entryData.key == TechType.GhostRayRed))
            {
                if (!KnownTech.Contains(DeathRun.decoModule.TechType))
                {
                    DeathRun.saveData.playerSave.setCue("DecoModule", 5);
                    //PDAEncyclopedia.Add("DecoModule", true);
                    KnownTech.Add(DeathRun.decoModule.TechType, true);
                }
            }
            return true;
        }
    }
}
