using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    public class MailingListEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Notes { get; set; }

        public MailingListEntry()
        {

        }
    }
}