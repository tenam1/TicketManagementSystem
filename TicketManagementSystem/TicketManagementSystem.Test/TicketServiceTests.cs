using System;
using Moq;
using NUnit.Framework;

namespace TicketManagementSystem.Test
{
    public class Tests
    {
        private TicketService ticketService;
        private Mock<ITicketRepository> ticketRepositoryMock;

        [SetUp]
        public void Setup()
        {
            ticketRepositoryMock = new Mock<ITicketRepository>();
            ticketService = new TicketService(ticketRepositoryMock.Object);
        }

        [Test]
        public void shall_throw_exception_if_title_is_null()
        {
            Assert.That(() => ticketService.CreateTicket(
                    null,
                    Priority.High,
                    "jim",
                    "high prio ticket",
                    DateTime.Now,
                    false),
                Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("Title is null or empty."));
        }

        [Test]
        public void shall_throw_exception_if_assigned_to_user_is_null()
        {
            Assert.That(() => ticketService.CreateTicket(
                    "myTicket",
                    Priority.High,
                    null,
                    "high prio ticket",
                    DateTime.Now,
                    false),
                Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("AssignedToUser is null or empty."));
        }

        [Test]
        public void shall_throw_exception_if_description_is_null()
        {
            Assert.That(() => ticketService.CreateTicket(
                    "myTicket",
                    Priority.High,
                    "jim",
                    null,
                    DateTime.Now,
                    false),
                Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("Description is null or empty."));
        }


        [Test]
        public void shall_create_ticket()
        {
            const string title = "myTicket";
            const Priority prio = Priority.High;
            const string assignedTo = "jsmith";
            const string description = "This is a medium ticket";
            DateTime when = DateTime.Now;

            ticketService.CreateTicket(title, prio, assignedTo, description, when, false);

            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title &&
                t.Priority == Priority.High &&
                t.Description == description &&
                t.AssignedUser.Username == assignedTo &&
                t.Created == when)));
        }

        [Test]
        public void check_priority_changes_given_the_title()
        {
            const string title = "System Crash";
            const Priority priority = Priority.Low;
            const string assignedTo = "jsmith";
            const string description = "This is a medium ticket";
            DateTime when = DateTime.Now;

            ticketService.CreateTicket(title, priority, assignedTo, description, when, false);

            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title &&
                t.Priority == Priority.Medium &&
                t.Description == description &&
                t.AssignedUser.Username == assignedTo &&
                t.Created == when)));
        }

        [Test]
        public void check_priority_changes_given_that_enough_time_has_passed()
        {
            const string title = "System Crash";
            const Priority priority = Priority.Medium;
            const string assignedTo = "jsmith";
            const string description = "This is a medium ticket";
            DateTime when = DateTime.Now + TimeSpan.FromHours(1);

            ticketService.CreateTicket(title, priority, assignedTo, description, when, false);

            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title &&
                t.Priority == Priority.High &&
                t.Description == description &&
                t.AssignedUser.Username == assignedTo &&
                t.Created == when)));
        }

        [Test]
        public void check_price_is_zero_when_is_paying_customer_is_false()
        {
            //given
            const string title = "myTicket";
            const Priority priority = Priority.Medium;
            const string assignedTo = "jsmith";
            const string description = "This is a medium ticket";
            DateTime when = DateTime.Now;
            const bool isPayingCustomer = false;

            //when
            ticketService.CreateTicket(title, priority, assignedTo, description, when, isPayingCustomer);

            //then
            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title &&
                t.Priority == Priority.Medium &&
                t.Description == description &&
                t.AssignedUser.Username == assignedTo &&
                t.Created == when &&
                t.PriceDollars == 0)));
        }

        [Test]
        public void check_price_is_one_hundred_when_is_paying_customer_is_true_and_priority_is_high()
        {
            //given
            const string title = "my ticket";
            const Priority priority = Priority.High;
            const string assignedTo = "jsmith";
            const string description = "This is a high ticket";
            DateTime when = DateTime.Now;
            const bool isPayingCustomer = true;

            //when
            ticketService.CreateTicket(title, priority, assignedTo, description, when, isPayingCustomer);

            //then
            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title &&
                t.Priority == Priority.High &&
                t.Description == description &&
                t.AssignedUser.Username == assignedTo &&
                t.Created == when &&
                t.PriceDollars == 100)));
        }

        [Test]
        public void check_price_is_fifty_when_is_paying_customer_is_true_and_priority_is_not_high()
        {
            //given
            const string title = "myTicket";
            const Priority priority = Priority.Medium;
            const string assignedTo = "jsmith";
            const string description = "This is a medium ticket";
            DateTime when = DateTime.Now;
            const bool isPayingCustomer = true;

            //when
            ticketService.CreateTicket(title, priority, assignedTo, description, when, isPayingCustomer);

            //then
            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title &&
                t.Priority == Priority.Medium &&
                t.Description == description &&
                t.AssignedUser.Username == assignedTo &&
                t.Created == when &&
                t.PriceDollars == 50)));
        }
    }
}