using System;
using UnityEngine;
using Aspid.FastTools.Types;

// Docs-media harness: open-generic picking demo for the TypeSelectorWindow GIF in Types.md —
// picking Amplify<T> walks through its argument page before returning the constructed type.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    [Serializable]
    public abstract class EffectModifier { }

    [Serializable]
    public sealed class Haste : EffectModifier { }

    [Serializable]
    public sealed class Amplify<T> : EffectModifier
        where T : StatusEffect { }

    [Serializable]
    public abstract class StatusEffect { }

    [Serializable]
    public sealed class Burning : StatusEffect { }

    [Serializable]
    public sealed class Frozen : StatusEffect { }

    [Serializable]
    public sealed class Slowed : StatusEffect { }

    public sealed class EffectRack : MonoBehaviour
    {
        [TypeSelector]
        [SerializeReference] private EffectModifier _modifier;
    }
}
