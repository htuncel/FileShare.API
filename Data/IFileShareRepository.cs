using FileShare.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileShare.API.Data
{
    public interface IFileShareRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();
        Task<User> GetUser(Guid id);
        Task<Folder> GetFolder(Guid id);
        Task<IEnumerable<Folder>> GetFolders(Guid id);
        Task<OneTimeAccessLink> GetOneTimeAccessLink(Guid id);
        Task<IEnumerable<OneTimeAccessLink>> GetOneTimeAccessLinks(Guid id);
        Task<File> GetFile(Guid id);
        Task<bool> FolderExists(string folderName, Guid userId);
        Task<bool> FileExists(string fileName, Guid folderId);
    }
}
