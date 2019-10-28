using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FileShare.API.Data;
using FileShare.API.Dtos;
using FileShare.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FileShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly IFileShareRepository _fileRepo;
        private readonly IHostingEnvironment _hostingEnv;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository authRepo, IFileShareRepository fileRepo, IHostingEnvironment hostingEnvironment, IConfiguration config)
        {
            _config = config;
            _authRepo = authRepo;
            _fileRepo = fileRepo;
            _hostingEnv = hostingEnvironment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Email = userForRegisterDto.Email.ToLower();

            if (await _authRepo.UserExists(userForRegisterDto.Email))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Email = userForRegisterDto.Email
            };

            var createdUser = await _authRepo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _authRepo.Login(userForLoginDto.Email, userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            try
            {
                string folderName = "App_Data/" + userFromRepo.Id.ToString();
                string webRootPath = _hostingEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Folder folder = new Folder
                    {
                        Id = Guid.NewGuid(),
                        UserId = userFromRepo.Id,
                        FolderName = userFromRepo.Id.ToString(),
                        FolderDescription = "User Root"
                    };

                    userFromRepo.Folders.Add(folder);
                     if (await _fileRepo.SaveAll())
                     {
                         Directory.CreateDirectory(newPath);
                     }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(400, ex.Message);
            }

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}