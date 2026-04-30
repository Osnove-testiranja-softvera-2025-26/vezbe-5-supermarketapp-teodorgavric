using NUnit.Framework;
using OTS_Supermarket.Models;
using OTS_Supermarket;
using System;
using System.Linq;

namespace OTS_Supermarket.Test
{
    [TestFixture]
    public class CartTest
    {
        // Helper: Finds a future date string in yyyy-MM-dd format within the provided inclusive range of days
        private string FindFutureDateString(int minDaysInclusive, int maxDaysInclusive, bool requireWeekday)
        {
            DateTime today = DateTime.Today;
            for (int d = minDaysInclusive; d <= maxDaysInclusive; d++)
            {
                DateTime candidate = today.AddDays(d);
                bool isWeekday = candidate.DayOfWeek != DayOfWeek.Saturday && candidate.DayOfWeek != DayOfWeek.Sunday;
                if (!requireWeekday || isWeekday)
                {
                    return candidate.ToString("yyyy-MM-dd");
                }
            }
            throw new InvalidOperationException("No suitable future date found in the given range.");
        }

        [Test]
        public void AddOneToCart_ShouldAddItemToCart_Success()
        {
            // ARRANGE
            Cart cart = new Cart();
            Monitor monitor = new Monitor();

            // ACT
            cart.AddOneToCart(monitor);

            // ASSERT
            Assert.That(cart.Size, Is.EqualTo(1));
            Assert.That(cart.Amount, Is.EqualTo(100));
        }

        [Test]
        public void AddOneToCart_ShouldIncrementCorrectCounter_Success()
        {
            // ARRANGE
            Cart cart = new Cart();
            Keyboard keyboard = new Keyboard();

            // ACT
            cart.AddOneToCart(keyboard);

            // ASSERT
            Assert.That(cart.Keyboard_counter, Is.EqualTo(1));
            Assert.That(cart.Monitor_counter, Is.EqualTo(0));
            Assert.That(cart.Laptop_counter, Is.EqualTo(0));
            Assert.That(cart.Computer_counter, Is.EqualTo(0));
            Assert.That(cart.Chair_counter, Is.EqualTo(0));
        }

        [Test]
        public void AddOneToCart_WhenCartHasTenItems_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();
            Monitor monitor = new Monitor();
            // fill to 10
            for (int i = 0; i < 10; i++) cart.AddOneToCart(monitor);

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.AddOneToCart(monitor));
            Assert.That(ex.Message, Is.EqualTo("Number of items in cart must be 10 or less!"));
        }

        [Test]
        public void AddMultipleToCart_ShouldAddMultipleItems_UpdateSizeAndAmount()
        {
            // ARRANGE
            Cart cart = new Cart();
            Monitor monitor = new Monitor();

            // ACT
            cart.AddMultipleToCart(monitor, 3);

            // ASSERT
            Assert.That(cart.Size, Is.EqualTo(3));
            Assert.That(cart.Amount, Is.EqualTo(300));
            Assert.That(cart.Monitor_counter, Is.EqualTo(3));
        }

        [Test]
        public void AddMultipleToCart_WhenExceedsTen_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();
            Monitor monitor = new Monitor();
            cart.AddMultipleToCart(monitor, 8);

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.AddMultipleToCart(monitor, 3));
            Assert.That(ex.Message, Is.EqualTo("Number of items in cart must be 10 or less!"));
        }

        [Test]
        public void DeleteAll_ShouldClearCartAndResetCounters_Success()
        {
            // ARRANGE
            Cart cart = new Cart();
            cart.AddOneToCart(new Monitor());
            cart.AddOneToCart(new Computer());
            cart.AddOneToCart(new Keyboard());
            Assert.That(cart.Size, Is.GreaterThan(0));

            // ACT
            cart.DeleteAll();

            // ASSERT
            Assert.That(cart.Size, Is.EqualTo(0));
            Assert.That(cart.Monitor_counter, Is.EqualTo(0));
            Assert.That(cart.Keyboard_counter, Is.EqualTo(0));
            Assert.That(cart.Laptop_counter, Is.EqualTo(0));
            Assert.That(cart.Computer_counter, Is.EqualTo(0));
            Assert.That(cart.Chair_counter, Is.EqualTo(0));
        }

        [Test]
        public void DeleteAll_WhenCartEmpty_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.DeleteAll());
            Assert.That(ex.Message, Is.EqualTo("Cannot restore empty cart!"));
        }

        [Test]
        public void Print_WhenCartHasItems_ShouldReturnNonEmptyString()
        {
            // ARRANGE
            Cart cart = new Cart();
            cart.AddOneToCart(new Monitor());

            // ACT
            string result = cart.Print();

            // ASSERT
            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result, Does.Contain("Item: Monitor"));
        }

        [Test]
        public void Print_WhenCartEmpty_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.Print());
            Assert.That(ex.Message, Is.EqualTo("Cannot print empty cart!"));
        }

        [Test]
        public void Calculate_InvalidDateFormat_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();
            string badDate = "01-01-2025";

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.Calculate(badDate));
            Assert.That(ex.Message, Is.EqualTo("Wrong date format! Date must be in format yyyy-MM-dd"));
        }

        [Test]
        public void Calculate_DateIsToday_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();
            string today = DateTime.Today.ToString("yyyy-MM-dd");

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.Calculate(today));
            Assert.That(ex.Message, Is.EqualTo("Date of delivery can't be today's date!"));
        }

        [Test]
        public void Calculate_DateMoreThanSevenDays_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();
            string future = DateTime.Today.AddDays(8).ToString("yyyy-MM-dd");

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.Calculate(future));
            Assert.That(ex.Message, Is.EqualTo("Days for delivery must be less than 7!"));
        }

        [Test]
        public void Calculate_WhenFinalPriceExceedsBudget_ShouldThrowException()
        {
            // ARRANGE
            Cart cart = new Cart();
            cart.AddOneToCart(new Computer()); // 1200
            cart.Budget = 100; // too small
            string date = FindFutureDateString(1, 3, true);

            // ACT & ASSERT
            var ex = Assert.Throws<Exception>(() => cart.Calculate(date));
            Assert.That(ex.Message, Is.EqualTo("Not enough budget!"));
        }

        [Test]
        public void Calculate_ShouldApply10PercentDiscount_WhenConditionsMet()
        {
            // ARRANGE
            Cart cart = new Cart();
            // 3 computers (1200 each) + 6 monitors (100 each) = 9 items
            cart.AddMultipleToCart(new Computer(), 3);
            cart.AddMultipleToCart(new Monitor(), 6);
            cart.Budget = 10000;
            string date = FindFutureDateString(1, 3, true); // weekday within 1..3 days

            double initialBudget = cart.Budget;
            double expectedAmount = cart.Amount;
            double expectedPrice = expectedAmount - (expectedAmount * 0.10);

            // ACT
            cart.Calculate(date);

            // ASSERT
            Assert.That(cart.Budget, Is.EqualTo(initialBudget - expectedPrice));
        }

        [Test]
        public void Calculate_ShouldApply8PercentDiscount_WhenConditionsMet()
        {
            // ARRANGE
            Cart cart = new Cart();
            // Size > 8, laptop between 1 and 7, ensure monitor < 3 and computer < 3 so 10% doesn't apply
            cart.AddOneToCart(new Laptop()); // 1 laptop
            cart.AddMultipleToCart(new Keyboard(), 8); // 8 keyboards -> total size 9
            cart.Budget = 10000;
            string date = FindFutureDateString(1, 3, true);

            double initialBudget = cart.Budget;
            double expectedAmount = cart.Amount;
            double expectedPrice = expectedAmount - (expectedAmount * 0.08);

            // ACT
            cart.Calculate(date);

            // ASSERT
            Assert.That(cart.Budget, Is.EqualTo(initialBudget - expectedPrice));
        }

        [Test]
        public void Calculate_ShouldApply5PercentDiscount_WhenHasLaptopComputerChair()
        {
            // ARRANGE
            Cart cart = new Cart();
            // Size > 5, include laptop, computer and chair
            cart.AddOneToCart(new Laptop());
            cart.AddOneToCart(new Computer());
            cart.AddOneToCart(new Chair());
            cart.AddMultipleToCart(new Monitor(), 3);
            cart.Budget = 10000;
            string date = FindFutureDateString(1, 3, true);

            double initialBudget = cart.Budget;
            double expectedAmount = cart.Amount;
            double expectedPrice = expectedAmount - (expectedAmount * 0.05);

            // ACT
            cart.Calculate(date);

            // ASSERT
            Assert.That(cart.Budget, Is.EqualTo(initialBudget - expectedPrice));
        }

        [Test]
        public void Calculate_ShouldApply5PercentDiscount_WhenSizeBetween6And7AndAmountOver1200()
        {
            // ARRANGE
            Cart cart = new Cart();
            // 1 computer (1200) + 5 keyboards (50*5=250) => amount 1450, size 6
            cart.AddOneToCart(new Computer());
            cart.AddMultipleToCart(new Keyboard(), 5);
            cart.Budget = 10000;
            string date = FindFutureDateString(1, 3, true);

            double initialBudget = cart.Budget;
            double expectedAmount = cart.Amount;
            double expectedPrice = expectedAmount - (expectedAmount * 0.05);

            // ACT
            cart.Calculate(date);

            // ASSERT
            Assert.That(cart.Size, Is.GreaterThan(5).And.LessThan(8));
            Assert.That(cart.Amount, Is.GreaterThan(1200));
            Assert.That(cart.Budget, Is.EqualTo(initialBudget - expectedPrice));
        }

        [Test]
        public void Calculate_ShouldApply20PercentDiscount_WhenConditionsMet()
        {
            // ARRANGE
            Cart cart = new Cart();
            // 3 computers + 6 monitors = 9 items, amount > 1500
            cart.AddMultipleToCart(new Computer(), 3);
            cart.AddMultipleToCart(new Monitor(), 6);
            cart.Budget = 20000;
            string date = FindFutureDateString(4, 7, true);

            double initialBudget = cart.Budget;
            double expectedAmount = cart.Amount;
            double expectedPrice = expectedAmount - (expectedAmount * 0.20);

            // ACT
            cart.Calculate(date);

            // ASSERT
            Assert.That(cart.Budget, Is.EqualTo(initialBudget - expectedPrice));
        }

        [Test]
        public void Calculate_ShouldApply18PercentDiscount_WhenConditionsMet()
        {
            // ARRANGE
            Cart cart = new Cart();
            // Size > 5, amount > 1200, include a chair
            cart.AddMultipleToCart(new Monitor(), 3);
            cart.AddOneToCart(new Computer());
            cart.AddOneToCart(new Chair());
            cart.AddOneToCart(new Keyboard());
            // Now size = 6, amount > 1200
            cart.Budget = 10000;
            string date = FindFutureDateString(4, 7, true);

            double initialBudget = cart.Budget;
            double expectedAmount = cart.Amount;
            double expectedPrice = expectedAmount - (expectedAmount * 0.18);

            // ACT
            cart.Calculate(date);

            // ASSERT
            Assert.That(cart.Budget, Is.EqualTo(initialBudget - expectedPrice));
        }
    }
}
