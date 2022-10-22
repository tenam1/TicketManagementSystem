using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketManagementSystem
{
    public class UserRepository
    {
        private readonly List<User> users = new()
        {
            new() {FirstName = "John", LastName = "Smith", Username = "jsmith"},
            new() {FirstName = "Sarah", LastName = "Berg", Username = "sberg"}
        };

        public User GetUser(string username)
        {
            //checking all users 
            foreach (User user in users)
            {
                if (username == user.Username)
                {
                    return user;
                }
            }

            throw new UnknownUserException("User " + username + " not found");
        }

        public User GetAccountManager()
        {
            // Assume this method does not need to change.
            return GetUser("sberg");
        }
    }
}