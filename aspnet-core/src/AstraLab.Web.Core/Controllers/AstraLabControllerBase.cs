using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace AstraLab.Controllers
{
    public abstract class AstraLabControllerBase: AbpController
    {
        protected AstraLabControllerBase()
        {
            LocalizationSourceName = AstraLabConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
