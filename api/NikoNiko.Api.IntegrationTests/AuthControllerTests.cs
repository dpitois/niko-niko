using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class AuthControllerTests
{
    [Fact]
    public async Task SuperAdminJwt_ContainsIsSuperAdminClaim()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // Act
        var (_, _, jwtToken) = await application.CreateUserAndClient("Super Admin", isSuperAdmin: true);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);

        // Assert
        var isSuperAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "is_super_admin");
        Assert.NotNull(isSuperAdminClaim);
        Assert.Equal("true", isSuperAdminClaim.Value, ignoreCase: true);
    }
}
