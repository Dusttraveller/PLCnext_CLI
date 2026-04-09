#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using PlcNext.Migration;
using Shouldly;
using Xunit;

namespace Test.PlcNext.UnitTests
{
    public class MigrationChainTests
    {
        [Fact]
        public void Start_ReturnsMigrationChainInstance()
        {
            var chain = MigrationChain.Start(_ => { });

            chain.ShouldNotBeNull();
        }

        [Fact]
        public void AddConversionStep_ReturnsSameChain()
        {
            var chain = MigrationChain.Start(_ => { });

            var result = chain.AddConversionStep<StubConversionStep>();

            result.ShouldBeSameAs(chain);
        }

        [Fact]
        public void AddPotentialLocation_ReturnsSameChain()
        {
            var chain = MigrationChain.Start(_ => { });

            var result = chain.AddPotentialLocation("/some/path", new Version(19, 0));

            result.ShouldBeSameAs(chain);
        }

        [Fact]
        public void AddMigrationFile_ReturnsSameChain()
        {
            var chain = MigrationChain.Start(_ => { });

            var result = chain.AddMigrationFile("settings.json");

            result.ShouldBeSameAs(chain);
        }

        [Fact]
        public void SetMigrationDestination_ReturnsSameChain()
        {
            var chain = MigrationChain.Start(_ => { });

            var result = chain.SetMigrationDestination("/new/location");

            result.ShouldBeSameAs(chain);
        }

        [Fact]
        public void Execute_NoKnownLocations_ReturnsTrueAndWritesMessages()
        {
            var messages = new List<string>();
            var chain = MigrationChain.Start(msg => messages.Add(msg));

            bool result = chain.Execute();

            result.ShouldBeTrue();
            messages.Count.ShouldBeGreaterThanOrEqualTo(2);
            messages[0].ShouldContain("migrating");
            messages[messages.Count - 1].ShouldContain("finished");
        }

        [Fact]
        public void Execute_NoKnownLocations_WritesStartAndFinishMessages()
        {
            var messages = new List<string>();
            var chain = MigrationChain.Start(msg => messages.Add(msg));

            chain.Execute();

            messages.ShouldContain(m => m.Contains("Start migrating"));
            messages.ShouldContain(m => m.Contains("Migration finished successfully"));
        }

        [Fact]
        public void Execute_KnownLocationDoesNotExist_ReturnsTrueWithoutMigrating()
        {
            var messages = new List<string>();
            var chain = MigrationChain.Start(msg => messages.Add(msg))
                                      .AddPotentialLocation(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString()), new Version(19, 0))
                                      .AddMigrationFile("settings.json");

            bool result = chain.Execute();

            result.ShouldBeTrue();
        }

        [Fact]
        public void Execute_WithExistingLocationAndFile_CopiesFileAndReturnsTrue()
        {
            var sourceDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            var destDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            const string fileName = "migration_test.json";

            try
            {
                Directory.CreateDirectory(sourceDir);
                File.WriteAllText(System.IO.Path.Combine(sourceDir, fileName), "{}");

                var messages = new List<string>();
                var chain = MigrationChain.Start(msg => messages.Add(msg))
                                          .AddPotentialLocation(sourceDir, new Version(19, 0))
                                          .AddMigrationFile(fileName)
                                          .SetMigrationDestination(destDir);

                bool result = chain.Execute();

                result.ShouldBeTrue();
                File.Exists(System.IO.Path.Combine(destDir, fileName)).ShouldBeTrue();
                messages.ShouldContain(m => m.Contains("Migration finished successfully"));
            }
            finally
            {
                if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            }
        }

        [Fact]
        public void Execute_WithConversionStepAfterMigration_ExecutesStep()
        {
            var sourceDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            var destDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            const string fileName = "settings.json";

            try
            {
                Directory.CreateDirectory(sourceDir);
                File.WriteAllText(System.IO.Path.Combine(sourceDir, fileName), "{}");

                bool stepExecuted = false;
                TrackingConversionStep.OnExecute = _ => stepExecuted = true;

                var chain = MigrationChain.Start(_ => { })
                                          .AddConversionStep<TrackingConversionStep>()
                                          .AddPotentialLocation(sourceDir, new Version(19, 0))
                                          .AddMigrationFile(fileName)
                                          .SetMigrationDestination(destDir);

                chain.Execute();

                stepExecuted.ShouldBeTrue();
            }
            finally
            {
                TrackingConversionStep.OnExecute = null;
                if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            }
        }

        [Fact]
        public void Execute_WhenConversionStepThrows_ReturnsFalseAndWritesError()
        {
            var sourceDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            var destDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            const string fileName = "settings.json";

            try
            {
                Directory.CreateDirectory(sourceDir);
                File.WriteAllText(System.IO.Path.Combine(sourceDir, fileName), "{}");

                ThrowingConversionStep.ErrorMessage = "step failed";

                var messages = new List<string>();
                var chain = MigrationChain.Start(msg => messages.Add(msg))
                                          .AddConversionStep<ThrowingConversionStep>()
                                          .AddPotentialLocation(sourceDir, new Version(19, 0))
                                          .AddMigrationFile(fileName)
                                          .SetMigrationDestination(destDir);

                bool result = chain.Execute();

                result.ShouldBeFalse();
                messages.ShouldContain(m => m.Contains("not successful"));
            }
            finally
            {
                if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            }
        }

        [Fact]
        public void Execute_MultipleLocations_UsesHighestVersion()
        {
            var lowerDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            var higherDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            var destDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            const string fileName = "settings.json";

            try
            {
                Directory.CreateDirectory(lowerDir);
                Directory.CreateDirectory(higherDir);
                File.WriteAllText(System.IO.Path.Combine(lowerDir, fileName), "lower");
                File.WriteAllText(System.IO.Path.Combine(higherDir, fileName), "higher");

                var chain = MigrationChain.Start(_ => { })
                                          .AddPotentialLocation(lowerDir, new Version(19, 0))
                                          .AddPotentialLocation(higherDir, new Version(21, 0))
                                          .AddMigrationFile(fileName)
                                          .SetMigrationDestination(destDir);

                chain.Execute();

                string copiedContent = File.ReadAllText(System.IO.Path.Combine(destDir, fileName));
                copiedContent.ShouldBe("higher");
            }
            finally
            {
                if (Directory.Exists(lowerDir)) Directory.Delete(lowerDir, true);
                if (Directory.Exists(higherDir)) Directory.Delete(higherDir, true);
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            }
        }

        // Helper stubs

        private class StubConversionStep : IConversionStep
        {
            public Version BaseVersion => new Version(19, 0);
            public void Execute(string migrationDestination) { }
        }

        private class TrackingConversionStep : IConversionStep
        {
            public static Action<string> OnExecute { get; set; }
            public Version BaseVersion => new Version(19, 0);
            public void Execute(string migrationDestination) => OnExecute?.Invoke(migrationDestination);
        }

        private class ThrowingConversionStep : IConversionStep
        {
            public static string ErrorMessage { get; set; } = "error";
            public Version BaseVersion => new Version(19, 0);
            public void Execute(string migrationDestination) => throw new InvalidOperationException(ErrorMessage);
        }
    }
}
