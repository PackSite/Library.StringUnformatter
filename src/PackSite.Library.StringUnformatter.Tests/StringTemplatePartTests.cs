namespace PackSite.Library.StringUnformatter.Tests
{
    using FluentAssertions;
    using Xunit;

    public class StringTemplatePartTests
    {
        [Fact]
        public void Part_value_should_have_proper_values()
        {
            new StringTemplatePart().Value.Should().NotBeNull();
            new StringTemplatePart().IsParameter.Should().BeFalse();

            StringTemplatePart first = new StringTemplatePart("test");
            first.Value.Should().Be("test");
            first.IsParameter.Should().BeFalse();

            StringTemplatePart second = new StringTemplatePart("other-test", true);
            second.Value.Should().Be("other-test");
            second.IsParameter.Should().BeTrue();
        }

        [Fact]
        public void Parts_should_be_equal()
        {
            StringTemplatePart first = new StringTemplatePart();
            StringTemplatePart second = new StringTemplatePart();

            first.Should().Be(second);
            (first == second).Should().BeTrue();
            (first != second).Should().BeFalse();
        }

        [Fact]
        public void Parts_should_be_equal_with_values()
        {
            StringTemplatePart first = new StringTemplatePart("test", true);
            StringTemplatePart second = new StringTemplatePart("test", true);

            first.Should().Be(second);
            (first == second).Should().BeTrue();
            (first != second).Should().BeFalse();
        }

        [Fact]
        public void Parts_should_not_be_equal_by_value()
        {
            StringTemplatePart first = new StringTemplatePart("test", true);
            StringTemplatePart second = new StringTemplatePart("other-test", true);

            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
            (first != second).Should().BeTrue();
        }

        [Fact]
        public void Parts_should_not_be_equal_by_param()
        {
            StringTemplatePart first = new StringTemplatePart("test", true);
            StringTemplatePart second = new StringTemplatePart("test", false);

            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
            (first != second).Should().BeTrue();
        }

        [Fact]
        public void Parts_should_not_be_equal_by_both()
        {
            StringTemplatePart first = new StringTemplatePart("test", true);
            StringTemplatePart second = new StringTemplatePart("other-test", false);

            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
            (first != second).Should().BeTrue();
        }
    }
}
