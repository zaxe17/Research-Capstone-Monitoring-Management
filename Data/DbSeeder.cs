using Microsoft.EntityFrameworkCore;
using monitoring_management.Models;

namespace monitoring_management.Data
{
    public static class DbSeeder
    {
        public static void SeedPrograms(ApplicationDbContext context)
        {
            if (context.AcademicPrograms.Any()) return; // already seeded

            var programs = new List<AcademicProgram>
            {
                // College of Accountancy and Finance
                new() { Code = "BSA", CollegeName = "College of Accountancy and Finance", SortOrder = 1 },
                new() { Code = "BSMA", CollegeName = "College of Accountancy and Finance", SortOrder = 2 },
                new() { Code = "BSBAFM", CollegeName = "College of Accountancy and Finance", SortOrder = 3 },

                // College of Architecture, Design and the Built Environment
                new() { Code = "BS-ARCH", CollegeName = "College of Architecture, Design and the Built Environment", SortOrder = 4 },
                new() { Code = "BSID", CollegeName = "College of Architecture, Design and the Built Environment", SortOrder = 5 },
                new() { Code = "BSEP", CollegeName = "College of Architecture, Design and the Built Environment", SortOrder = 6 },

                // College of Arts and Letters
                new() { Code = "ABELS", CollegeName = "College of Arts and Letters", SortOrder = 7 },
                new() { Code = "ABF", CollegeName = "College of Arts and Letters", SortOrder = 8 },
                new() { Code = "ABLCS", CollegeName = "College of Arts and Letters", SortOrder = 9 },
                new() { Code = "AB-PHILO", CollegeName = "College of Arts and Letters", SortOrder = 10 },
                new() { Code = "BPEA", CollegeName = "College of Arts and Letters", SortOrder = 11 },

                // College of Business Administration
                new() { Code = "BSBAHRM", CollegeName = "College of Business Administration", SortOrder = 12 },
                new() { Code = "BSBA-MM", CollegeName = "College of Business Administration", SortOrder = 13 },
                new() { Code = "BSENTREP", CollegeName = "College of Business Administration", SortOrder = 14 },
                new() { Code = "BSOA", CollegeName = "College of Business Administration", SortOrder = 15 },

                // College of Communication
                new() { Code = "BADPR", CollegeName = "College of Communication", SortOrder = 16 },
                new() { Code = "BA Broadcasting", CollegeName = "College of Communication", SortOrder = 17 },
                new() { Code = "BACR", CollegeName = "College of Communication", SortOrder = 18 },
                new() { Code = "BAJ", CollegeName = "College of Communication", SortOrder = 19 },

                // College of Computer and Information Sciences
                new() { Code = "BSCS", CollegeName = "College of Computer and Information Sciences", SortOrder = 20 },
                new() { Code = "BSIT", CollegeName = "College of Computer and Information Sciences", SortOrder = 21 },

                // College of Education
                new() { Code = "BTLEd", CollegeName = "College of Education", SortOrder = 22 },
                new() { Code = "BLIS", CollegeName = "College of Education", SortOrder = 23 },
                new() { Code = "BSEd", CollegeName = "College of Education", SortOrder = 24 },
                new() { Code = "BEEd", CollegeName = "College of Education", SortOrder = 25 },
                new() { Code = "BECEd", CollegeName = "College of Education", SortOrder = 26 },

                // College of Engineering
                new() { Code = "BSCE", CollegeName = "College of Engineering", SortOrder = 27 },
                new() { Code = "BSCpE", CollegeName = "College of Engineering", SortOrder = 28 },
                new() { Code = "BSEE", CollegeName = "College of Engineering", SortOrder = 29 },
                new() { Code = "BSECE", CollegeName = "College of Engineering", SortOrder = 30 },
                new() { Code = "BSIE", CollegeName = "College of Engineering", SortOrder = 31 },
                new() { Code = "BSME", CollegeName = "College of Engineering", SortOrder = 32 },
                new() { Code = "BSRE", CollegeName = "College of Engineering", SortOrder = 33 },

                // College of Human Kinetics
                new() { Code = "BPE", CollegeName = "College of Human Kinetics", SortOrder = 34 },
                new() { Code = "BSESS", CollegeName = "College of Human Kinetics", SortOrder = 35 },

                // College of Political Science and Public Administration
                new() { Code = "BPA", CollegeName = "College of Political Science and Public Administration", SortOrder = 36 },
                new() { Code = "BAIS", CollegeName = "College of Political Science and Public Administration", SortOrder = 37 },
                new() { Code = "BAPE", CollegeName = "College of Political Science and Public Administration", SortOrder = 38 },
                new() { Code = "BAPS", CollegeName = "College of Political Science and Public Administration", SortOrder = 39 },

                // College of Social Sciences and Development
                new() { Code = "ABS", CollegeName = "College of Social Sciences and Development", SortOrder = 40 },
                new() { Code = "BSE", CollegeName = "College of Social Sciences and Development", SortOrder = 41 },
                new() { Code = "BAH", CollegeName = "College of Social Sciences and Development", SortOrder = 42 },
                new() { Code = "BAPHS", CollegeName = "College of Social Sciences and Development", SortOrder = 43 },
                new() { Code = "BSPSY", CollegeName = "College of Social Sciences and Development", SortOrder = 44 },
                new() { Code = "BHS", CollegeName = "College of Social Sciences and Development", SortOrder = 45 },
                new() { Code = "BSSW", CollegeName = "College of Social Sciences and Development", SortOrder = 46 },

                // College of Science
                new() { Code = "BSFT", CollegeName = "College of Science", SortOrder = 47 },
                new() { Code = "BSAPMATH", CollegeName = "College of Science", SortOrder = 48 },
                new() { Code = "BSBIO", CollegeName = "College of Science", SortOrder = 49 },
                new() { Code = "BSCHEM", CollegeName = "College of Science", SortOrder = 50 },
                new() { Code = "BSMATH", CollegeName = "College of Science", SortOrder = 51 },
                new() { Code = "BSND", CollegeName = "College of Science", SortOrder = 52 },
                new() { Code = "BSPHY", CollegeName = "College of Science", SortOrder = 53 },
                new() { Code = "BSSTAT", CollegeName = "College of Science", SortOrder = 54 },

                // College of Tourism, Hospitality and Transportation Management
                new() { Code = "BSHM", CollegeName = "College of Tourism, Hospitality and Transportation Management", SortOrder = 55 },
                new() { Code = "BSTM", CollegeName = "College of Tourism, Hospitality and Transportation Management", SortOrder = 56 },
                new() { Code = "BSTRM", CollegeName = "College of Tourism, Hospitality and Transportation Management", SortOrder = 57 },
            };

            context.AcademicPrograms.AddRange(programs);
            context.SaveChanges();
        }
    }
}