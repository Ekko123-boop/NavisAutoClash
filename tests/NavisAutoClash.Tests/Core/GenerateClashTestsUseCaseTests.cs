using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Application.UseCases;
using NavisAutoClash.Core.Domain.Models;
using Xunit;

namespace NavisAutoClash.Tests.Core
{
    public sealed class GenerateClashTestsUseCaseTests
    {
        // ── fixtures ──────────────────────────────────────────────────────────
        private static SelectionSetInfo MakeSet(string name) =>
            new SelectionSetInfo(name, $"Root / {name}", isSearchSet: false);

        private static NwcModelInfo MakeModel(string name, int idx = 0) =>
            new NwcModelInfo(idx, $"C:\\models\\{name}.nwc");

        private static ClashRuleConfig DefaultRule =>
            ClashRuleConfig.Default;

        private GenerateClashTestsUseCase MakeUseCase(
            out Mock<IClashService> clashSvcMock,
            out Mock<IAppLogger> logMock)
        {
            clashSvcMock = new Mock<IClashService>();
            logMock = new Mock<IAppLogger>();

            // By default, CreateClashTests returns back the input names
            clashSvcMock
                .Setup(s => s.CreateClashTests(It.IsAny<IEnumerable<ClashTestDefinition>>()))
                .Returns<IEnumerable<ClashTestDefinition>>(defs =>
                    (IReadOnlyList<string>)defs.Select(d => d.TestName).ToList());

            return new GenerateClashTestsUseCase(clashSvcMock.Object, logMock.Object);
        }

        // ── construction ──────────────────────────────────────────────────────

        [Fact]
        public void Constructor_NullClashService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GenerateClashTestsUseCase(null!, new Mock<IAppLogger>().Object));
        }

        [Fact]
        public void Constructor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GenerateClashTestsUseCase(new Mock<IClashService>().Object, null!));
        }

        // ── Execute — cartesian product ────────────────────────────────────────

        [Fact]
        public void Execute_TwoSetsThreeModels_Creates6Tests()
        {
            var uc = MakeUseCase(out var svcMock, out _);
            var sets = new[] { MakeSet("Arch"), MakeSet("Struct") };
            var models = new[] { MakeModel("Mech", 0), MakeModel("Elec", 1), MakeModel("Plumb", 2) };

            var result = uc.Execute(sets, models, DefaultRule);

            Assert.Equal(6, result.Count);
            svcMock.Verify(s => s.CreateClashTests(
                It.Is<IEnumerable<ClashTestDefinition>>(d => d.Count() == 6)), Times.Once);
        }

        [Fact]
        public void Execute_OneSetOneModel_TestNameUsesPattern()
        {
            var uc = MakeUseCase(out _, out _);
            var sets = new[] { MakeSet("Base Build") };
            var models = new[] { MakeModel("Structure") };

            var result = uc.Execute(sets, models, DefaultRule);

            Assert.Single(result);
            Assert.Equal("Base Build vs Structure", result[0]);
        }

        [Fact]
        public void Execute_EmptySets_ThrowsInvalidOperation()
        {
            var uc = MakeUseCase(out _, out _);
            Assert.Throws<InvalidOperationException>(() =>
                uc.Execute(
                    Array.Empty<SelectionSetInfo>(),
                    new[] { MakeModel("M") },
                    DefaultRule));
        }

        [Fact]
        public void Execute_EmptyModels_ThrowsInvalidOperation()
        {
            var uc = MakeUseCase(out _, out _);
            Assert.Throws<InvalidOperationException>(() =>
                uc.Execute(
                    new[] { MakeSet("S") },
                    Array.Empty<NwcModelInfo>(),
                    DefaultRule));
        }

        [Fact]
        public void Execute_NullSets_ThrowsArgumentNull()
        {
            var uc = MakeUseCase(out _, out _);
            Assert.Throws<ArgumentNullException>(() =>
                uc.Execute(null!, new[] { MakeModel("M") }, DefaultRule));
        }

        [Fact]
        public void Execute_NullModels_ThrowsArgumentNull()
        {
            var uc = MakeUseCase(out _, out _);
            Assert.Throws<ArgumentNullException>(() =>
                uc.Execute(new[] { MakeSet("S") }, null!, DefaultRule));
        }

        [Fact]
        public void Execute_NullConfig_ThrowsArgumentNull()
        {
            var uc = MakeUseCase(out _, out _);
            Assert.Throws<ArgumentNullException>(() =>
                uc.Execute(new[] { MakeSet("S") }, new[] { MakeModel("M") }, null!));
        }

        // ── Execute — custom naming pattern ───────────────────────────────────

        [Fact]
        public void Execute_CustomPattern_AppliedToAllTests()
        {
            var uc = MakeUseCase(out _, out _);
            var rule = new ClashRuleConfig("Custom", ClashType.Hard, 0, "[{SetA}] x [{ModelB}]");
            var sets = new[] { MakeSet("A") };
            var models = new[] { MakeModel("B") };

            var result = uc.Execute(sets, models, rule);

            Assert.Equal("[A] x [B]", result[0]);
        }

        // ── Preview ───────────────────────────────────────────────────────────

        [Fact]
        public void Preview_DoesNotCallClashService()
        {
            var uc = MakeUseCase(out var svcMock, out _);
            var sets = new[] { MakeSet("S") };
            var models = new[] { MakeModel("M") };

            var preview = uc.Preview(sets, models, DefaultRule);

            Assert.Single(preview);
            svcMock.Verify(s => s.CreateClashTests(It.IsAny<IEnumerable<ClashTestDefinition>>()),
                Times.Never);
        }

        [Fact]
        public void Preview_ReturnsCorrectDefinitions()
        {
            var uc = MakeUseCase(out _, out _);
            var sets = new[] { MakeSet("Arch"), MakeSet("MEP") };
            var models = new[] { MakeModel("Site") };

            var preview = uc.Preview(sets, models, DefaultRule);

            Assert.Equal(2, preview.Count);
            Assert.Equal("Arch vs Site", preview[0].TestName);
            Assert.Equal("MEP vs Site", preview[1].TestName);
        }
    }
}
