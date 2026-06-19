using monitoring_management.Models;

namespace monitoring_management.Services
{
    public static class SidebarData
    {
        public static List<SidebarModel> StudentMenu()
        {
            return new List<SidebarModel>
            {
                new SidebarModel { Title = "Repository", Folder = "Student", Action = "Index" },
                new SidebarModel { Title = "My Works", Folder = "Student", Action = "MyWorks" },
                new SidebarModel { Title = "Submit Research", Folder = "Student", Action = "Submit" },
            };
        }

        public static List<SidebarModel> AdminMenu()
        {
            return new List<SidebarModel>
            {
                new SidebarModel { Title = "Dashboard", Folder = "Admin", Action = "Index" },
                new SidebarModel { Title = "Users", Folder = "Admin", Action = "" },
                new SidebarModel { Title = "Reports", Folder = "Admin", Action = "" },
            };
        }
    }
}