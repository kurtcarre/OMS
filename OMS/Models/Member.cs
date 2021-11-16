using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OMS.Models
{
    public class Member
    {
        [Display(Name = "Member Number")]
        public int MemberNo { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        public string Parish { get; set; }
        [Display(Name = "Post code")]
        public string PostCode { get; set; }

        [Required]
        [Display(Name = "Member type")]
        public string MemberType { get; set; }
        [Display(Name = "Under 18")]
        public bool Under18 { get; set; }

        [Required]
        public string Section { get; set; }
        public string Instrument { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date joined")]
        public DateTime DateJoined { get; set; }

        public string Notes { get; set; }

        public static readonly List<SelectListItem> Titles = new List<SelectListItem>(new SelectListItem[]
            {
                new SelectListItem("Mr.", "Mr."),
                new SelectListItem("Mrs.", "Mrs."),
                new SelectListItem("Miss", "Miss"),
                new SelectListItem("Ms.", "Ms."),
                new SelectListItem("Dr.", "Dr.")
            });
        public static readonly List<SelectListItem> Sections = new List<SelectListItem>(new SelectListItem[]
            {
                new SelectListItem("Brass", "Brass"),
                new SelectListItem("Woodwind", "Woodwind"),
                new SelectListItem("Strings", "Strings"),
                new SelectListItem("Misc.", "Misc."),
                new SelectListItem("N/A", "N/A")
            });

        public Member()
        {

        }
    }
}