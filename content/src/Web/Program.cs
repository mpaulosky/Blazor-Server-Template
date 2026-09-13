using BlazorServerTemplate.Web.Components;
using BlazorServerTemplate.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0ClientId = builder.Configuration["Auth0:ClientId"];
var auth0ClientSecret = builder.Configuration["Auth0:ClientSecret"];
var auth0Configured = !string.IsNullOrWhiteSpace(auth0Domain)
    && !string.IsNullOrWhiteSpace(auth0ClientId)
    && !string.IsNullOrWhiteSpace(auth0ClientSecret);

var authenticationBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        if (auth0Configured)
        {
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
        }
    })
    .AddCookie();

// The OIDC handler validates its options (Authority, ClientId, ...) on every request, not just
// on challenge — an ASP.NET Core remote-auth handler must inspect every request to see whether
// it's its own callback. Registering it with empty Auth0 config would 500 every page, including
// public ones, so it's only added once real Auth0 credentials are configured.
if (auth0Configured)
{
    authenticationBuilder.AddOpenIdConnect(options =>
    {
        options.Authority = $"https://{auth0Domain}";
        options.ClientId = auth0ClientId;
        options.ClientSecret = auth0ClientSecret;
        options.ResponseType = "code";
        options.CallbackPath = "/callback";
        options.ClaimsIssuer = "Auth0";
        options.SaveTokens = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "https://blazorservertemplate/roles";

        options.Events = new OpenIdConnectEvents
        {
            // Auth0 has its own logout endpoint separate from the standard OIDC end-session endpoint.
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                var logoutUri = $"https://{auth0Domain}/v2/logout?client_id={auth0ClientId}";

                var postLogoutUri = context.Properties.RedirectUri;
                if (!string.IsNullOrEmpty(postLogoutUri))
                {
                    if (postLogoutUri.StartsWith('/'))
                    {
                        var request = context.Request;
                        postLogoutUri = request.Scheme + "://" + request.Host + postLogoutUri;
                    }

                    logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                }

                context.Response.Redirect(logoutUri);
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IReleaseInfoService, GitHubReleaseInfoService>(client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BlazorServerTemplate");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/login", (HttpContext http, string? returnUrl) =>
    auth0Configured
        ? Results.Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
            [OpenIdConnectDefaults.AuthenticationScheme])
        : Results.Problem(
            "Auth0 is not configured. Set Auth0:Domain, Auth0:ClientId and Auth0:ClientSecret (see appsettings.json / user-secrets).",
            statusCode: StatusCodes.Status503ServiceUnavailable));

app.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    if (auth0Configured)
    {
        await http.SignOutAsync(
            OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = "/" });
    }
});

app.Run();

public partial class Program;
