#nullable enable
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums
{
    [Serializable]
    internal sealed class EnumValue<TValue>
    {
        [Tooltip("The name of the enum member this entry is keyed by.")]
        [SerializeField] private string _key = string.Empty;

        [Tooltip("The value returned for this entry's key.")]
        [SerializeField] private TValue? _value;

#if UNITY_EDITOR
        [Tooltip("The enum the key belongs to; mirrored from the parent so the drawer can pick the right field.")]
        [SerializeField] private string? _enumType;
#endif

        public Enum? Key { get; private set; }

        public long NumericKey { get; private set; }

        public TValue? Value => _value;

        public bool IsResolved => Key is not null;

        public void Initialize(Type type)
        {
#if !ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED
            using (this.Marker())
#endif
            {
                if (Enum.TryParse(type, _key, out var parsedEnum))
                {
                    Key = (Enum)parsedEnum;
                    NumericKey = EnumInfo.ToInt64(Key);
                }
                else
                {
                    Reset();

                    Debug.LogError($"[{nameof(EnumValue<TValue>)}] [{nameof(Initialize)}] " +
                        $"Couldn't parse key '{_key}' to Enum '{type.FullName}'");
                }
            }
        }

        public void Reset()
        {
            Key = null;
            NumericKey = 0L;
        }
    }
}
