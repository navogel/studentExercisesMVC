using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "YO YOU GOT THE WRONG FIGS")]
        public string SlackHandle { get; set; }
        [Required]

        public string Specialty { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int CohortId { get; set; }
        public Cohort InstructorCohort { get; set; }
    }
}
