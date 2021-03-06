﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Cohort
    {
        [Display(Name = "Cohort")]
        public string Name { get; set; }

        public int Id { get; set; }


        public List<Student> StudentsInCohort { get; set; } = new List<Student>();
        public List<Instructor> InstructorsInCohort { get; set; } = new List<Instructor>();
    }
}
