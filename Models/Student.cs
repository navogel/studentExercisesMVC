using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Student
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "YO YOU GOT THE WRONG FIGS")]
        public string SlackHandle { get; set; }

        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]

        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }


        public List<Exercise> StudentsExercises = new List<Exercise>();
    }
}
