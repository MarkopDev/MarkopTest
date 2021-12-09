using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Identity;

public class AuthorizationRequirements : IAuthorizationRequirement
{
    public readonly List<string> RoleNames;

    public AuthorizationRequirements(List<string> roleNames)
    {
        RoleNames = roleNames;
    }
}