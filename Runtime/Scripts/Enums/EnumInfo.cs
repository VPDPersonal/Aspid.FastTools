#nullable enable
using System;
using Unity.Collections.LowLevel.Unsafe;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums
{
    internal static class EnumInfo
    {
        public static bool IsFlags(Type type) =>
            type.IsDefined(typeof(FlagsAttribute), inherit: false);

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

    internal static class EnumInfo<TEnum>
        where TEnum : struct, Enum
    {
        public static readonly bool IsFlags = EnumInfo.IsFlags(typeof(TEnum));
        private static readonly TypeCode _underlyingTypeCode = Type.GetTypeCode(typeof(TEnum));

        public static long ToInt64(TEnum value) => _underlyingTypeCode switch
        {
            TypeCode.SByte => UnsafeUtility.As<TEnum, sbyte>(ref value),
            TypeCode.Byte => UnsafeUtility.As<TEnum, byte>(ref value),
            TypeCode.Int16 => UnsafeUtility.As<TEnum, short>(ref value),
            TypeCode.UInt16 => UnsafeUtility.As<TEnum, ushort>(ref value),
            TypeCode.Int32 => UnsafeUtility.As<TEnum, int>(ref value),
            TypeCode.UInt32 => UnsafeUtility.As<TEnum, uint>(ref value),
            TypeCode.Int64 => UnsafeUtility.As<TEnum, long>(ref value),
            TypeCode.UInt64 => (long)UnsafeUtility.As<TEnum, ulong>(ref value),
            _ => throw new InvalidOperationException($"Unsupported enum underlying type '{Enum.GetUnderlyingType(typeof(TEnum))}'."),
        };
    }
}
