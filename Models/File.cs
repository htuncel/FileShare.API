using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileShare.API.Models
{
    public class File
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long FileSize { get; set; }
        public DateTime Created { get; set; }
        public Folder Folder { get; set; }
        public Guid FolderId { get; set; }
    }
}