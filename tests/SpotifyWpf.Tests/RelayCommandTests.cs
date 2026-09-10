using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpotifyWpf.Core.Common;

namespace SpotifyWpf.Tests
{
    [TestClass]
    public class RelayCommandTests
    {
        [TestMethod]
        public void CanExecute_WithPredicateTrue_ReturnsTrue()
        {
            // Arrange
            var command = new RelayCommand(
                execute: _ => { },
                canExecute: _ => true
            );

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WithPredicateFalse_ReturnsFalse()
        {
            // Arrange
            var command = new RelayCommand(
                execute: _ => { },
                canExecute: _ => false
            );

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanExecute_WithNullPredicate_ReturnsTrueByDefault()
        {
            // Arrange: Null predicate represents unconditionally executable command
            var command = new RelayCommand(
                execute: _ => { },
                canExecute: null
            );

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Execute_InvokesActionDelegate()
        {
            // Arrange
            bool executed = false;
            var command = new RelayCommand(
                execute: _ => executed = true,
                canExecute: null
            );

            // Act
            command.Execute(null);

            // Assert
            Assert.IsTrue(executed);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullExecute_ThrowsArgumentNullException()
        {
            // Act: Must reject null execute delegate to enforce defensive boundary
            var command = new RelayCommand((Action<object>)null);
            Assert.IsNotNull(command);
        }
    }
}
