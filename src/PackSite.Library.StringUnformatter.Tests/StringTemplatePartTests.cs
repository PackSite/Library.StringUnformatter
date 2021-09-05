namespace PackSite.Library.StringUnformatter.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Xunit;

    public class StringTemplatePartTests
    {
        [Fact]
        public void Part_value_should_have_proper_values()
        {
            new StringTemplatePart(null!, true).Value.Should().NotBeNull();
            new StringTemplatePart(null!, false).IsParameter.Should().BeFalse();

            StringTemplatePart first = new("test");
            first.Value.Should().Be("test");
            first.IsParameter.Should().BeFalse();

            StringTemplatePart second = new("other-test", true);
            second.Value.Should().Be("other-test");
            second.IsParameter.Should().BeTrue();
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
        public void Empty_collection_should_be_joined()
        {
            string result0 = StringTemplatePart.Join(new List<StringTemplatePart>());
            result0.Should().BeEmpty();

            string result1 = StringTemplatePart.Join(new List<StringTemplatePart>(), out int parametersCount);
            result1.Should().BeEmpty();
            parametersCount.Should().Be(0);

            string result2 = StringTemplatePart.Join(Array.Empty<StringTemplatePart>());
            result2.Should().BeEmpty();
        }

        [Fact]
        public void Parts_should_be_joined_using_enumerable()
        {
            List<StringTemplatePart> list = new()
            {
                new("test-param", true),
                new("other-Test-Value")
            };

            string result = StringTemplatePart.Join(list);
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("{test-param}other-Test-Value");
        }

        [Fact]
        public void Parts_should_be_joined_using_enumerable_and_params_count_should_be_returned()
        {
            List<StringTemplatePart> list = new()
            {
                new("test-param", true),
                new("other-Test-Value")
            };

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
