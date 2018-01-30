using System;

namespace ctac
{
    [Flags]
    public enum Statuses
    {
        None       = 0,
        Silence    = 1 << 0,
        Shield     = 1 << 1,
        Paralyze   = 1 << 2,
        Taunt      = 1 << 3,
        Cloak      = 1 << 4,
        TechResist = 1 << 5,
        Root       = 1 << 6,
        Charge     = 1 << 7,
        CantAttack = 1 << 8,
        DyadStrike = 1 << 9,
        Flying     = 1 << 10,
        Airdrop    = 1 << 11,
        Cleave     = 1 << 12,
        Piercing   = 1 << 13,


        //The rest of these are fake client side only statuses derived from other properties
        //Used mainly for consistency in showing the status icons
        hasDeathEvent = 1 << 20,
        hasEvent      = 1 << 21,
        isRanged      = 1 << 22,
        hasAura       = 1 << 23

    }
}
