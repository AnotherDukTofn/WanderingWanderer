namespace Game.Logic.Shared
{
    #region Stat Modify
    
    public enum StatType
    {
        POT = 0, SPI = 1, WIS = 2, VIT = 3, AGI = 4,
        FirePot = 5, WaterPot = 6, IcePot = 7, LightningPot = 8,
        FireRes = 9, WaterRes = 10, IceRes = 11, LightningRes = 12,
        MaxHp = 13, MaxMp = 14, MpRecovery = 15,
        AllPot = 16, AllRes = 17
    }
    
    public enum ModType
    {
        Flat = 0,
        Percent = 1
    }

    #endregion
}