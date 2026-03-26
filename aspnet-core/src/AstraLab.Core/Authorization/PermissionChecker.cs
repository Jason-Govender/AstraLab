using Abp.Authorization;
using AstraLab.Authorization.Roles;
using AstraLab.Authorization.Users;

namespace AstraLab.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
