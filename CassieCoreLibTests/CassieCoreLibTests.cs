using System;
using System.IO;
using CassieCoreLib;
using Xunit;

namespace CassieCoreLibTests
{
    public class CassieCoreLibTests
    {
        [Fact]
        public void verify_destination_database()
        {
            var destinationDb = new DestinationDatabase("127.0.0.1");
            Assert.True(destinationDb.VerifyConnection());
        }

        [Fact]
        public void verify_destination_apollo_database()
        {
            // TODO: Add assumption of DB name & such to README.md.
            var apolloDb = new ApolloDb("adron", "superSecret!", "secrets.zip", "testks");
            var destinationDb = new DestinationDatabase(apolloDb);
            Assert.True(destinationDb.VerifyConnection());
        }

        [Fact]
        public void verify_migration_execution()
        {
            var path = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());

            var filesToProcess = new FileSelection(path).GetFiles();
            var migrationsToExecute = new MigrationPath(filesToProcess);

            var upResult = migrationsToExecute.Migrate(Migration.Up);
            Assert.True(upResult);

            var downResult = migrationsToExecute.Migrate(Migration.Down);
            Assert.True(downResult);
            
            TestHelpers.TearDownMigrationsTested(path);
        }

        [Fact]
        public void verify_tasks_are_for_down_migration_when_down_migration_called()
        {
            var path = GetMigrationsToExecute(out var migrationsToExecute);

            foreach(var migration in migrationsToExecute.Path(Migration.Down))
            {
                Assert.Equal(Migration.Down, migration.MigrationType);
            }

            TestHelpers.TearDownMigrationsTested(path);
        }

        [Fact]
        public void verify_tasks_are_for_up_migration_when_up_migration_called()
        {
            var path = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());

            var filesToProcess = new FileSelection(path).GetFiles();
            var migrationsToExecute = new MigrationPath(filesToProcess);
            
            foreach(var migration in migrationsToExecute.Path(Migration.Up))
            {
                Assert.Equal(Migration.Up, migration.MigrationType);
            }
            
            TestHelpers.TearDownMigrationsTested(path);
        }
        
        [Fact]
        public void verify_tasks_are_ordered_for_up_migration()
        {
            var path = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());

            var filesToProcess = new FileSelection(path).GetFiles();
            var migrationsToExecute = new MigrationPath(filesToProcess);
            var migrateUpMigrations = migrationsToExecute.Path(Migration.Up);
            
            Assert.Equal("01 05 2001.doing-whatever.up.cql", migrateUpMigrations[0].TaskFile.Name);
            Assert.Equal("01 01 2003.doing-stuff.up.cql", migrateUpMigrations[1].TaskFile.Name);
            Assert.Equal("01 08 2006.doing-something.up.cql", migrateUpMigrations[2].TaskFile.Name);
            Assert.Equal("march 01 2009.doing-other-bits.up.cql", migrateUpMigrations[3].TaskFile.Name);
            Assert.Equal("01 02 2010.doing-more-stuff.up.cql", migrateUpMigrations[4].TaskFile.Name);
            
            TestHelpers.TearDownMigrationsTested(path);
        }
        
        [Fact]
        public void verify_tasks_are_ordered_for_down_migration()
        {
            var path = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());

            var filesToProcess = new FileSelection(path).GetFiles();
            var migrationsToExecute = new MigrationPath(filesToProcess);
            var migrateUpMigrations = migrationsToExecute.Path(Migration.Down);

            foreach (var migration in migrateUpMigrations)
            {
                Assert.Equal(Migration.Down, migration.MigrationType);
            }

            TestHelpers.TearDownMigrationsTested(path);
        }

        [Fact]
        public void verify_tasks_are_ready_for_migration_execution()
        {
            var migrationPath = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());

            var filesToProcess = new FileSelection(migrationPath).GetFiles();
            var migrationsToExecute = new MigrationPath(filesToProcess);
            
            Assert.True(migrationsToExecute.Ready());

            TestHelpers.TearDownMigrationsTested(migrationPath);
        }

        [Fact]
        public void verify_correct_number_tasks_derived_from_files()
        {
            var migrationPath = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());
            var result = new FileSelection(migrationPath).GetFiles();
            Assert.Equal(9, result.Count);
            TestHelpers.TearDownMigrationsTested(migrationPath);
        }

        [Fact]
        public void verify_migration_files_do_not_exist()
        {
            var migrationPath = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());
            Assert.NotEmpty(migrationPath);
            TestHelpers.TearDownMigrationsTested(migrationPath);
        }

        [Fact]
        public void verify_migration_files_exist()
        {
            var migrationPath = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());
            var fileSystem = new FileSelection(migrationPath);
            Assert.True(fileSystem.ProspectiveMigrationsExist());
            TestHelpers.TearDownMigrationsTested(migrationPath);
        }

        [Fact]
        public void verify_migration_path_list()
        {
            var migrationPath = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());
            var fileSystem = new FileSelection(migrationPath);
            var result = fileSystem.ShowMigrationPath();
            Assert.True(result.Ready());
            TestHelpers.TearDownMigrationsTested(migrationPath);
        }

        [Fact]
        public void verify_path_avialable()
        {
            var fileSystem = new FileSelection();
            Assert.Equal(fileSystem.OperationsPath(), Directory.GetCurrentDirectory());
        }

        [Fact]
        public void verify_selected_path_available()
        {
            // Read path from config, if not available, default to path of current execution call.
            var configurationPath = "/this/does/not/exist";
            var fileSelection = new FileSelection(configurationPath);
            Assert.Equal(fileSelection.OperationsPath(), Directory.GetCurrentDirectory());
        }

        [Fact]
        public void verify_selected_path_available_and_exists()
        {
            var newPath = "newpath";
            Directory.CreateDirectory(newPath);
            var fileSelection = new FileSelection(newPath);
            Assert.Equal(fileSelection.OperationsPath(), newPath);
            Directory.Delete(newPath);
        }
        
        private static string GetMigrationsToExecute(out MigrationPath migrationsToExecute)
        {
            var path = TestHelpers.SetupMigrationsForTests(true, Guid.NewGuid());
            var filesToProcess = new FileSelection(path).GetFiles();
            migrationsToExecute = new MigrationPath(filesToProcess);
            return path;
        }
    }
}