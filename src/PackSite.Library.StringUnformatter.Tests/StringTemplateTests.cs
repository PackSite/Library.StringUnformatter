namespace PackSite.Library.StringUnformatter.Tests
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using VerifyXunit;
    using Xunit;

    public class StringTemplateTests
    {
        [Fact]
        public async Task Should_parse()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/QWErty");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_create_from_parts()
        {
            StringTemplate template = StringTemplate.FromParts(new StringTemplatePart[]
            {
                new("category/delete/"),
                new("Id", true),
                new("/QWErty"),
            });

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_when_ending_with_parameter()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/{Value}");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_single_value()
        {
            StringTemplate template = StringTemplate.Parse("category/get-all");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_with_colon()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/{date:d}");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_single_parameter()
        {
            StringTemplate template = StringTemplate.Parse("{Id}");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_when_starting_with_parameter()
        {
            StringTemplate template = StringTemplate.Parse("{Group}/category/delete/{Id}/{Value}");

            await Verifier.Verify(template);
        }

        [Fact]
        public void Should_not_parse_when_template_is_empty()
        {
            Action action = () =>
            {
                StringTemplate template = StringTemplate.Parse(" ");
            };

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData("{Group}{XYZ}/category/delete/{Id}/{Value}")]
        [InlineData("{XYZ}/category/delete/{Id}{Value}{XYZ}")]
        public void Should_not_parse_with_two_or_more_subsequent_parameters(string str)
        {
            Action action = () =>
            {
                StringTemplate template = StringTemplate.Parse(str);
            };

            Assert.Throws<FormatException>(action);
        }

        [Theory]
        [InlineData("{}/category/delete/{Id}/")]
        [InlineData("/cate{}gory/delete/{Id}/{}")]
        [InlineData("/category/delete/{Id}/{}")]
        [InlineData("/category/delete/{Arg}/{}/")]
        public void Should_not_parse_when_empty_parameter(string str)
        {
            Action action = () =>
            {
                StringTemplate template = StringTemplate.Parse(str);
            };

            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public async Task Should_parse_with_escaped_brackets1()
        {
            StringTemplate template = StringTemplate.Parse("/category/{{V}}/delete/{Id}/");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_with_escaped_brackets2()
        {
            StringTemplate template = StringTemplate.Parse("/category/{{{{V}}}}/delete/{Id}/}}");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_with_escaped_brackets3()
        {
            StringTemplate template = StringTemplate.Parse("/category/{{{V}}}/delete/{Id}/");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_with_escaped_brackets4()
        {
            StringTemplate template = StringTemplate.Parse("{{XYZ/category/delete/{Id}/{Value}");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_with_escaped_brackets5()
        {
            StringTemplate template = StringTemplate.Parse("{{XYZ/category/delete/{Id}/{Value}}}");

            await Verifier.Verify(template);
        }

        [Fact]
        public async Task Should_parse_with_escaped_brackets6()
        {
            StringTemplate template = StringTemplate.Parse("{{{{XYZ/category/delete/{Id}{{/}}{Value}}}}}}}");

            await Verifier.Verify(template);
        }

        [Theory]
        [InlineData("{Group{XYZ}/category/delete/{Id}/{Value}")]
        [InlineData("{XYZ/category/delete/{Id}/{Value}")]
        [InlineData("{{XYZ}/category/delete/{Id}/{Value}")]
        [InlineData("{XYZ}}/category/delete/{Id}/{Value}")]
        [InlineData("XYZ}/category/delete/{Id}/{XYZ}")]
        [InlineData("XYZ}/category/delete/{Id}/{{XYZ}")]
        [InlineData("XYZ}/category/delete/{Id}/{XYZ}}")]
        public void Should_not_parse_when_missing_bracket(string str)
        {
            Action action = () =>
            {
                StringTemplate template = StringTemplate.Parse(str);
            };

            Assert.Throws<FormatException>(action);
        }

        [Theory]
        [InlineData("category/get-all", "category/get-all")]
        [InlineData("category/delete/{Id}/", "category/delete/00-000/")]
        [InlineData("category/delete/{Id}/ABC", "category/delete/00-000/ABC")]
        [InlineData("category/delete/{Id}/ABC{X}/QWERTY", "category/delete/00-000/ABC/str/QWERTY")]
        [InlineData("category/delete/{Id}/{Value}", "category/delete/00-000/test")]
        [InlineData("{Id}/{Value}", "category/delete")]
        public void Should_match_string_to_template(string input, string output)
        {
            StringTemplate template = StringTemplate.Parse(input);

            Assert.True(template.Matches(output));
        }

        [Theory]
        [InlineData("category/get-all", "category/delete/00-000/test")]
        [InlineData("category/get-all", "category/get-all/00-000/test")]
        [InlineData("category/delete/{Id}/{Value}", "category//00-000/test")]
        [InlineData("category/delete/{Id}/{Value}", "category/delet/00-000/test")]
        [InlineData("category/delete/{Id}/{Value}", "category/deletx/00-000/test")]
        [InlineData("category/delete/{Id}/{Value}", "category/delete/00-000")]
        [InlineData("category/delete/{Id}/{Value}", "category/delete///")]
        [InlineData("category/delete/{Id}/{Value}", "category/delete//")]
        [InlineData("category/delete/{Id}/{Value}", "/delete///")]
        [InlineData("category/delete/{Id}/{Value}", "category/00-000/test")]
        public void Should_not_match_string_to_template(string input, string output)
        {
            StringTemplate template = StringTemplate.Parse(input);

            Assert.False(template.Matches(output));
        }

        [Fact]
        public async Task Should_bind_string()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/{Value}");

            FrozenDictionary<string, string>? result = template.Unformat("category/delete/00-000/test");

            await Verifier.Verify(result);
        }

        [Fact]
        public async Task Should_bind_template_with_no_parameters_to_empty_collection()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/Id/Value");

            FrozenDictionary<string, string>? result = template.Unformat("category/delete/Id/Value");

            await Verifier.Verify(result);
        }

        [Fact]
        public async Task Should_bind_template_with_no_parameters_to_null()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/Id/Value");

            FrozenDictionary<string, string>? result = template.Unformat("category/delete/00-000/test");

            await Verifier.Verify(result);
        }

        [Fact]
        public void Should_be_comparable()
        {
            StringTemplate template = StringTemplate.FromParts(new StringTemplatePart[]
            {
                new("category/delete/this/"),
                new("Id", true),
                new("/QWErty"),
            });

            StringTemplate otherTemplate = StringTemplate.Parse("category/delete/this/{Id}/QWErty");
            StringTemplate otherWrongTemplate = StringTemplate.Parse("category/delete/this/{Id}/WErty");

            Assert.Equal(template, template);

            Assert.NotEqual(expected: template, new object());
            Assert.NotEqual(otherTemplate, new object());
            Assert.NotEqual(otherWrongTemplate, new object());

            Assert.Equal(template, otherTemplate);
            Assert.NotEqual(template, otherWrongTemplate);
        }

        [Fact]
        public async Task Should_format()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{id:X}/{number:C2}/{text}");

            Guid id = Guid.Parse("{EFAAC911-2CB3-454F-AA44-C2AD37FA7FCD}");
            double number = 1.23433;
            string text = "avx";

            string formatted = template.Format(new Dictionary<string, object?>
            {
                ["id"] = id,
                ["number"] = number,
                ["text"] = text,
            });

            await Verifier.Verify(new
            {
                Template = template,
                Formatted = formatted,
            });
        }
    }
}
