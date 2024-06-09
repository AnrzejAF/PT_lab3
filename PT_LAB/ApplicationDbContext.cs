using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT_LAB
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FileMetadata> FileMetadatas { get; set; }
        public DbSet<UserFilePermission> UserFilePermissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PT_LAB", "app.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserFilePermission>()
                .HasOne(ufp => ufp.User)
                .WithMany()
                .HasForeignKey(ufp => ufp.UserId);

            modelBuilder.Entity<UserFilePermission>()
                .HasOne(ufp => ufp.FileMetadata)
                .WithMany()
                .HasForeignKey(ufp => ufp.FileMetadataId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.FileMetadata)
                .WithMany()
                .HasForeignKey(n => n.FileMetadataId);
        }
    }

}
