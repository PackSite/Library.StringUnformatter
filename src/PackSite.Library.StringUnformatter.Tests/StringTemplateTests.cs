namespace PackSite.Library.StringUnformatter.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Xunit;

    public class StringTemplateTests
    {
        [Fact]
        public void Should_parse()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/QWErty");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("category/delete/"),
                new("Id", true),
                new("/QWErty")
            });
        }

        [Fact]
        public void Should_create_from_parts()
        {
            StringTemplate template = StringTemplate.FromParts(new StringTemplatePart[]
            {
                new("category/delete/"),
                new("Id", true),
                new("/QWErty")
            });

            StringTemplate otherTemplate = StringTemplate.Parse("category/delete/{Id}/QWErty");
            template.Template.Should().Be("category/delete/{Id}/QWErty");
            template.Should().Be(otherTemplate);
        }

        [Fact]
        public void Should_parse_when_ending_with_parameter()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/{Value}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("category/delete/"),
                new("Id", true),
                new("/"),
                new("Value", true)
            });
        }

        [Fact]
        public void Should_parse_single_value()
        {
            StringTemplate template = StringTemplate.Parse("category/get-all");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("category/get-all")
            });
        }

        [Fact]
        public void Should_parse_with_colon()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/{date:d}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("category/delete/"),
                new("Id", true),
                new("/"),
                new("date:d", true)
            });
        }

        [Fact]
        public void Should_parse_single_parameter()
        {
            StringTemplate template = StringTemplate.Parse("{Id}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("Id", true)
            });
        }

        [Fact]
        public void Should_parse_when_starting_with_parameter()
        {
            StringTemplate template = StringTemplate.Parse("{Group}/category/delete/{Id}/{Value}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("Group", true),
                new("/category/delete/"),
                new("Id", true),
                new("/"),
                new("Value", true)
            });
        }

        [Fact]
        public void Should_not_parse_when_template_is_empty()
        {
            Action action = () =>
            {
                StringTemplate template = StringTemplate.Parse(" ");
            };

            action.Should().Throw<ArgumentException>().WithMessage("*cannot be null or whitespace*");
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

            action.Should().Throw<FormatException>().WithMessage("*Template cannot contain two or more subsequent parameters.");
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

            action.Should().Throw<FormatException>().WithMessage("*Template cannot contain empty parameters.");
        }

        [Fact]
        public void Should_parse_with_escaped_brackets1()
        {
            StringTemplate template = StringTemplate.Parse("/category/{{V}}/delete/{Id}/");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("/category/{V}/delete/"),
                new("Id", true),
                new("/"),
            });
        }

        [Fact]
        public void Should_parse_with_escaped_brackets2()
        {
            StringTemplate template = StringTemplate.Parse("/category/{{{{V}}}}/delete/{Id}/}}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("/category/{{V}}/delete/"),
                new("Id", true),
                new("/}"),
            });
        }

        [Fact]
        public void Should_parse_with_escaped_brackets3()
        {
            StringTemplate template = StringTemplate.Parse("/category/{{{V}}}/delete/{Id}/");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("/category/{"),
                new("V", true),
                new("}/delete/"),
                new("Id", true),
                new("/"),
            });
        }

        [Fact]
        public void Should_parse_with_escaped_brackets4()
        {
            StringTemplate template = StringTemplate.Parse("{{XYZ/category/delete/{Id}/{Value}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("{XYZ/category/delete/"),
                new("Id", true),
                new("/"),
                new("Value", true)
            });
        }

        [Fact]
        public void Should_parse_with_escaped_brackets5()
        {
            StringTemplate template = StringTemplate.Parse("{{XYZ/category/delete/{Id}/{Value}}}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("{XYZ/category/delete/"),
                new("Id", true),
                new("/"),
                new("Value", true),
                new("}")
            });
        }

        [Fact]
        public void Should_parse_with_escaped_brackets6()
        {
            StringTemplate template = StringTemplate.Parse("{{{{XYZ/category/delete/{Id}{{/}}{Value}}}}}}}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("{{XYZ/category/delete/"),
                new("Id", true),
                new("{/}"),
                new("Value", true),
                new("}}}")
            });
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

            action.Should().Throw<FormatException>().WithMessage("*but was never*.");
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

            template.Matches(output).Should().BeTrue();
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

            template.Matches(output).Should().BeFalse();
        }

        [Fact]
        public void Should_bind_string()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{Id}/{Value}");

            template.Unformat("category/delete/00-000/test").Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "Id", "00-000" },
                { "Value", "test" }
            });
        }

        [Fact]
        public void Should_bind_template_with_no_parameters_to_empty_collection()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/Id/Value");

            template.Unformat("category/delete/Id/Value").Should().BeEmpty();
        }

        [Fact]
        public void Should_bind_template_with_no_parameters_to_null()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/Id/Value");

            template.Unformat("category/delete/00-000/test").Should().BeNull();
        }

        [Fact]
        public void Should_be_comparable()
        {
            StringTemplate template = StringTemplate.FromParts(new StringTemplatePart[]
            {
                new("category/delete/this/"),
                new("Id", true),
                new("/QWErty")
            });

            StringTemplate otherTemplate = StringTemplate.Parse("category/delete/this/{Id}/QWErty");
            StringTemplate otherWrongTemplate = StringTemplate.Parse("category/delete/this/{Id}/WErty");

            template.Should().Be(template);

            template.Should().NotBe(new object());
            otherTemplate.Should().NotBe(new object());
            otherWrongTemplate.Should().NotBe(new object());

            (template == otherTemplate).Should().BeTrue();
            (template != otherTemplate).Should().BeFalse();
            template.Equals(otherTemplate).Should().BeTrue();

            (template == otherWrongTemplate).Should().BeFalse();
            (template != otherWrongTemplate).Should().BeTrue();
            template.Equals(otherWrongTemplate).Should().BeFalse();
        }

        [Fact]
        public void Should_format()
        {
            StringTemplate template = StringTemplate.Parse("category/delete/{id:X}/{number:C2}/{text}");

            template.Parts.Should().BeEquivalentTo(new StringTemplatePart[]
            {
                new("category/delete/"),
                new("id:X", true),
                new("/"),
                new("number:C2", true),
                new("/"),
                new("text", true),
            });

            Guid id = Guid.NewGuid();
            double number = 1.23;
            string text = "avx";

            string formatted = template.Format(new Dictionary<string, object?>
            {
                ["id"] = id,
                ["number"] = number,
                ["text"] = text,
            });

            formatted.Should().Be($"category/delete/{id:X}/{number:C2}/{text}");
        }
    }
}
