using System;
using NavisAutoClash.Core.Domain.Models;
using Xunit;

namespace NavisAutoClash.Tests.Core
{
    public sealed class ClashRuleConfigTests
    {
        [Fact]
        public void Default_IsValid()
        {
            var config = ClashRuleConfig.Default;
            Assert.Equal(ClashType.Hard, config.TestType);
            Assert.Equal(0.0, config.Tolerance);
            Assert.Contains("{SetA}", config.NamingPattern);
            Assert.Contains("{ModelB}", config.NamingPattern);
        }

        [Fact]
        public void Constructor_NegativeTolerance_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ClashRuleConfig("P", ClashType.Clearance, -0.001, "{SetA} vs {ModelB}"));
        }

        [Fact]
        public void Constructor_PatternMissingSetAToken_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new ClashRuleConfig("P", ClashType.Hard, 0, "only {ModelB}"));
        }

        [Fact]
        public void Constructor_PatternMissingModelBToken_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new ClashRuleConfig("P", ClashType.Hard, 0, "only {SetA}"));
        }

        [Fact]
        public void GenerateTestName_ReplacesTokens()
        {
            var config = new ClashRuleConfig("P", ClashType.Hard, 0, "[{SetA}] — [{ModelB}]");
            var name = config.GenerateTestName("Base Build", "Structure");
            Assert.Equal("[Base Build] — [Structure]", name);
        }

        [Fact]
        public void GenerateTestName_EmptySetName_Throws()
        {
            var config = ClashRuleConfig.Default;
            Assert.Throws<ArgumentException>(() => config.GenerateTestName("", "M"));
        }
    }

    public sealed class SelectionSetInfoTests
    {
        [Fact]
        public void Constructor_EmptyName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new SelectionSetInfo("", "path", false));
        }

        [Fact]
        public void Equality_SameFullPath_Equal()
        {
            var a = new SelectionSetInfo("Walls", "Root / Walls", false);
            var b = new SelectionSetInfo("Walls", "Root / Walls", false);
            Assert.Equal(a, b);
        }

        [Fact]
        public void Equality_DifferentFullPath_NotEqual()
        {
            var a = new SelectionSetInfo("Walls", "Root / Arch / Walls", false);
            var b = new SelectionSetInfo("Walls", "Root / Struct / Walls", false);
            Assert.NotEqual(a, b);
        }
    }

    public sealed class NwcModelInfoTests
    {
        [Fact]
        public void DisplayName_ExtractedFromFilePath()
        {
            var info = new NwcModelInfo(0, @"C:\project\models\Structure.nwc");
            Assert.Equal("Structure", info.DisplayName);
        }

        [Fact]
        public void Constructor_NegativeIndex_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new NwcModelInfo(-1, "C:\\m.nwc"));
        }

        [Fact]
        public void Constructor_EmptyFilePath_Throws()
        {
            Assert.Throws<ArgumentException>(() => new NwcModelInfo(0, ""));
        }
    }
}
