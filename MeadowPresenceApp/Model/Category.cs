using System;

namespace MeadowPresenceApp.Model
{
    [Flags]
    public enum Category
    {
        Information = 1,
        Error = 2,
        Flow = 4,
        Debug = 8
    }
}
