using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels
{
    public class CreateStudentViewModel
    {
        public Student Student { get; set; } = new Student();

        public List<int> ExerciseIds { get; set; } = new List<int>();
    }
}
