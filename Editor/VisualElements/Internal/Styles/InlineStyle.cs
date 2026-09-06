using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Tracks the effective value of a USS-controlled property and remembers whether it was set inline from code.
    // Inline values take precedence over USS values, mirroring the inline-vs-stylesheet rule in UIToolkit.
    internal class InlineStyle<T>
    {
        private readonly Action<T, T> _onSet;

        public T Value { get; private set; }

        public bool IsInline { get; private set; }

        public InlineStyle(T value, Action<T, T> onSet = null)
        {
            Value = value;
            _onSet = onSet;
            IsInline = false;

            onSet?.Invoke(default, Value);
        }

        public void SetInlineValue(T value)
        {
            _onSet?.Invoke(Value, value);

            Value = value;
            IsInline = true;
        }

        public void SetDefaultValue(T value)
        {
            if (IsInline)  return;
            if (EqualityComparer<T>.Default.Equals(Value, value)) return;

            _onSet?.Invoke(Value, value);
            Value = value;
        }

        public static implicit operator T(InlineStyle<T> inlineStyle) => inlineStyle.Value;
    }
}
