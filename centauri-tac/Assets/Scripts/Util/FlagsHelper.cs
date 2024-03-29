﻿using System;

namespace ctac
{
    // The casts to object in the below code are an unfortunate necessity due to
    // C#'s restriction against a where T : Enum constraint. (There are ways around
    // this, but they're outside the scope of this simple illustration.)
    // TODO: stop using IsSet becuase of these memory alloc casts
    public static class FlagsHelper
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }

        public static T TryParse<T>(string valueToParse, T defaultValue)
        {
            var returnValue = defaultValue;
            if (Enum.IsDefined(typeof(T), valueToParse))
            {
                returnValue = (T)Enum.Parse(typeof(T), valueToParse);
            }
            return returnValue;
        }
    }
}
