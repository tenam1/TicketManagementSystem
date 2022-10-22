using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TicketManagementSystem
{
    public class TicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public int CreateTicket(string title, Priority priority, string assignedToUser, string description,
            DateTime createdOn, bool isPayingCustomer)

        {
            ValidationCheck(title, assignedToUser, description);

            Ticket ticket = new Ticket()
            {
                Title = title,
                AssignedUser = SetAssignedUser(assignedToUser),
                Priority = SetPriority(title, priority, createdOn),
                Description = description,
                Created = createdOn,
                PriceDollars = SetPrice(isPayingCustomer, SetPriority(title, priority, createdOn)),
                AccountManager = SetAccountManager(isPayingCustomer)
            };

            int id = _ticketRepository.CreateTicket(ticket);
            return id;
        }

        private User SetAssignedUser(string assignedToUser)
        {
            User assignedUser = new UserRepository().GetUser(assignedToUser);
            return assignedUser;
        }

        private double SetPrice(bool isPayingCustomer, Priority priority)
        {
            if (isPayingCustomer)
            {
                if (priority == Priority.High)
                {
                    return 100;
                }

                return 50;
            }

            return 0;
        }

        private User SetAccountManager(bool isPayingCustomer)
        {
            if (isPayingCustomer)
            {
                return new UserRepository().GetAccountManager();
            }

            return null;
        }

        private Priority SetPriority(string title, Priority priority, DateTime createdOn)
        {
            bool priorityRaised = false;
            if (createdOn < DateTime.UtcNow - TimeSpan.FromHours(1))
            {
                switch (priority)
                {
                    case Priority.Low:
                        priority = Priority.Medium;
                        priorityRaised = true;
                        break;
                    case Priority.Medium:
                        priority = Priority.High;
                        priorityRaised = true;
                        break;
                }
            }

            if ((title.Contains("Crash") || title.Contains("Important") || title.Contains("Failure")) &&
                !priorityRaised)
            {
                priority = priority switch
                {
                    Priority.Low => Priority.Medium,
                    Priority.Medium => Priority.High,
                    _ => priority
                };
            }

            return priority;
        }

        private static void ValidationCheck(string title, string assignedToUser, string description)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new InvalidTicketException("Title is null or empty.");
            }

            if (string.IsNullOrWhiteSpace(assignedToUser))
            {
                throw new InvalidTicketException("AssignedToUser is null or empty.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new InvalidTicketException("Description is null or empty.");
            }
        }

        private static void ValidationCheck(string username, Ticket ticket, int id)
        {
            if (ticket == null)
            {
                throw new ApplicationException("No ticket found for id " + id);
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new UnknownUserException($"{username} is not valid.");
            }
        }

        public void AssignTicket(int id, string username)
        {
            Ticket ticket = _ticketRepository.GetTicket(id);

            ValidationCheck(username, ticket, id);

            ticket.AssignedUser = SetAssignedUser(username);

            _ticketRepository.UpdateTicket(ticket);

            WriteTicketToFile(ticket);
        }

        private void WriteTicketToFile(Ticket ticket)
        {
            var ticketJson = JsonSerializer.Serialize(ticket);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"ticket_{ticket.Id}.json"), ticketJson);
            // Console.WriteLine(Path.GetTempPath());
            Console.WriteLine("ticket saved to file.");
        }
    }

    public enum Priority
    {
        High,
        Medium,
        Low
    }
}