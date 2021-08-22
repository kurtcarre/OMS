using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OMS.Models
{
    public class ChildMember
    {
        [Required]
        [Key]
        [Display(Name = "Member number")]
        public int MemberNo { get; set; }
        public virtual Member Member { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DoB { get; set; }

        public int Age
        {
            get
            {
                TimeSpan difference = DateTime.Today.Date - DoB.Date;
                DateTime retDate = new DateTime(difference.Ticks);
                int ret = retDate.Year-1;
                return ret;
            }
        }

        [Required]
        [Display(Name = "Parent first name")]
        public string ParentFirstName { get; set; }
        [Required]
        [Display(Name = "Parent last name")]
        public string ParentLastName { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Emergency contact number")]
        public string EmergencyContactNo { get; set; }

        public bool Consent { get; set; }
    }
}