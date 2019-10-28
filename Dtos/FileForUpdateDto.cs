using System.ComponentModel.DataAnnotations;

namespace FileShare.API.Dtos
{
    public class FileForUpdateDto
    {

        [Required]
        public string Name { get; set; }
    }
}
