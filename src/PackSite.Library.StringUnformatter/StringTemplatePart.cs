namespace PackSite.Library.StringUnformatter
{
    using System;

    /// <summary>
    /// String template part
    /// </summary>
    public readonly struct StringTemplatePart : IEquatable<StringTemplatePart>
    {
        /// <summary>
        /// String template part value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Whether value is parameter
        /// </summary>
        public bool IsParameter { get; }

        /// <summary>
        /// Initializes an instance of <see cref="StringTemplatePart"/>.
        /// </summary>
        public StringTemplatePart(string value, bool isParameter = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace", nameof(value));
            }

            Value = value;
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
        public static bool operator ==(StringTemplatePart left, StringTemplatePart right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified string template parts have different values.
        /// </summary>
        public static bool operator !=(StringTemplatePart left, StringTemplatePart right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, IsParameter);
        }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return IsParameter ? $"PARAM: \"{{{Value}}}\"" : $"VALUE: \"{Value}\"";
        }
    }
}
