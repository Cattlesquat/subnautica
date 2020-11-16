namespace NitrogenMod.Patchers
{
    using HarmonyLib;
    using Items;

    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch("Unlock")]
    internal class UnlockScanPatchers
    {
        // Code here is adapted from Kylinator25's Alien Rifle unlock patch https://github.com/kylinator25/SubnauticaMods/blob/master/AlienRifle/PDAScannerUnlockPatch.cs
        public static bool Prefix(PDAScanner.EntryData entryData)
        {
            if (entryData.key == TechType.SpineEel)
            {
                if (!KnownTech.Contains(ReinforcedSuitsCore.ReinforcedStillSuit))
                {
                    KnownTech.Add(ReinforcedSuitsCore.ReinforcedStillSuit);
                    ErrorMessage.AddMessage("Added blueprint for reinforced still suit to database");
                }
                if (!KnownTech.Contains(ReinforcedSuitsCore.ReinforcedSuit2ID))
                {
                    KnownTech.Add(ReinforcedSuitsCore.ReinforcedSuit2ID);
                    ErrorMessage.AddMessage("Added blueprint for reinforced dive suit mark 2 to database");
                }
            }
            if (entryData.key == TechType.LavaLizard)
            {
                if (!KnownTech.Contains(ReinforcedSuitsCore.ReinforcedSuit3ID))
                {
                    KnownTech.Add(ReinforcedSuitsCore.ReinforcedSuit3ID);
                    ErrorMessage.AddMessage("Added blueprint for reinforced dive suit mark 3 to database");
                }
            }
            if (entryData.key == TechType.LavaLarva && Main.specialtyTanks)
            {
                if (!KnownTech.Contains(O2TanksCore.ChemosynthesisTankID))
                {
                    KnownTech.Add(O2TanksCore.ChemosynthesisTankID);
                    ErrorMessage.AddMessage("Added blueprint for chemosynthesis oxygen tank to database");
                }
            }
            return true;
        }
    }
}
