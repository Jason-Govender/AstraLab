using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Abp.Authorization.Users;
using Abp.Domain.Services;
using Abp.IdentityFramework;
using Abp.UI;
using AstraLab.Authorization.Roles;
using AstraLab.MultiTenancy;

namespace AstraLab.Authorization.Users
{
    public class UserRegistrationManager : DomainService
    {
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRegistrationManager(
            TenantManager tenantManager,
            UserManager userManager,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher)
        {
            _tenantManager = tenantManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> RegisterAsync(string name, string surname, string emailAddress, string userName, string plainPassword, int tenantId, bool isEmailConfirmed)
        {
            var tenant = await GetActiveTenantAsync(tenantId);

            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                var user = new User
                {
                    TenantId = tenant.Id,
                    Name = name,
                    Surname = surname,
                    EmailAddress = emailAddress,
                    IsActive = true,
                    UserName = userName,
                    IsEmailConfirmed = isEmailConfirmed,
                    Roles = new List<UserRole>()
                };

                user.SetNormalizedNames();

                foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
                {
                    user.Roles.Add(new UserRole(tenant.Id, user.Id, defaultRole.Id));
                }

                await _userManager.InitializeOptionsAsync(tenant.Id);

                CheckErrors(await _userManager.CreateAsync(user, plainPassword));
                await CurrentUnitOfWork.SaveChangesAsync();

                return user;
            }
        }

        private async Task<Tenant> GetActiveTenantAsync(int tenantId)
        {
            var tenant = await _tenantManager.FindByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
            }

            return tenant;
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
