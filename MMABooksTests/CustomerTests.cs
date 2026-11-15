using System.Collections.Generic;
using System.Linq;
using System;

using NUnit.Framework;
using MMABooksEFClasses.MarisModels;
using Microsoft.EntityFrameworkCore;

namespace MMABooksTests
{
    [TestFixture]
    public class CustomerTests
    {
        MMABooksContext dbContext;
        Customer? c;
        List<Customer>? customers;

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
            customers = dbContext.Customers.OrderBy(c => c.Name).ToList();
            Assert.AreEqual(696, customers.Count);
            Assert.AreEqual("Abeyatunge, Derek", customers[0].Name);
            PrintAll(customers);
        }

        // Query 2
        [Test]
        public void GetByPrimaryKeyTest()
        {
            c = dbContext.Customers.Find(1);
            Assert.IsNotNull(c);
            Assert.AreEqual("Molunguri, A", c.Name);
            Console.WriteLine(c);
        }

        // Query 3
        [Test]
        public void GetUsingWhere()
        {
            // get a list of all of the customers who live in OR
            customers = dbContext.Customers.Where(c => c.StateCode == "OR").OrderBy(c => c.Name).ToList();
            Assert.AreEqual(5, customers.Count);
            Assert.AreEqual("Erpenbach, Lee", customers[0].Name);
            PrintAll(customers);
        }

        // Query 4
        [Test]
        public void GetWithInvoicesTest()
        {
            // get the customer whose id is 20 and all of the invoices for that customer
            c = dbContext.Customers.Include("Invoices").Where(c => c.CustomerId == 20).SingleOrDefault();
            Assert.IsNotNull(c);
            Assert.AreEqual("Chamberland, Sarah", c.Name);
            // CHECK THIS NUMBER in MySQL Workbench:  SELECT COUNT(*) FROM invoices WHERE CustomerID = 20;
            Assert.AreEqual(3, c.Invoices.Count);
            Console.WriteLine(c);
        }

        // Query 5
        [Test]
        public void GetWithJoinTest()
        {
            // get a list of objects that include the customer id, name, statecode and statename
            var customers = dbContext.Customers.Join(
               dbContext.States,
               c => c.StateCode,
               s => s.StateCode,
               (c, s) => new { c.CustomerId, c.Name, c.StateCode, s.StateName }).OrderBy(r => r.StateName).ToList();
            Assert.AreEqual(696, customers.Count);
            // I wouldn't normally print here but this lets you see what each object looks like
            foreach (var c in customers)
            {
                Console.WriteLine(c);
            }
        }

        // Query 6
        [Test]
        public void DeleteTest()
        {
            // Pick a customer that has NO invoices, or the delete is blocked by the foreign key.
            // Find one with:  SELECT CustomerID FROM customers
            //                 WHERE CustomerID NOT IN (SELECT CustomerID FROM invoices) LIMIT 5;
            c = dbContext.Customers.Find(1);
            Assert.IsNotNull(c);
            dbContext.Customers.Remove(c);
            dbContext.SaveChanges();
            Assert.IsNull(dbContext.Customers.Find(1));
        }

        // Query 7
        [Test]
        public void CreateTest()
        {
            c = new Customer();
            c.Name = "Doe, John";
            c.Address = "123 Main St";
            c.City = "Klamath Falls";
            c.StateCode = "OR";     // must be a real state code (foreign key)
            c.ZipCode = "97601";
            dbContext.Customers.Add(c);
            dbContext.SaveChanges();

            // CustomerID is auto-increment, so EF fills it in after SaveChanges
            Customer? found = dbContext.Customers.Find(c.CustomerId);
            Assert.IsNotNull(found);
            Assert.AreEqual("Doe, John", found.Name);
            Console.WriteLine(found);
        }

        // Query 8
        [Test]
        public void UpdateTest()
        {
            c = dbContext.Customers.Find(1);
            Assert.IsNotNull(c);
            c.Name = "Molunguri, Aaron";
            dbContext.Customers.Update(c);
            dbContext.SaveChanges();

            // use a fresh context so we know the change really went to the database
            MMABooksContext freshContext = new MMABooksContext();
            Customer? updated = freshContext.Customers.Find(1);
            Assert.IsNotNull(updated);
            Assert.AreEqual("Molunguri, Aaron", updated.Name);
            Console.WriteLine(updated);
        }

        public void PrintAll(List<Customer> customers)
        {
            foreach (Customer c in customers)
            {
                Console.WriteLine(c);
            }
        }
    }
}
