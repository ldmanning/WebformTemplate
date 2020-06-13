using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;

namespace WebformTemplate.Model
{
    public class User : ModelBase
    {
        private string fullName;
        private string username;
        private string[] userGroups;
        private bool canLoad;
        private string fullUsername;

        public string FullName { get => fullName; set => fullName = value; }
        public string Username { get => username; set => username = value; }
        public string[] UserGroups { get => userGroups; set => userGroups = value; }
        public bool CanLoad { get => canLoad; set => canLoad = value; }
        public string FullUsername { get => fullUsername; set { fullUsername = value; RaisePropertyChanged(); } }

        private bool canEdit;
        public bool CanEdit { get => canEdit; set { canEdit = value; RaisePropertyChanged(); } }

        public User()
        {
            CanLoad = true;
        }

        public User(string str = "")
        {
            try
            {
                string logonName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
                FullUsername = logonName;
                Username = logonName.Substring(logonName.IndexOf(@"\") + 1);



                LoadUserInformation();


                //CanEdit = UserGroups.Any(s => s.Equals(ConfigurationManager.AppSettings["CanEdit"].ToString()));

                if (Username.Equals("ldmanning"))
                {
                    CanLoad = true;
                }

                if (CanEdit) CanLoad = true;
                CanLoad = true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void LoadUserInformation()
        {
            string[] output = null;
            using (var ctx = new PrincipalContext(ContextType.Domain))
            using (var user = UserPrincipal.FindByIdentity(ctx, Username))
            {
                if (user != null)
                {
                    FullName = user.DisplayName;
                    //output = user.GetAuthorizationGroups()
                    //    .Select(x => x.SamAccountName)
                    //    .ToArray();
                }
                else
                {
                    FullName = "NOT FOUND";
                }
            }

            //UserGroups = output;            
        }
    }
}
