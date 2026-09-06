#nullable enable
using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums
{
    internal static class EnumInfo
    {
        public static bool IsFlags(Type type) =>
            type.IsDefined(typeof(FlagsAttribute), false);

        public static long ToInt64(Enum value) => Type.GetTypeCode(value.GetType()) switch
        {
            TypeCode.SByte => (sbyte)(object)value,
            TypeCode.Byte => (byte)(object)value,
            TypeCode.Int16 => (short)(object)value,
            TypeCode.UInt16 => (ushort)(object)value,
            TypeCode.Int32 => (int)(object)value,
            TypeCode.UInt32 => (uint)(object)value,
            TypeCode.Int64 => (long)(object)value,
            TypeCode.UInt64 => unchecked((long)(ulong)(object)value),
            _ => throw new InvalidOperationException($"Unsupported enum underlying type '{Enum.GetUnderlyingType(value.GetType())}'."),
        };
    }
}
