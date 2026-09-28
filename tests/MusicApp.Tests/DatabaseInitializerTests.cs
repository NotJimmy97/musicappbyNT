using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Persistence;

namespace MusicApp.Tests
{
    [TestClass]
    public class DatabaseInitializerTests
    {
        private string _tempDbFile;

        [TestInitialize]
        public void Setup()
        {
            _tempDbFile = Path.Combine(Path.GetTempPath(), $"db_init_test_{Guid.NewGuid():N}.db");
            string connStr = $"Data Source={_tempDbFile};Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;Default Timeout=5;";
            DatabaseInitializer.SetCustomConnectionString(connStr);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DatabaseInitializer.ResetInitialization();
            if (File.Exists(_tempDbFile))
            {
                try
                {
                    File.Delete(_tempDbFile);
                }
                catch { }
            }
        }

        [TestMethod]
        public void Initialize_CreatesDatabaseFileAndTables()
        {
            // Act
            DatabaseInitializer.Initialize();

            // Assert
            Assert.IsTrue(File.Exists(_tempDbFile), "Database file should be created on disk.");
        }

        [TestMethod]
        public void Initialize_Idempotent_CanRunMultipleTimesWithoutError()
        {
            // Act & Assert
            DatabaseInitializer.Initialize();
            DatabaseInitializer.Initialize(); // Second call should be safe
            Assert.IsTrue(File.Exists(_tempDbFile));
        }
    }
}
