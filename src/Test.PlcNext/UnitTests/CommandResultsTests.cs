#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System.Collections.Generic;
using System.Linq;
using PlcNext.Common.Commands.CommandResults;
using Shouldly;
using Xunit;
using PathResult = PlcNext.Common.Commands.CommandResults.Path;

namespace Test.PlcNext.UnitTests
{
    public class CommandResultsTests
    {
        #region TargetResult

        [Fact]
        public void TargetResult_Constructor_SetsAllProperties()
        {
            var result = new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0", true);

            result.Name.ShouldBe("AXCF2152");
            result.Version.ShouldBe("22.0");
            result.LongVersion.ShouldBe("22.0.0.12345");
            result.ShortVersion.ShouldBe("22.0");
            result.Available.ShouldBe(true);
        }

        [Fact]
        public void TargetResult_Constructor_AvailableDefaultsToNull()
        {
            var result = new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0");

            result.Available.ShouldBeNull();
        }

        [Fact]
        public void TargetResult_Constructor_AvailableFalse()
        {
            var result = new TargetResult("RFC4072S", "21.0", "21.0.0.1", "21.0", false);

            result.Available.ShouldBe(false);
        }

        #endregion

        #region TargetsCommandResult

        [Fact]
        public void TargetsCommandResult_Constructor_SetsTargets()
        {
            var targets = new[]
            {
                new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0"),
                new TargetResult("RFC4072S", "21.0", "21.0.0.1", "21.0")
            };

            var result = new TargetsCommandResult(targets);

            result.Targets.ShouldBe(targets);
        }

        [Fact]
        public void TargetsCommandResult_Constructor_WithEmptyList_SetsEmptyTargets()
        {
            var result = new TargetsCommandResult(Enumerable.Empty<TargetResult>());

            result.Targets.ShouldBeEmpty();
        }

        #endregion

        #region CompilerMacroResult

        [Fact]
        public void CompilerMacroResult_Constructor_SetsNameAndValue()
        {
            var result = new CompilerMacroResult("__GNUC__", "10");

            result.Name.ShouldBe("__GNUC__");
            result.Value.ShouldBe("10");
        }

        [Fact]
        public void CompilerMacroResult_Constructor_WithNullValue_SetsNull()
        {
            var result = new CompilerMacroResult("MACRO", null);

            result.Name.ShouldBe("MACRO");
            result.Value.ShouldBeNull();
        }

        #endregion

        #region CompilerSpecificationResult

        [Fact]
        public void CompilerSpecificationResult_Constructor_SetsAllProperties()
        {
            var includePaths = new[] { new PathResult("/usr/include") };
            var macros = new[] { new CompilerMacroResult("__GNUC__", "10") };
            var targets = new[] { new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0") };

            var result = new CompilerSpecificationResult(
                "/usr/bin/arm-linux-gnueabihf-g++",
                "C++",
                "/sysroot",
                "-std=c++14",
                includePaths,
                macros,
                targets);

            result.CompilerPath.ShouldBe("/usr/bin/arm-linux-gnueabihf-g++");
            result.Language.ShouldBe("C++");
            result.CompilerSystemRoot.ShouldBe("/sysroot");
            result.CompilerFlags.ShouldBe("-std=c++14");
            result.IncludePaths.ShouldBe(includePaths);
            result.CompilerMacros.ShouldBe(macros);
            result.Targets.ShouldBe(targets);
        }

        #endregion

        #region CompilerSpecificationCommandResult

        [Fact]
        public void CompilerSpecificationCommandResult_Constructor_SetsSpecifications()
        {
            var specs = new[]
            {
                new CompilerSpecificationResult(
                    "/usr/bin/g++", "C++", "/sysroot", "-std=c++14",
                    Enumerable.Empty<PathResult>(),
                    Enumerable.Empty<CompilerMacroResult>(),
                    Enumerable.Empty<TargetResult>())
            };

            var result = new CompilerSpecificationCommandResult(specs);

            result.Specifications.ShouldBe(specs);
        }

        #endregion

        #region EntityResult

        [Fact]
        public void EntityResult_Constructor_SetsAllProperties()
        {
            var children = new[] { "ChildEntity1", "ChildEntity2" };
            var result = new EntityResult("MyProgram", "Arp.Plc.Gds", "program", children);

            result.Name.ShouldBe("MyProgram");
            result.Namespace.ShouldBe("Arp.Plc.Gds");
            result.Type.ShouldBe("program");
            result.ChildEntities.ShouldBe(children);
        }

        [Fact]
        public void EntityResult_Constructor_WithEmptyChildren_SetsEmptyCollection()
        {
            var result = new EntityResult("MyComponent", "Arp", "component", Enumerable.Empty<string>());

            result.ChildEntities.ShouldBeEmpty();
        }

        #endregion

        #region ProjectInformationCommandResult

        [Fact]
        public void ProjectInformationCommandResult_Constructor_SetsAllProperties()
        {
            var targets = new[] { new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0") };
            var entities = new[] { new EntityResult("MyProgram", "Arp", "program", Enumerable.Empty<string>()) };
            var includePaths = new[] { new UncheckedPath("/include", true) };
            var externalLibs = new[] { new PathResult("/lib/libfoo.so") };

            var result = new ProjectInformationCommandResult(
                "MyProject",
                "Arp.Plc",
                "project",
                targets,
                entities,
                includePaths,
                externalLibs,
                true,
                "/path/to/CSharp.csproj");

            result.Name.ShouldBe("MyProject");
            result.Namespace.ShouldBe("Arp.Plc");
            result.Type.ShouldBe("project");
            result.Targets.ShouldBe(targets);
            result.Entities.ShouldBe(entities);
            result.IncludePaths.ShouldBe(includePaths);
            result.ExternalLibraries.ShouldBe(externalLibs);
            result.GenerateNamespaces.ShouldBeTrue();
            result.CSharpProjectPath.ShouldBe("/path/to/CSharp.csproj");
        }

        [Fact]
        public void ProjectInformationCommandResult_Constructor_NullCSharpPath_SetsNull()
        {
            var result = new ProjectInformationCommandResult(
                "MyProject", "Arp", "project",
                Enumerable.Empty<TargetResult>(),
                Enumerable.Empty<EntityResult>(),
                Enumerable.Empty<UncheckedPath>(),
                Enumerable.Empty<PathResult>(),
                false,
                null);

            result.CSharpProjectPath.ShouldBeNull();
            result.GenerateNamespaces.ShouldBeFalse();
        }

        #endregion

        #region Path classes

        [Fact]
        public void Path_Constructor_SetsPathValue()
        {
            var path = new PathResult("/some/path");

            path.PathValue.ShouldBe("/some/path");
        }

        [Fact]
        public void UncheckedPath_Constructor_SetsExistsAndPath()
        {
            var path = new UncheckedPath("/some/path", true);

            path.PathValue.ShouldBe("/some/path");
            path.Exists.ShouldBeTrue();
        }

        [Fact]
        public void UncheckedPath_Constructor_NotExists()
        {
            var path = new UncheckedPath("/missing/path", false);

            path.Exists.ShouldBeFalse();
        }

        [Fact]
        public void IncludePath_Constructor_SetsTargetsAndPath()
        {
            var targets = new[] { new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0") };
            var path = new IncludePath("/include", true, targets);

            path.PathValue.ShouldBe("/include");
            path.Exists.ShouldBeTrue();
            path.Targets.ShouldBe(targets);
        }

        [Fact]
        public void SdkPath_Constructor_SetsTargetsAndPath()
        {
            var targets = new[] { new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0") };
            var path = new SdkPath("/sdk/path", targets);

            path.PathValue.ShouldBe("/sdk/path");
            path.Targets.ShouldBe(targets);
        }

        #endregion

        #region SdksCommandResult

        [Fact]
        public void SdksCommandResult_Constructor_SetsSdks()
        {
            var targets = new[] { new TargetResult("AXCF2152", "22.0", "22.0.0.12345", "22.0") };
            var sdks = new[] { new SdkPath("/sdk/AXCF2152_22.0", targets) };

            var result = new SdksCommandResult(sdks);

            result.Sdks.ShouldBe(sdks);
        }

        [Fact]
        public void SdksCommandResult_Constructor_WithEmptyList_SetsEmptySdks()
        {
            var result = new SdksCommandResult(Enumerable.Empty<SdkPath>());

            result.Sdks.ShouldBeEmpty();
        }

        #endregion

        #region SettingCommandResult

        [Fact]
        public void SettingCommandResult_Constructor_SetsSettingsObject()
        {
            var settings = new { key = "value" };
            var result = new SettingCommandResult(settings);

            result.Settings.ShouldBe(settings);
        }

        [Fact]
        public void SettingCommandResult_Constructor_WithStringValue_SetsString()
        {
            var result = new SettingCommandResult("some-setting-value");

            result.Settings.ShouldBe("some-setting-value");
        }

        [Fact]
        public void SettingCommandResult_Constructor_WithNull_SetsNull()
        {
            var result = new SettingCommandResult(null);

            result.Settings.ShouldBeNull();
        }

        #endregion
    }
}
