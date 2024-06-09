using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT_LAB
{
    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public bool Blocked { get; internal set; }
    }

    public class FileMetadata
    {
        public int FileMetadataId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
        public string ModificationHistory { get; set; }
    }

    public class UserFilePermission
    {
        public int UserFilePermissionId { get; set; }
        public int UserId { get; set; }
        public int FileMetadataId { get; set; }
        public string Permission { get; set; }
        public User User { get; set; }
        public FileMetadata FileMetadata { get; set; }
    }

    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public int FileMetadataId { get; set; }
        public string NotificationMessage { get; set; }
        public User User { get; set; }
        public FileMetadata FileMetadata { get; set; }
    }

}
