using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Aspid.FastTools.Generators.IdStruct.Data;

internal readonly struct DiagnosticInfo : IEquatable<DiagnosticInfo>
{
    public readonly string DescriptorId;
    public readonly string MessageArg0;
    public readonly string? MessageArg1;
    public readonly string? FilePath;
    public readonly TextSpan TextSpan;
    public readonly LinePositionSpan LineSpan;

    public DiagnosticInfo(DiagnosticDescriptor descriptor, Location? location, string messageArg0, string? messageArg1 = null)
    {
        DescriptorId = descriptor.Id;
        MessageArg0 = messageArg0;
        MessageArg1 = messageArg1;

        if (location is { SourceTree: not null })
        {
            FilePath = location.SourceTree.FilePath;
            TextSpan = location.SourceSpan;
            LineSpan = location.GetLineSpan().Span;
        }
        else
        {
            FilePath = null;
            TextSpan = default;
            LineSpan = default;
        }
    }

    public Diagnostic ToDiagnostic()
    {
        var descriptor = IdStructDiagnostics.GetDescriptor(DescriptorId);
        if (descriptor is null) return Diagnostic.Create("UNKNOWN", "Generator", "Unknown descriptor", DiagnosticSeverity.Hidden, DiagnosticSeverity.Hidden, false, 4);

        var location = FilePath is null
            ? Location.None
            : Location.Create(FilePath, TextSpan, LineSpan);

        return MessageArg1 is null
            ? Diagnostic.Create(descriptor, location, MessageArg0)
            : Diagnostic.Create(descriptor, location, MessageArg0, MessageArg1);
    }

    public bool Equals(DiagnosticInfo other) =>
        DescriptorId == other.DescriptorId
        && MessageArg0 == other.MessageArg0
        && MessageArg1 == other.MessageArg1
        && FilePath == other.FilePath
        && TextSpan.Equals(other.TextSpan)
        && LineSpan.Equals(other.LineSpan);

    public override bool Equals(object? obj) => obj is DiagnosticInfo other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = DescriptorId.GetHashCode();
            hash = (hash * 397) ^ MessageArg0.GetHashCode();
            hash = (hash * 397) ^ (MessageArg1?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (FilePath?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ TextSpan.GetHashCode();
            hash = (hash * 397) ^ LineSpan.GetHashCode();
            return hash;
        }
    }
}
