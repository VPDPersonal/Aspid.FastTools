using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [Flags]
    internal enum AspidSettingsScope
    {
        Shared = 1 << 0,

        User = 1 << 1,

        All = Shared | User,
    }
}
