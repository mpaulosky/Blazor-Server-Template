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

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        var auth0 = builder.Configuration.GetSection("Auth0");

        options.Authority = $"https://{auth0["Domain"]}";
        options.ClientId = auth0["ClientId"];
        options.ClientSecret = auth0["ClientSecret"];
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
                var logoutUri = $"https://{auth0["Domain"]}/v2/logout?client_id={auth0["ClientId"]}";

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
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        [OpenIdConnectDefaults.AuthenticationScheme]));

app.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignOutAsync(
        OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
});

app.Run();

public partial class Program;
