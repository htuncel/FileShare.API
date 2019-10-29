using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FileShare.API.Data;
using FileShare.API.Dtos;
using FileShare.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileShareController : ControllerBase
    {
        private readonly IFileShareRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnv = null;
        public FileShareController(IFileShareRepository repo, IMapper mapper, IHostingEnvironment HostingEnv)
        {
            _mapper = mapper;
            _repo = repo;
            _hostingEnv = HostingEnv;
        }

        [Authorize]
        [HttpGet("folder")]
        public async Task<IActionResult> GetFolders()
        {
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);

            var folders = await _repo.GetFolders(userId);

            return Ok(folders);
        }

        [Authorize]
        [HttpDelete("folder/{id}")]
        public async Task<IActionResult> DeleteFolder(string id)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid folderForRepoId = new Guid(id);

                Folder folder = await _repo.GetFolder(folderForRepoId);

                if (userId != folder.UserId)
                    return Unauthorized();

                _repo.Delete(folder);


                string folderName = "App_Data/" + userId.ToString() + "/" + id;
                string webRootPath = _hostingEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                Directory.Delete(newPath, true);

                if (await _repo.SaveAll())
                {
                    return Ok();
                }

                return BadRequest("Failed to delete the folder");

            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("folder/{id}")]
        public async Task<IActionResult> GetFolder(string id)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid folderForRepoId = new Guid(id);

                Folder folder = await _repo.GetFolder(folderForRepoId);

                if (userId != folder.UserId)
                    return Unauthorized();

                return Ok(folder);

            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("access")]
        public async Task<IActionResult> GetOneTimeAccessLinks()
        {
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);

            var links = await _repo.GetOneTimeAccessLinks(userId);

            return Ok(links);
        }

        [HttpPut("access/{id}")]
        public async Task<IActionResult> ConsumeOneTimeAccessLink(string id)
        {
            try
            {
                Guid linkForRepoId = new Guid(id);
                OneTimeAccessLink link = await _repo.GetOneTimeAccessLink(linkForRepoId);
                Models.File fileFromRepo = await _repo.GetFile(link.FileId);
                Folder folderFromRepo = await _repo.GetFolder(fileFromRepo.FolderId);

                if (link.IsUsed)
                {
                    return BadRequest("The link is expired.");
                }

                link.IsUsed = true;
                link.UsedAt = DateTime.Now;

                string folderName;
                if (folderFromRepo.FolderName == folderFromRepo.UserId.ToString())
                {
                    folderName = "App_Data/" + folderFromRepo.UserId.ToString() + "/" + fileFromRepo.Id;
                }
                else
                {
                    folderName = "App_Data/" + folderFromRepo.UserId.ToString() + "/" + folderFromRepo.Id + "/" + fileFromRepo.Id;
                }
                string webRootPath = _hostingEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);

                if (await _repo.SaveAll())
                {
                    string fileName = fileFromRepo.FileName + "." + fileFromRepo.FileExtension;


                    byte[] fileBytes = System.IO.File.ReadAllBytes(newPath);

                    return new JsonResult(new
                    {
                        File = File(fileBytes, "application/force-download", fileName),
                        Name = fileName
                    });
                }
                return BadRequest("There was a problem accessing file information");

            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("access/{id}")]
        public async Task<IActionResult> DeleteOneTimeAccessLink(string id)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid linkForRepoId = new Guid(id);

                OneTimeAccessLink link = await _repo.GetOneTimeAccessLink(linkForRepoId);

                if(userId != link.UserId)
                {
                    return Unauthorized();
                }

                _repo.Delete(link);                

                if (await _repo.SaveAll())
                {
                    return Ok();
                }

                return BadRequest("There was a problem deleting file access link");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }


        [Authorize]
        [HttpPost("folder/{folderId}/file/{fileId}")]
        public async Task<IActionResult> CreateOneTimeAccessLink(string folderId, string fileId)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid folderForRepoId = new Guid(folderId);
                Guid fileForRepoId = new Guid(fileId);

                Folder folder = await _repo.GetFolder(folderForRepoId);
                Models.File file = await _repo.GetFile(fileForRepoId);

                if (userId != folder.UserId)
                    return Unauthorized();

                OneTimeAccessLink otal = new OneTimeAccessLink();
                otal.Id = Guid.NewGuid();
                otal.UserId = userId;
                otal.IsUsed = false;
                otal.FileName = file.FileName;
                otal.FolderName = folder.FolderName;
                otal.FileId = file.Id;
                otal.UsedAt = null;
                _repo.Add(otal);

                if (await _repo.SaveAll())
                {
                    return StatusCode(200, "Access Link Created.");
                }
                return BadRequest("There was an error creating the access link");
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [Authorize]
        [HttpPut("folder/{id}")]
        public async Task<IActionResult> UpdateFolder(string id, FolderForCreationDto folderForCreationDto)
        {
            {
                try
                {
                    Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                    Guid folderForRepoId = new Guid(id);

                    Folder folder = await _repo.GetFolder(folderForRepoId);

                    if (userId != folder.UserId)
                        return Unauthorized();

                    folder.FolderName = folderForCreationDto.Name;
                    folder.FolderDescription = folderForCreationDto.Description;

                    if (await _repo.SaveAll())
                        return NoContent();

                    return BadRequest("There was a problem updating folder information");

                }
                catch (Exception ex)
                {
                    return StatusCode(400, ex.Message);
                }

            }
        }

        [Authorize]
        [HttpPut("folder/{folderId}/file/{fileId}")]
        public async Task<IActionResult> UpdateFile(string folderId, string fileId, FileForUpdateDto fileForUpdateDto)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid fileForRepoId = new Guid(fileId);
                Guid folderForRepoId = new Guid(folderId);

                Folder folder = await _repo.GetFolder(folderForRepoId);
                Models.File file = await _repo.GetFile(fileForRepoId);

                if (userId != folder.UserId && folder.Id != file.FolderId)
                    return Unauthorized();

                file.FileName = fileForUpdateDto.Name;

                if (await _repo.SaveAll())
                    return NoContent();

                return BadRequest("There was a problem updating file information");

            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }

        }

        [Authorize]
        [HttpPost("folder/{folderId}")]
        public async Task<IActionResult> UploadFile(IFormFile file, string folderId = "")
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid.TryParse(folderId, out Guid folderForRepoId);
                Folder folderFromRepo = await _repo.GetFolder(folderForRepoId);

                string folderName;
                if ((string.IsNullOrEmpty(folderId) && string.IsNullOrWhiteSpace(folderId)) || folderFromRepo.FolderName == userId.ToString())
                {
                    folderName = "App_Data/" + userId.ToString();
                }
                else
                {
                    folderName = "App_Data/" + userId.ToString() + "/" + folderFromRepo.Id.ToString();
                }

                string webRootPath = _hostingEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    return BadRequest("No folder exist in this path");
                }
                if (file.Length > 0)
                {
                    Guid localFileId = Guid.NewGuid();
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string fullPath = Path.Combine(newPath, localFileId.ToString());

                    if (await _repo.FileExists(Path.GetFileNameWithoutExtension(fileName), folderForRepoId))
                        return BadRequest("There is already a file named: " + fileName);

                    Models.File fileForRepo = new Models.File();
                    fileForRepo.Id = localFileId;
                    fileForRepo.FolderId = folderForRepoId;
                    fileForRepo.FileName = Path.GetFileNameWithoutExtension(fileName);
                    fileForRepo.FileExtension = Path.GetExtension(file.FileName).Substring(1);
                    fileForRepo.FileSize = file.Length;
                    fileForRepo.Created = DateTime.Now;
                    folderFromRepo.Files.Add(fileForRepo);

                    //var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx" };
                    //var fileExt = Path.GetExtension(file.FileName).Substring(1);
                    /*
                     * if (!supportedTypes.Contains(fileExt))
                    {
                        return BadRequest("File Extension Is InValid - Only Upload WORD/PDF/EXCEL/TXT File");
                    }
                    */
                    if (file.Length > (10 * 1024 * 1024))
                    {
                        return BadRequest("File size Should Be UpTo " + 10 + "MB");
                    }
                    else
                    {
                        if (await _repo.SaveAll())
                        {
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            return StatusCode(200, "Upload Successful.");
                        }
                    }
                }
                return BadRequest("There was an error creating the file");
            }
            catch (System.Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("folder/{folderId}/file/{fileId}")]
        public async Task<IActionResult> DeleteFile(string folderId, string fileId)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid folderForRepoId = new Guid(folderId);
                Guid fileForRepoId = new Guid(fileId);

                Folder folder = await _repo.GetFolder(folderForRepoId);
                Models.File file = await _repo.GetFile(fileForRepoId);

                if (userId != folder.UserId)
                    return Unauthorized();

                _repo.Delete(file);

                string folderName;
                if (folder.FolderName == userId.ToString())
                {
                    folderName = "App_Data/" + userId.ToString() + "/" + fileId;
                }
                else
                {
                    folderName = "App_Data/" + userId.ToString() + "/" + folderId + "/" + fileId;
                }
                string webRootPath = _hostingEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                System.IO.File.Delete(newPath);

                if (await _repo.SaveAll())
                {
                    return Ok();
                }

                return BadRequest("Failed to delete the file");

            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("folder")]
        public async Task<IActionResult> CreateFolder(FolderForCreationDto folderForCreationDto)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                string name = folderForCreationDto.Name;
                string description = folderForCreationDto.Description;

                if (await _repo.FolderExists(name, userId))
                    return BadRequest("There is already a folder named: " + name);

                if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    return StatusCode(400, "Folder name is not proper");
                }

                User user = await _repo.GetUser(userId);


                Guid localFolderId = Guid.NewGuid();
                string folderName = "App_Data/" + userId.ToString() + "/" + localFolderId.ToString();
                string webRootPath = _hostingEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Folder folder = new Folder();
                    folder.Id = localFolderId;
                    folder.UserId = userId;
                    folder.FolderName = name;
                    folder.FolderDescription = description;
                    user.Folders.Add(folder);

                    if (await _repo.SaveAll())
                    {
                        Directory.CreateDirectory(newPath);
                        return StatusCode(200, "Folder Created.");
                    }
                    return BadRequest("There was an error creating the folder");
                }
                else return BadRequest("There is already a folder named: " + folderName);
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }
        }



        [Authorize]
        [HttpGet("folder/{folderId}/file/{fileId}")]
        public async Task<IActionResult> GetFile(string folderId, string fileId)
        {
            try
            {
                Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier).Value, out Guid userId);
                Guid folderForRepoId = new Guid(folderId);
                Guid fileForRepoId = new Guid(fileId);

                Folder folder = await _repo.GetFolder(folderForRepoId);
                Models.File fileFromRepo = await _repo.GetFile(fileForRepoId);

                if (userId != folder.UserId)
                    return Unauthorized();

                string folderName;
                if (folder.FolderName == folder.UserId.ToString())
                {
                    folderName = "App_Data/" + userId.ToString() + "/" + fileFromRepo.Id;
                }
                else
                {
                    folderName = "App_Data/" + userId.ToString() + "/" + folder.Id + "/" + fileFromRepo.Id;
                }
                string webRootPath = _hostingEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);

                string fileName = fileFromRepo.FileName + "." + fileFromRepo.FileExtension;
                byte[] fileBytes = System.IO.File.ReadAllBytes(newPath);

                return File(fileBytes, "application/force-download", fileName);

            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }

        }
    }
}