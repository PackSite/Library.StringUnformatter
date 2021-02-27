﻿namespace PackSite.Library.StringUnformatter
{
    using System;

    /// <summary>
    /// String template part
    /// </summary>
    public readonly struct StringTemplatePart : IEquatable<StringTemplatePart>
    {
        private readonly string? _value;

        /// <summary>
        /// String template part value
        /// </summary>
        public string Value => _value ?? string.Empty;

        /// <summary>
        /// Whether value is parameter
        /// </summary>
        public bool IsParameter { get; }

        /// <summary>
        /// Initializes an instance of <see cref="StringTemplatePart"/>.
        /// </summary>
        public StringTemplatePart(string value, bool isParameter = false)
        {
            _value = value;
            IsParameter = isParameter;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is StringTemplatePart part && Equals(part);
        }

        /// <inheritdoc/>
        public bool Equals(StringTemplatePart other)
        {
            return Value == other.Value &&
                   IsParameter == other.IsParameter;
        }

        /// <summary>
        /// Determines whether two specified string template parts have the same value.
        /// </summary>
        public static bool operator ==(in StringTemplatePart left, in StringTemplatePart right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified string template parts have different values.
        /// </summary>
        public static bool operator !=(in StringTemplatePart left, in StringTemplatePart right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Value, IsParameter);
        }

        /// <inheritdoc/>
        public override readonly string? ToString()
        {
            return IsParameter ? $"PARAM: \"{{{Value}}}\"" : $"VALUE: \"{Value}\"";
        }
    }
}
