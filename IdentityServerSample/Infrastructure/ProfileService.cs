using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServerSample.Infrastructure
{
    public class ProfileService : IProfileService
    {

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.DateOfBirth, "01.01.2010")
            };
            context.IssuedClaims.AddRange(claims);
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;

            return Task.CompletedTask;
        }
    }
}
