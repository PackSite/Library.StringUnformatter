namespace PackSite.Library.StringUnformatter.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using VerifyXunit;
    using Xunit;

    public class StringTemplatePartTests
    {
        [Fact]
        public async Task Part_should_have_proper_values()
        {
            await Verifier.Verify(new
            {
                EmptyParameter = new StringTemplatePart(null, true),
                EmptyNonParameter = new StringTemplatePart(null, true),
                Parameter = new StringTemplatePart("other-test", true),
                NonParameter = new StringTemplatePart("other-test"),
            });
        }

        [Fact]
        public async Task Part_with_format_should_have_proper_values()
        {
            StringTemplatePart part = new("test:format", true);

            await Verifier.Verify(part);
        }

        [Fact]
        public async Task Parts_should_be_equal()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("test", true);

            Assert.Equal(first, first);
            Assert.Equal(first, second);
        }

        [Fact]
        public async Task Parts_should_not_be_equal_by_value()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("other-test", true);

            Assert.NotEqual(first, new object());
            Assert.NotEqual(first, second);
        }

        [Fact]
        public async Task Parts_should_not_be_equal_by_param()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("test", false);

            Assert.NotEqual(first, new object());
            Assert.NotEqual(first, second);
        }

        [Fact]
        public async Task Parts_should_not_be_equal_by_both()
        {
            StringTemplatePart first = new("test", true);
            StringTemplatePart second = new("other-test", false);

            Assert.NotEqual(first, new object());
            Assert.NotEqual(first, second);
        }

        [Fact]
        public async Task Empty_collection_should_not_be_joined()
        {
            Action action = () =>
            {
                string result0 = StringTemplatePart.Join(new List<StringTemplatePart>());
            };

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public async Task Parts_should_be_joined_using_enumerable()
        {
            List<StringTemplatePart> list =
            [
                new("test-param", true),
                new("other-Test-Value"),
            ];

            string result = StringTemplatePart.Join(list);

            Assert.Equal("{test-param}other-Test-Value", result);
        }

        [Fact]
        public async Task Parts_should_be_joined_using_enumerable_and_params_count_should_be_returned()
        {
            List<StringTemplatePart> list =
            [
                new("test-param", true),
                new("other-Test-Value"),
            ];

            string result = StringTemplatePart.Join(list, out int parametersCount);

            Assert.Equal("{test-param}other-Test-Value", result);
            Assert.Equal(1, parametersCount);
        }

        [Fact]
        public async Task Parts_should_be_joined_using_params()
        {
            string result = StringTemplatePart.Join(new("test-param", true), new("other-Test-Value"));

            Assert.Equal("{test-param}other-Test-Value", result);
        }
    }
}
