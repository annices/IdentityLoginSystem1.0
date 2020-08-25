namespace LoginSystem.Models
{
    /// <summary>
    /// This class specifies getters and setters for permission roles based on the property values specified in appsettings.json.
    /// Note! If you change the values here, they also have to reflect the property names in the appsettings file and vice versa.
    /// </summary>
    public class Role
    {
        // Set values based on property names defined in appsettings.json:
        private string _sa = "UserRoles:SA";
        private string _a = "UserRoles:A";
        private string _la = "UserRoles:LA";

        public string SA
        {
            get => _sa;
            set => _sa = value;
        }
        public string A
        {
            get => _a;
            set => _a = value;
        }
        public string LA
        {
            get => _la;
            set => _la = value;
        }

    } // End class.
} // End namespace.
