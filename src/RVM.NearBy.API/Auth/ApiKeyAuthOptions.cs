using Microsoft.AspNetCore.Authentication;

namespace RVM.NearBy.API.Auth;

public class ApiKeyAuthOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "ApiKey";
    public string ApiKey { get; set; } = string.Empty;
}
