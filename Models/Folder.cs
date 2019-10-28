using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileShare.API.Models
{
    public class Folder
    {
        public Guid Id { get; set; }
        public string FolderName { get; set; }
        public string FolderDescription { get; set; }
        public List<File> Files { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }

        public Folder()
        {
            Files = new List<File>();
        }
    }
}
