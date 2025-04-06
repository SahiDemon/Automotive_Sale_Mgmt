using Automotive_Sale_Mgmt.Models;
using System.Collections.Concurrent;

namespace Automotive_Sale_Mgmt.Services
{
    public class InMemoryUserService
    {
        private static readonly ConcurrentDictionary<string, ApplicationUser> _users = new();
        private static readonly ConcurrentDictionary<string, string> _passwords = new();

        // Add a demo user for testing
        static InMemoryUserService()
        {
            var demoUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "demo@example.com",
                Email = "demo@example.com",
                EmailConfirmed = true,
                FirstName = "Demo",
                LastName = "User",
                CreatedOn = DateTime.UtcNow,
            };
            
            _users.TryAdd(demoUser.Email.ToLower(), demoUser);
            _passwords.TryAdd(demoUser.Email.ToLower(), "Demo@123"); // Never store passwords like this in a real app
        }

        public ApplicationUser? GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            _users.TryGetValue(email.ToLower(), out var user);
            return user;
        }

        public bool ValidateUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            _passwords.TryGetValue(email.ToLower(), out var storedPassword);
            return password == storedPassword;
        }

        public bool RegisterUser(string email, string password, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName))
            {
                return false;
            }

            email = email.ToLower();
            if (_users.ContainsKey(email))
                return false;

            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                CreatedOn = DateTime.UtcNow
            };

            return _users.TryAdd(email, newUser) && _passwords.TryAdd(email, password);
        }

        public IEnumerable<ApplicationUser> GetAllUsers()
        {
            return _users.Values.ToList();
        }
    }
}