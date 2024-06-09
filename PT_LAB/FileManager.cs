using System;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PT_LAB
{
    public class FileManager
    {
        private readonly ApplicationDbContext _context;

        public FileManager()
        {
            _context = new ApplicationDbContext();
        }

        public async Task InitializeDatabaseAsync(string password)
        {
            await _context.Database.MigrateAsync();
            var currentUser = Environment.UserName;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == currentUser);

            if (user == null)
            {
                Console.WriteLine("Local user not found. Creating new user.");

                user = new User { Login = currentUser, Password = password, IPAddress = GetLocalIPAddress(), Blocked = false };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                Console.WriteLine("User created successfully.");
            }
            else
            {
                if (user.Password != password)
                {
                    Console.WriteLine("Password incorrect. Updating password.");

                    user.Password = password;
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Password updated successfully.");
                }
            }
        }

        public async Task RegisterRemoteUserAsync(string login, string password, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty", nameof(password));
            }

            var user = new User { Login = login, Password = password, IPAddress = ipAddress, Blocked = false };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task ManageUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                Console.WriteLine($"User: {user.Login}, IP: {user.IPAddress}, Blocked: {user.Blocked}");
            }

            Console.WriteLine("Enter user login to block/unblock: ");
            var login = Console.ReadLine();
            var userToManage = users.FirstOrDefault(u => u.Login == login);

            if (userToManage != null)
            {
                userToManage.Blocked = !userToManage.Blocked;
                await _context.SaveChangesAsync();
                Console.WriteLine($"User {login} {(userToManage.Blocked ? "blocked" : "unblocked")}.");
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
