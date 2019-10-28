using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileShare.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FileShare.API.Data
{
    public class FileShareRepository : IFileShareRepository
    {
        private readonly DataContext _context;
        public FileShareRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<User> GetUser(Guid id)
        {
            var user = await _context.Users.Include(f => f.Folders).FirstOrDefaultAsync(f => f.Id == id);

            return user;
        }
        public async Task<Folder> GetFolder(Guid id)
        {
            var folder = await _context.Folders.Include(f => f.Files).FirstOrDefaultAsync(f => f.Id == id);

            return folder;
        }
        public async Task<IEnumerable<Folder>> GetFolders(Guid id)
        {
            List<Folder> folders = await _context.Folders.Where(f => f.UserId == id).Include(f => f.Files).ToListAsync();

            return folders;
        }

        public async Task<File> GetFile(Guid id)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id);

            return file;
        }

        public async Task<bool> FolderExists(string folderName, Guid userId)
        {
            if (await _context.Folders.AnyAsync(f => f.UserId == userId && f.FolderName == folderName))
            {
                return true;
            }
            return false;
        }

        public async Task<bool> FileExists(string fileName, Guid folderId)
        {
            if (await _context.Files.AnyAsync(f => f.FolderId == folderId && f.FileName == fileName))
            {
                return true;
            }
            return false;
        }

        public async Task<OneTimeAccessLink> GetOneTimeAccessLink(Guid id)
        {
            OneTimeAccessLink link = await _context.OneTimeAccessLinks.FirstOrDefaultAsync(otal => otal.Id == id);

            return link;
        }
        
        public async Task<IEnumerable<OneTimeAccessLink>> GetOneTimeAccessLinks(Guid id)
        {
            List<OneTimeAccessLink> links = await _context.OneTimeAccessLinks.Where(a => a.UserId == id).ToListAsync();

            return links;
        }
    }
}
