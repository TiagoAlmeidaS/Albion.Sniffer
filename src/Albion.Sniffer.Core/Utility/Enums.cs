namespace Albion.Sniffer.Core.Utility
{
    public enum Faction : byte
    {
        NoPVP = 0,
        Martlock = 1,
        Lymhurst = 2,
        Bridjewatch = 3,
        ForthSterling = 4,
        Thetford = 5,
        Caerleon = 6,
        PVP = 255
    }

    public enum Flags : byte
    {
        Speed = 1,
        NewPosition = 2,
        PlayerId = 4,
        N8 = 8,
        N16 = 16,
        N32 = 32,
    }
}
