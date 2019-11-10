namespace EnzymeChargedBattery.Patchers
{
    using Harmony;
    using Items;

    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch("Unlock")]
    internal class UnlockScanPatchers
    {
        // Code here is adapted from Kylinator25's Alien Rifle unlock patch https://github.com/kylinator25/SubnauticaMods/blob/master/AlienRifle/PDAScannerUnlockPatch.cs
        public static bool Prefix(PDAScanner.EntryData entryData)
        {
            if (entryData.key == TechType.Warper)
            {
                if (!KnownTech.Contains(KharaaBattery.BattID))
                {
                    KnownTech.Add(KharaaBattery.BattID);
                    KnownTech.Add(KharaaPowerCell.PowCelID);
                    ErrorMessage.AddMessage("Added blueprint for biochemical batteries to database");
                }
            }
            return true;
        }
    }
}
