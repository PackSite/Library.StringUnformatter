namespace PackSite.Library.StringUnformatter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// String template object.
    /// </summary>
    public class StringTemplate : IEquatable<StringTemplate>
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
        public bool HasParameters { get; private set; }

        private StringTemplate(string template, IReadOnlyList<StringTemplatePart> parts, bool hasParameters)
        {
            Template = template;
            Parts = parts;
            HasParameters = hasParameters;
        }

        /// <summary>
        /// Parse template from string.
        /// </summary>
        public static StringTemplate Parse(string template)
        {
            (bool HasParameters, List<StringTemplatePart> Parts) parsed = ParseTemplate(template);

            return new StringTemplate(template, parsed.Parts, parsed.HasParameters);
        }

        /// <summary>
        /// Parse template from string.
        /// </summary>
        public static StringTemplate FromParts(IReadOnlyList<StringTemplatePart> parts)
        {
            string template = StringTemplatePart.Join(parts);

            return new StringTemplate(template, parts, parts.Where(x => x.IsParameter).Any());
        }

        private static (bool HasParameters, List<StringTemplatePart> Parts) ParseTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException($"'{nameof(template)}' cannot be null or whitespace", nameof(template));
            }

            List<StringTemplatePart> parts = new();
            bool hasParameters = false;

            ReadOnlySpan<char> span = template.AsSpan();

            bool isOpened = false;
            int sliceStart = 0;

            for (int i = 0; i < span.Length; ++i)
            {
                char ch = span[i];
                if (!isOpened && ch == '{')
                {
                    if (i - sliceStart != 0)
                    {
                        string tmp = new(span[sliceStart..i]);

                        if (string.IsNullOrWhiteSpace(tmp))
                        {
                            throw new FormatException($"String template '{template}' is invalid. Template cannot contain empty parameters.");
                        }

                        parts.Add(new StringTemplatePart(tmp));
                    }

                    isOpened = true;
                    sliceStart = i + 1;
                }
                else if (isOpened && ch == '}')
                {
                    if (parts.Count > 0 && parts[^1].IsParameter)
                    {
                        throw new FormatException($"String template '{template}' is invalid. Template cannot contain two or more subsequent parameters.");
                    }

                    string tmp = new(span[sliceStart..i]);

                    if (string.IsNullOrWhiteSpace(tmp))
                    {
                        throw new FormatException($"String template '{template}' is invalid. Template cannot contain empty parameters.");
                    }

                    parts.Add(new StringTemplatePart(tmp, true));
                    hasParameters = true;

                    isOpened = false;
                    sliceStart = i + 1;
                }
                else if (ch == '{' || ch == '}')
                {
                    throw new FormatException($"String template '{template}' is invalid. Parameter {(isOpened ? "opened" : "closed")} but was never {(!isOpened ? "opened" : "closed")}.");
                }
            }

            if (span.Length != sliceStart)
            {
                string tmp = new(span[sliceStart..]);

                if (string.IsNullOrWhiteSpace(tmp))
                {
                    throw new FormatException($"String template '{template}' is invalid. Template cannot contain empty parameters.");
                }

                parts.Add(new StringTemplatePart(tmp));
            }

            return (hasParameters, parts);
        }

        /// <summary>
        /// Checks whether formatted string matches the template.
        /// </summary>
        /// <param name="formatted"></param>
        /// <returns></returns>
        public bool Matches(string formatted)
        {
            return HasParameters ? Unformat(formatted) is not null : formatted.Equals(Template, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Returns unformated parameters, null when failed to unformat, or empty collection when successfully unformatted but no parameters were present in template.
        /// </summary>
        public Dictionary<string, string>? Unformat(string formatted)
        {
            Dictionary<string, string> boundedValues = new();

            int searchStartIndex = 0;
            for (int i = 0; i < Parts.Count; ++i)
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

                        if (string.IsNullOrWhiteSpace(value1))
                        {
                            return null;
                        }

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

            return boundedValues;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is StringTemplate st && Equals(st);
        }

        /// <inheritdoc/>
        public bool Equals(StringTemplate? other)
        {
            return other is not null &&
                other.HasParameters == HasParameters &&
                other.Parts.Count == Parts.Count &&
                other.Template == Template; ;
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
