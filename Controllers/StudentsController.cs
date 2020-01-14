using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Students
        public async Task<ActionResult> Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, c.Id AS CohortId, c.Name AS CohortName,  e.Id AS ExerciseId, e.Name, e.Language FROM Student s
                                        LEFT JOIN StudentExercise se ON se.StudentId = s.Id
                                        LEFT JOIN Exercise e ON e.Id = se.ExerciseId
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Student> students = new List<Student>();


                    while (reader.Read())
                    {
                        //create student ID
                        var studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        //search to see if student is already added
                        var studentAlreadyAdded = students.FirstOrDefault(s => s.Id == studentId);
                        //create bool for if there is an exercise in row
                        var hasExercise = !reader.IsDBNull(reader.GetOrdinal("ExerciseId"));
                        //if statement for adding new student, null means they were NOT found, let's add them!
                        if (studentAlreadyAdded == null)
                        {

                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                Cohort = new Cohort()
                                {
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                }

                            };
                            students.Add(student);

                            //If row has an exercise then add it to the exercise list
                            if (hasExercise)
                            {
                                student.StudentsExercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });

                            }
                        }
                        else
                        //Student was already added!  Lets check to see if there are exercises to add.
                        {


                            if (hasExercise)
                            {
                                studentAlreadyAdded.StudentsExercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });

                            }
                        }
                    }
                    reader.Close();
                    //from controllerbase interface - returns official json result with 200 status code
                    return View(students);
                }
            }
        }
           
        

        // GET: Students/Details/5
        public async Task<ActionResult> Details([FromRoute]int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, c.Id AS CohortId, c.Name AS CohortName,  e.Id AS ExerciseId, e.Name, e.Language FROM Student s
                                LEFT JOIN StudentExercise se ON se.StudentId = s.Id
                                LEFT JOIN Exercise e ON e.Id = se.ExerciseId
                                LEFT JOIN Cohort c ON s.CohortId = c.Id


                                WHERE s.Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Student student = null;

                    while (reader.Read())
                    {
                        if (student == null)
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                StudentsExercises = new List<Exercise>(),
                                Cohort = new Cohort()
                                {
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                }

                            };
                            if (!reader.IsDBNull(reader.GetOrdinal("Name")))
                            {
                                student.StudentsExercises.Add(new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });
                            };     
                        }
                        else if (!reader.IsDBNull(reader.GetOrdinal("Name")) && student != null)
                        {
                                student.StudentsExercises.Add(new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                });  
                        }
                    }
                    if (student == null)
                    {
                        reader.Close();
                        return NotFound();
                    }
                    reader.Close();
                    return View(student);
                }
            }
        }
            
        

        // GET: Students/Create
        public ActionResult Create()
        {
            var exercises = GetExercises().Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            }).ToList();

            var cohorts = GetAllCohorts().Select(c => new SelectListItem
             {
                Text = c.Name,
                Value = c.Id.ToString()
             }).ToList();

            cohorts.Insert(0, new SelectListItem
            {
                Text = "Choose Cohort...",
                Value = "0"
            });

            var viewModel = new StudentViewModel
            {
                Student = new Student(),
                Exercises = exercises,
                Cohorts = cohorts
            };
            return View(viewModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        //helper method to grab exercises list
        private List<Exercise> GetExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ID, Name, Language FROM Exercise";

                    var reader = cmd.ExecuteReader();

                    var exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        exercises.Add(new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        });
                    }

                    reader.Close();

                    return exercises;
                }
            }
        }

        //helper method to grab all cohorts
        private List<Cohort> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Cohort";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        cohorts.Add(new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        });
                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }
    }
}