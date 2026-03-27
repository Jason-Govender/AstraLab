using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Users;
using Abp.Authorization;
using Abp.MultiTenancy;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Authorization.Accounts;
using AstraLab.Authorization.Accounts.Dto;
using AstraLab.Authorization.Users;
using AstraLab.MultiTenancy;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Authorization.Accounts
{
    public class AnonymousAccountFlow_Tests : AstraLabTestBase
    {
        private readonly IAccountAppService _accountAppService;
        private readonly LogInManager _logInManager;
        private readonly AnonymousTenantResolver _anonymousTenantResolver;

        public AnonymousAccountFlow_Tests()
        {
            _accountAppService = Resolve<IAccountAppService>();
            _logInManager = Resolve<LogInManager>();
            _anonymousTenantResolver = Resolve<AnonymousTenantResolver>();
        }

        [Fact]
        public async Task Register_Should_Create_User_For_Explicit_Tenant_Without_Ambient_Tenant()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            var userName = $"tenantuser{suffix}";

            using (UseSession(null, null))
            {
                var output = await _accountAppService.Register(new RegisterInput
                {
                    Name = "Tenant",
                    Surname = "User",
                    EmailAddress = $"tenant-{suffix}@example.com",
                    UserName = userName,
                    Password = "Pass1234",
                    TenancyName = AbpTenantBase.DefaultTenantName
                });

                output.CanLogin.ShouldBeTrue();
            }

            UsingDbContext(1, context =>
            {
                var user = context.Users.SingleOrDefault(u => u.UserName == userName);
                user.ShouldNotBeNull();
                user.TenantId.ShouldBe(1);
            });
        }

        [Fact]
        public async Task Register_Should_Reject_Unknown_Tenant()
        {
            using (UseSession(null, null))
            {
                var exception = await Should.ThrowAsync<UserFriendlyException>(() => _accountAppService.Register(new RegisterInput
                {
                    Name = "Unknown",
                    Surname = "Tenant",
                    EmailAddress = "unknown-tenant@example.com",
                    UserName = "unknowntenant",
                    Password = "Pass1234",
                    TenancyName = "missing-tenant"
                }));

                exception.Message.ShouldContain("There is no tenant defined with name missing-tenant");
            }
        }

        [Fact]
        public async Task Register_Should_Reject_Inactive_Tenant()
        {
            const string inactiveTenancyName = "inactive-tenant";

            UsingDbContext(null, context =>
            {
                if (context.Tenants.SingleOrDefault(t => t.TenancyName == inactiveTenancyName) == null)
                {
                    context.Tenants.Add(new Tenant(inactiveTenancyName, "Inactive Tenant")
                    {
                        IsActive = false
                    });
                }
            });

            using (UseSession(null, null))
            {
                var exception = await Should.ThrowAsync<UserFriendlyException>(() => _accountAppService.Register(new RegisterInput
                {
                    Name = "Inactive",
                    Surname = "Tenant",
                    EmailAddress = "inactive-tenant@example.com",
                    UserName = "inactivetenant",
                    Password = "Pass1234",
                    TenancyName = inactiveTenancyName
                }));

                exception.Message.ShouldContain("is not active");
            }
        }

        [Fact]
        public async Task Register_Should_Reject_Host_Context()
        {
            using (UseSession(null, null))
            {
                var exception = await Should.ThrowAsync<UserFriendlyException>(() => _accountAppService.Register(new RegisterInput
                {
                    Name = "Host",
                    Surname = "User",
                    EmailAddress = "host-user@example.com",
                    UserName = "hostuser",
                    Password = "Pass1234"
                }));

                exception.Message.ShouldBe("Tenant is required for registration.");
            }
        }

        [Fact]
        public async Task Authenticate_Should_Succeed_With_Request_Body_TenancyName()
        {
            using (UseSession(null, null))
            {
                var resolvedTenant = await _anonymousTenantResolver.ResolveAsync(AbpTenantBase.DefaultTenantName, allowHost: true);
                var result = await _logInManager.LoginAsync(
                    AbpUserBase.AdminUserName,
                    User.DefaultPassword,
                    resolvedTenant.TenancyName);

                result.Result.ShouldBe(AbpLoginResultType.Success);
                result.User.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task Authenticate_Should_Succeed_With_Ambient_Tenant_Context()
        {
            using (UseSession(1, null))
            {
                var resolvedTenant = await _anonymousTenantResolver.ResolveAsync(null, allowHost: true);
                var result = await _logInManager.LoginAsync(
                    AbpUserBase.AdminUserName,
                    User.DefaultPassword,
                    resolvedTenant.TenancyName);

                result.Result.ShouldBe(AbpLoginResultType.Success);
                result.User.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task Authenticate_Should_Reject_Conflicting_Explicit_And_Ambient_Tenant()
        {
            const string otherTenancyName = "other-tenant";

            var otherTenantId = UsingDbContext(null, context =>
            {
                var tenant = context.Tenants.SingleOrDefault(t => t.TenancyName == otherTenancyName);
                if (tenant == null)
                {
                    tenant = context.Tenants.Add(new Tenant(otherTenancyName, "Other Tenant")).Entity;
                    context.SaveChanges();
                }

                return tenant.Id;
            });

            using (UseSession(otherTenantId, null))
            {
                var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                    _anonymousTenantResolver.ResolveAsync(AbpTenantBase.DefaultTenantName, allowHost: true));

                exception.Message.ShouldBe("Tenant in the request body does not match the current request tenant context.");
            }
        }

        private IDisposable UseSession(int? tenantId, long? userId)
        {
            var previousTenantId = AbpSession.TenantId;
            var previousUserId = AbpSession.UserId;

            AbpSession.TenantId = tenantId;
            AbpSession.UserId = userId;

            return new DisposeAction(() =>
            {
                AbpSession.TenantId = previousTenantId;
                AbpSession.UserId = previousUserId;
            });
        }
    }
}
