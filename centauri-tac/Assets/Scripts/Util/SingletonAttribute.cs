using System;

namespace ctac
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false) ]
    class SingletonAttribute : Attribute { }
}
