using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Core.Common;

namespace MusicApp.Tests
{
    /// <summary>
    /// Bo kiem thu don vi cho lop co so RelayCommand (ICommand implementation).
    /// 
    /// - Tac dung: Kiem thu hanh vi cua mau lenh RelayCommand dung xuyen suot trong kien truc MVVM
    ///   cua du an de binding hanh dong nguoi dung tu XAML xuong ViewModel.
    /// 
    /// - Van de giai quyet:
    ///   1. Kiem thu CanExecute: Xac minh dieu kien thuc thi tra ve True khi predicate thoa man,
    ///      tra ve False khi khong thoa man, va mac dinh luon True khi predicate la null.
    ///   2. Kiem thu Execute: Dam bao delegate Action duoc kich hoat chinh xac khi Command duoc goi.
    ///   3. Kiem thu Constructor phong thu: Nem ArgumentNullException ngay khi delegate execute bi truyen null.
    /// 
    /// - Cach thuc van hanh:
    ///   Khoi tao RelayCommand voi cac bieu thuc lambda kiem thu va kiem tra ket qua CanExecute/Execute.
    /// </summary>
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

