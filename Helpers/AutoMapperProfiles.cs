using AutoMapper;
using FileShare.API.Dtos;
using FileShare.API.Models;

namespace FileShare.API.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<FolderForCreationDto, Folder>();
            CreateMap<FileForUpdateDto, File>();
        }
    }
}
