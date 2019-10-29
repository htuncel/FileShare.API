using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileShare.API.Models
{
    public class OneTimeAccessLink
    {
        public Guid Id { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string FileName { get; set; }
        public string FolderName { get; set; }
        public File File { get; set; }
        public Guid FileId { get; set; }
        public Guid UserId { get; set; }
    }
}
