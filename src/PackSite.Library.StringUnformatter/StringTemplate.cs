﻿namespace PackSite.Library.StringUnformatter
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// String template object.
    /// </summary>
    public sealed class StringTemplate : IEquatable<StringTemplate>
    {
        /// <summary>
        /// Template definition.
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// Template parts.
        /// </summary>
        public IReadOnlyList<StringTemplatePart> Parts { get; }

        /// <summary>
        /// Whether template has any parameters.
        /// </summary>
        public bool HasParameters => ParmetersCount > 0;

        /// <summary>
        /// Number of parameters in the template.
        /// </summary>
        public int ParmetersCount { get; }

        /// <summary>
        /// Initializes an instance of <see cref="StringTemplate"/>.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="parts"></param>
        /// <param name="parmetersCount"></param>
        private StringTemplate(string template, IReadOnlyList<StringTemplatePart> parts, int parmetersCount)
        {
            Template = template;
            Parts = parts;
            ParmetersCount = parmetersCount;
        }

        /// <summary>
        /// Parse template from string.
        /// </summary>
        public static StringTemplate FromParts(IEnumerable<StringTemplatePart> parts)
        {
            ArgumentNullException.ThrowIfNull(parts);

            string template = StringTemplatePart.Join(parts, out int parametersCount);

            return new StringTemplate(template, [.. parts], parametersCount);
        }

        /// <summary>
        /// Parse template from string.
        /// </summary>
        /// <exception cref="ArgumentException">Throws when template is null or whitespace.</exception>
        /// <exception cref="FormatException">Throws when template format is invalid.</exception>
        public static StringTemplate Parse(string template)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(template);

            List<StringTemplatePart> parts = [];
            int parametersCount = 0;

            ReadOnlySpan<char> span = template.AsSpan();

            bool isOpened = false;
            int sliceStart = 0;

            for (int i = 0; i < span.Length; i++)
            {
                char ch = span[i];
                if (!isOpened && ch == '{')
                {
                    int openingBracketCount = CountUntillChange(span, i, '{');
                    bool isEscaped = openingBracketCount % 2 == 0;
                    i += openingBracketCount - 1;

                    if (!isEscaped)
                    {
                        if (i - sliceStart > 0)
                        {
                            string tmp = new(span[sliceStart..i]);
                            tmp = tmp.Replace("{{", "{").Replace("}}", "}");

                            parts.Add(new StringTemplatePart(tmp, false));
                        }

                        isOpened = true;
                        sliceStart = i + 1;
                    }
                }
                else if (!isOpened && ch == '}')
                {
                    int closingBracketCount = CountUntillChange(span, i, '}');
                    bool isEscaped = closingBracketCount % 2 == 0;
                    i += closingBracketCount - 1;

                    if (!isEscaped)
                    {
                        throw new FormatException($"String template '{template}' is invalid. Parameter closed but was never opened.");
                    }
                }
                else if (isOpened && ch == '}')
                {
                    if (parts.Count > 0 && parts[^1].IsParameter)
                    {
                        throw new FormatException($"String template '{template}' is invalid. Template cannot contain two or more subsequent parameters.");
                    }

                    string tmp = new(span[sliceStart..i]);
                    tmp = tmp.Replace("{{", "{").Replace("}}", "}");

                    if (string.IsNullOrWhiteSpace(tmp))
                    {
                        throw new FormatException($"String template '{template}' is invalid. Template cannot contain empty parameters.");
                    }

                    parts.Add(new StringTemplatePart(tmp, true));
                    ++parametersCount;

                    isOpened = false;
                    sliceStart = i + 1;
                }
                else if (ch is '{' or '}')
                {
                    throw new FormatException($"String template '{template}' is invalid. Parameter {(isOpened ? "opened" : "closed")} but was never {(!isOpened ? "opened" : "closed")}.");
                }
            }

            if (span.Length != sliceStart)
            {
                string tmp = new(span[sliceStart..]);
                tmp = tmp.Replace("{{", "{").Replace("}}", "}");

                parts.Add(new StringTemplatePart(tmp, false));
            }

            return new StringTemplate(template, parts, parametersCount);
        }

        private static int CountUntillChange(ReadOnlySpan<char> span, int startIndex, char ch)
        {
            int i = startIndex;

            while (i < span.Length && span[i] == ch)
            {
                ++i;
            }

            return i - startIndex;
        }

        /// <summary>
        /// Checks whether formatted string matches the template.
        /// </summary>
        /// <param name="formatted"></param>
        /// <returns></returns>
        public bool Matches(string formatted)
        {
            return Unformat(formatted) is not null;
        }

        /// <summary>
        /// Returns unformatted parameters, null when failed to unformat, or empty collection when successfully unformatted but no parameters were present in template.
        /// </summary>
        public FrozenDictionary<string, string>? Unformat(string formatted)
        {
            if (string.IsNullOrWhiteSpace(formatted))
            {
                return FrozenDictionary<string, string>.Empty;
            }

            Dictionary<string, string> boundedValues = [];

            if (!HasParameters)
            {
                return formatted.Equals(Template, StringComparison.InvariantCulture)
                    ? FrozenDictionary<string, string>.Empty
                    : null;
            }

            int searchStartIndex = 0;
            for (int i = 0; i < Parts.Count; i++)
            {
                StringTemplatePart part = Parts[i];

                if (part.IsParameter)
                {
                    if (i + 1 < Parts.Count)
                    {
                        StringTemplatePart nextPart = Parts[i + 1];
                        int index = formatted.IndexOf(nextPart.Value, searchStartIndex, StringComparison.InvariantCulture);

                        if (index < 0)
                        {
                            return null;
                        }

                        string value0 = formatted[searchStartIndex..index];

                        if (string.IsNullOrWhiteSpace(value0))
                        {
                            return null;
                        }

                        searchStartIndex = index;

                        boundedValues.Add(part.Value, value0);
                    }
                    else
                    {
                        string value1 = formatted[searchStartIndex..];

                        boundedValues.Add(part.Value, value1);
                    }
                }
                else
                {
                    if (searchStartIndex + part.Value.Length > formatted.Length)
                    {
                        return null;
                    }

                    int index = formatted.IndexOf(part.Value, searchStartIndex, part.Value.Length, StringComparison.InvariantCulture);

                    if (index < 0)
                    {
                        return null;
                    }

                    searchStartIndex = part.Value.Length + index;
                }
            }

            return boundedValues.ToFrozenDictionary();
        }

        /// <summary>
        /// Formats the message.
        /// </summary>
        /// <param name="placeholderValues"></param>
        /// <returns></returns>
        public string Format(IEnumerable<KeyValuePair<string, object?>> placeholderValues)
        {
            ArgumentNullException.ThrowIfNull(placeholderValues);

            if (!HasParameters)
            {
                return Template;
            }

            IReadOnlyDictionary<string, object?> dict = placeholderValues as IReadOnlyDictionary<string, object?> ??
                new Dictionary<string, object?>(placeholderValues);

            return Format(dict);
        }

        /// <summary>
        /// Formats the message.
        /// </summary>
        /// <param name="placeholderValues"></param>
        /// <returns></returns>
        public string Format(Dictionary<string, object?> placeholderValues)
        {
            ArgumentNullException.ThrowIfNull(placeholderValues);

            if (!HasParameters)
            {
                return Template;
            }

            IReadOnlyDictionary<string, object?> dict = placeholderValues as IReadOnlyDictionary<string, object?> ??
                new Dictionary<string, object?>(placeholderValues);

            return Format(dict);
        }

        /// <summary>
        /// Formats the message.
        /// </summary>
        /// <param name="placeholderValues"></param>
        /// <returns></returns>
        public string Format(FrozenDictionary<string, object?> placeholderValues)
        {
            ArgumentNullException.ThrowIfNull(placeholderValues);

            if (!HasParameters)
            {
                return Template;
            }

            return Format(placeholderValues as IReadOnlyDictionary<string, object?>);
        }

        /// <summary>
        /// Formats the message.
        /// </summary>
        /// <param name="placeholderValues"></param>
        /// <returns></returns>
        public string Format(IDictionary<string, object?> placeholderValues)
        {
            ArgumentNullException.ThrowIfNull(placeholderValues);

            if (!HasParameters)
            {
                return Template;
            }

            IReadOnlyDictionary<string, object?> dict = placeholderValues as IReadOnlyDictionary<string, object?> ??
                new Dictionary<string, object?>(placeholderValues);

            return Format(dict);
        }

        /// <summary>
        /// Formats the message.
        /// </summary>
        /// <param name="placeholderValues"></param>
        /// <returns></returns>
        public string Format(IReadOnlyDictionary<string, object?> placeholderValues)
        {
            ArgumentNullException.ThrowIfNull(placeholderValues);

            if (!HasParameters)
            {
                return Template;
            }

            StringBuilder builder = new();

            for (int i = 0; i < Parts.Count; i++)
            {
                StringTemplatePart part = Parts[i];

                if (part.IsParameter && part.Parameter is not null)
                {
                    object? placeholderValue = placeholderValues[part.Parameter];

                    string? text = part.Format is null
                        ? placeholderValue?.ToString()
                        : string.Format($"{{0:{part.Format}}}", placeholderValue);

                    builder.Append(text ?? string.Empty);
                }
                else
                {
                    builder.Append(part.Value);
                }
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is StringTemplate st && Equals(st);
        }

        /// <inheritdoc/>
        public bool Equals(StringTemplate? other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return other is not null &&
                other.HasParameters == HasParameters &&
                other.Parts.Count == Parts.Count &&
                other.Template == Template;
        }

        /// <summary>
        /// Determines whether two specified string templates have the same value.
        /// </summary>
        public static bool operator ==(in StringTemplate left, in StringTemplate right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified string templates have different values.
        /// </summary>
        public static bool operator !=(in StringTemplate left, in StringTemplate right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            return HashCode.Combine(Template, HasParameters);
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string? ToString()
        {
            return Template;
        }
    }
}
