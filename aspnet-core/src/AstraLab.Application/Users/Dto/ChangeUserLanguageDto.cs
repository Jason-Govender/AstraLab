using System.ComponentModel.DataAnnotations;

namespace AstraLab.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}