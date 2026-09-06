using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class NameModifier : Modifier<string>
    {
        public override string Describe() => $"named \"{Value}\"";
    }
}
