using System.Collections.Generic;
using System.Linq;
using System;

using NUnit.Framework;
using MMABooksEFClasses.MarisModels;
using Microsoft.EntityFrameworkCore;

namespace MMABooksTests
{
    [TestFixture]
    public class ProductTests
    {
        MMABooksContext dbContext;
        Products? p;
        List<Products>? products;

        [SetUp]
        public void Setup()
        {
            dbContext = new MMABooksContext();
            dbContext.Database.ExecuteSqlRaw("call usp_testingResetData()");
        }

        // Query 1
        [Test]
        public void GetAllTest()
        {
            products = dbContext.Products.OrderBy(p => p.ProductCode).ToList();
            Assert.AreEqual(16, products.Count);
            Assert.AreEqual("A4CS", products[0].ProductCode);
            PrintAll(products);
        }

        // Query 2
        [Test]
        public void GetByPrimaryKeyTest()
        {
            p = dbContext.Products.Find("A4CS");
            Assert.IsNotNull(p);
            Assert.AreEqual("Murach's ASP.NET 4 Web Programming with C# 2010", p.Description);
            Console.WriteLine(p);
        }

        // Query 3
        [Test]
        public void GetUsingWhere()
        {
            // get a list of all of the products that have a unit price of 56.50
            products = dbContext.Products.Where(p => p.UnitPrice == 56.50m).OrderBy(p => p.ProductCode).ToList();
            Assert.AreEqual(7, products.Count);
            Assert.AreEqual("A4CS", products[0].ProductCode);
            PrintAll(products);
        }

        // Query 4
        [Test]
        public void GetWithCalculatedFieldTest()
        {
            // get a list of objects that include the productcode, unitprice, quantity and inventoryvalue
            var products = dbContext.Products.Select(
            p => new { p.ProductCode, p.UnitPrice, p.OnHandQuantity, Value = p.UnitPrice * p.OnHandQuantity }).
            OrderBy(p => p.ProductCode).ToList();
            Assert.AreEqual(16, products.Count);
            foreach (var p in products)
            {
                Console.WriteLine(p);
            }
        }

        // Query 5
        [Test]
        public void DeleteTest()
        {
            // Pick a product that is NOT on any invoice, or the delete is blocked by the foreign key.
            // Find one with:  SELECT ProductCode FROM products
            //                 WHERE ProductCode NOT IN (SELECT ProductCode FROM invoicelineitems);
            p = dbContext.Products.Find("ZJLR");
            Assert.IsNotNull(p);
            dbContext.Products.Remove(p);
            dbContext.SaveChanges();
            Assert.IsNull(dbContext.Products.Find("ZJLR"));
        }

        // Query 6
        [Test]
        public void CreateTest()
        {
            p = new Products();
            p.ProductCode = "AAAA";
            p.Description = "Test Product";
            p.UnitPrice = 9.99m;
            p.OnHandQuantity = 10;
            dbContext.Products.Add(p);
            dbContext.SaveChanges();
            Assert.IsNotNull(dbContext.Products.Find("AAAA"));
        }

        // Query 7
        [Test]
        public void UpdateTest()
        {
            p = dbContext.Products.Find("A4CS");
            Assert.IsNotNull(p);
            p.Description = "Updated Description";
            p.UnitPrice = 60.00m;
            dbContext.Products.Update(p);
            dbContext.SaveChanges();

            // use a fresh context so we know the change really went to the database
            MMABooksContext freshContext = new MMABooksContext();
            Products? updated = freshContext.Products.Find("A4CS");
            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Description", updated.Description);
            Assert.AreEqual(60.00m, updated.UnitPrice);
            Console.WriteLine(updated);
        }

        public void PrintAll(List<Products> products)
        {
            foreach (Products p in products)
            {
                Console.WriteLine(p);
            }
        }
    }
}
