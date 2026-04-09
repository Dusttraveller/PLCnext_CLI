#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using CommandLine.Text;
using PlcNext.CommandLine;
using Shouldly;
using Xunit;

namespace Test.PlcNext.UnitTests
{
    public class AttributeExtensionsTests
    {
        // MultilineTextAttribute is abstract — use the concrete AssemblyLicenseAttribute subclass
        [Fact]
        public void AddToHelpText_WithOneLine_AddsLineToHelpText()
        {
            var attribute = new AssemblyLicenseAttribute("Line one");
            var helpText = new HelpText();

            attribute.AddToHelpText(helpText);

            helpText.ToString().ShouldContain("Line one");
        }

        [Fact]
        public void AddToHelpText_WithMultipleLines_AddsAllLinesToHelpText()
        {
            var attribute = new AssemblyLicenseAttribute("First line", "Second line", "Third line");
            var helpText = new HelpText();

            attribute.AddToHelpText(helpText);

            string result = helpText.ToString();
            result.ShouldContain("First line");
            result.ShouldContain("Second line");
            result.ShouldContain("Third line");
        }

        [Fact]
        public void AddToHelpText_WithTrailingEmptyLines_StillContainsDefinedLines()
        {
            var attribute = new AssemblyLicenseAttribute("Only line");
            var helpText = new HelpText();

            attribute.AddToHelpText(helpText);

            helpText.ToString().ShouldContain("Only line");
        }

        [Fact]
        public void AddToHelpText_WithAllFiveLines_AddsAllFiveLines()
        {
            var attribute = new AssemblyLicenseAttribute("L1", "L2", "L3", "L4", "L5");
            var helpText = new HelpText();

            attribute.AddToHelpText(helpText);

            string result = helpText.ToString();
            result.ShouldContain("L1");
            result.ShouldContain("L2");
            result.ShouldContain("L3");
            result.ShouldContain("L4");
            result.ShouldContain("L5");
        }

        [Fact]
        public void ToTotalAccessType_ReturnsCorrectTotalAccessType()
        {
            var result = typeof(string).ToTotalAccessType();

            result.Type.ShouldBe(typeof(string));
        }

        [Fact]
        public void ToTotalAccessType_WithNullConstructor_TypeIsNull()
        {
            var result = new TotalAccessType(null);

            result.Type.ShouldBeNull();
        }
    }
}
