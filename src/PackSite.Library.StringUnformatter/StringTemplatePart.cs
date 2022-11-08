namespace PackSite.Library.StringUnformatter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// String template part
    /// </summary>
    public sealed class StringTemplatePart : IEquatable<StringTemplatePart>
    {
        /// <summary>
        /// String template part raw value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Whether value is parameter.
        /// </summary>
        public bool IsParameter { get; }

        /// <summary>
        /// Parameter value.
        /// </summary>
        [MemberNotNullWhen(true, nameof(IsParameter))]
        public string? Parameter { get; }

        /// <summary>
        /// Value format.
        /// </summary>
        public string? Format { get; }

        /// <summary>
        /// Initializes an instance of <see cref="StringTemplatePart"/>.
        /// </summary>
        public StringTemplatePart(string? value, bool isParameter = false)
        {
            Value = value ?? string.Empty;
            Parameter = isParameter ? Value : null;
            IsParameter = isParameter;

            if (isParameter && !string.IsNullOrEmpty(value))
            {
                int formatDelimiterPosition = Value.LastIndexOf(':');

                if (formatDelimiterPosition == 0)
                {
                    throw new FormatException($"Message format must not be the only part of the '{nameof(StringTemplatePart)}'");
                }
                else if (formatDelimiterPosition == Value.Length - 1)
                {
                    throw new FormatException($"Message format must not be the last character of the '{nameof(StringTemplatePart)}'");
                }
                else if (formatDelimiterPosition > 0)
                {
                    Parameter = Value[..formatDelimiterPosition];
                    Format = Value[(formatDelimiterPosition + 1)..];
                }
            }
        }

        /// <summary>
        /// Joins parts to string.
        /// </summary>
        /// <param name="parts"></param>
        public static string Join(params StringTemplatePart[] parts)
        {
            return Join((IEnumerable<StringTemplatePart>)parts);
        }

        /// <summary>
        /// Joins parts to string.
        /// </summary>
        /// <param name="parts"></param>
        public static string Join(IEnumerable<StringTemplatePart> parts)
        {
            StringBuilder stringBuilder = new();

            foreach (StringTemplatePart part in parts)
            {
                if (part.IsParameter)
                {
                    stringBuilder.Append('{');
                    stringBuilder.Append(part.Value);
                    stringBuilder.Append('}');
                }
                else
                {
                    stringBuilder.Append(part.Value);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Joins parts to string.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="parametersCount"></param>
        public static string Join(IEnumerable<StringTemplatePart> parts, out int parametersCount)
        {
            parametersCount = 0;

            StringBuilder stringBuilder = new();

            foreach (StringTemplatePart part in parts)
            {
                if (part.IsParameter)
                {
                    stringBuilder.Append('{');
                    stringBuilder.Append(part.Value);
                    stringBuilder.Append('}');

                    ++parametersCount;
                }
                else
                {
                    stringBuilder.Append(part.Value);
                }
            }

            return stringBuilder.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is StringTemplatePart part && Equals(part);
        }

        /// <inheritdoc/>
        public bool Equals(StringTemplatePart? other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return other is not null &&
                   IsParameter == other.IsParameter &&
                   Value == other.Value;
        }

        /// <summary>
        /// Determines whether two specified <see cref="StringTemplatePart"/> have the same value.
        /// </summary>
        public static bool operator ==(in StringTemplatePart left, in StringTemplatePart right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified <see cref="StringTemplatePart"/> have different values.
        /// </summary>
        public static bool operator !=(in StringTemplatePart left, in StringTemplatePart right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, IsParameter);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string? ToString()
        {
            return IsParameter ? $"PARAM: \"{{{Value}}}\"" : $"VALUE: \"{Value}\"";
        }
    }
}
