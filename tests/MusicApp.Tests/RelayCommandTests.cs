using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Common;

namespace MusicApp.Tests
{
    [TestClass]
    public class RelayCommandTests
    {
        [TestMethod]
        public void CanExecute_WithPredicateTrue_ReturnsTrue()
        {
            var command = new RelayCommand(_ => { }, _ => true);
            bool canExecute = command.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void CanExecute_WithPredicateFalse_ReturnsFalse()
        {
            var command = new RelayCommand(_ => { }, _ => false);
            bool canExecute = command.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        public void CanExecute_WithNullPredicate_ReturnsTrueByDefault()
        {
            var command = new RelayCommand(_ => { }, null);
            bool canExecute = command.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void Execute_InvokesActionDelegate()
        {
            bool executed = false;
            var command = new RelayCommand(_ => executed = true);

            command.Execute(null);

            Assert.IsTrue(executed);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullExecute_ThrowsArgumentNullException()
        {
            new RelayCommand(null);
        }
    }
}

