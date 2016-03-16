using System;

namespace ctac
{
    [Flags]
    public enum Statuses
    {
      Silence = 1,
      Shield = 2,
      Paralyze = 4,
      Taunt = 8,
      Cloak = 16,
      TechResist = 32,
      Rooted = 64
    }
}
