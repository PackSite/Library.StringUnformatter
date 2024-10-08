namespace PackSite.Library.StringUnformatter.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Xunit;

    public class StringTemplatePartTests
    {
        [Fact]
        public void Part_should_have_proper_values()
        {
            new StringTemplatePart(null, true).Value.Should().NotBeNull();
            new StringTemplatePart(null, false).IsParameter.Should().BeFalse();

            StringTemplatePart first = new("test");
            first.Value.Should().Be("test");
            first.Parameter.Should().BeNull();
            first.Format.Should().BeNull();
            first.IsParameter.Should().BeFalse();

            StringTemplatePart second = new("other-test", true);
            second.Value.Should().Be("other-test");
            second.Parameter.Should().Be("other-test");
            second.Format.Should().BeNull();
            second.IsParameter.Should().BeTrue();
        }

        [Fact]
        public void Part_with_format_should_have_proper_values()
        {
            StringTemplatePart first = new("test:format", true);
            first.Value.Should().Be("test:format");
            first.Parameter.Should().Be("test");
            first.Format.Should().Be("format");
            first.IsParameter.Should().BeTrue();
        }

        [Fact]
        public void Parts_should_be_equal()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("test", true);

            first.Should().Be(first);
            second.Should().Be(second);

            first.Should().Be(second);
            (first == second).Should().BeTrue();
            (first != second).Should().BeFalse();
        }

        [Fact]
        public void Parts_should_not_be_equal_by_value()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("other-test", true);

            first.Should().NotBe(new object());
            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
            (first != second).Should().BeTrue();
        }

        [Fact]
        public void Parts_should_not_be_equal_by_param()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("test", false);

            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
            (first != second).Should().BeTrue();
        }

        [Fact]
        public void Parts_should_not_be_equal_by_both()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("other-test", false);

            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
            (first != second).Should().BeTrue();
        }

        [Fact]
        public void Empty_collection_should_not_be_joined()
        {
            Action action = () =>
            {
                string result0 = StringTemplatePart.Join(new List<StringTemplatePart>());
            };

            action.Should().Throw<ArgumentException>().WithMessage("*Parts collections must contain at least one element*");
        }

        [Fact]
        public void Parts_should_be_joined_using_enumerable()
        {
            List<StringTemplatePart> list =
            [
                new("test-param", true),
                new("other-Test-Value"),
            ];

            string result = StringTemplatePart.Join(list);
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("{test-param}other-Test-Value");
        }

        [Fact]
        public void Parts_should_be_joined_using_enumerable_and_params_count_should_be_returned()
        {
            List<StringTemplatePart> list =
            [
                new("test-param", true),
                new("other-Test-Value"),
            ];

            string result = StringTemplatePart.Join(list, out int parametersCount);
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("{test-param}other-Test-Value");

            parametersCount.Should().Be(1);
        }

        [Fact]
        public void Parts_should_be_joined_using_params()
        {
            string result = StringTemplatePart.Join(new("test-param", true), new("other-Test-Value"));

            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("{test-param}other-Test-Value");
        }
    }
}
