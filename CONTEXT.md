# Blazor Server Template

A `dotnet new` template that scaffolds a Blazor Web App (server-rendered) with Tailwind CSS theming and Auth0 authentication/authorization baked in.

## Language

**Template**:
The installable `dotnet new` package (short name `blazorserver`) that this repository builds and publishes. Distinct from the app it produces.
_Avoid_: Project, solution (when referring to the package itself)

**Generated App**:
The Blazor Web App produced by running `dotnet new blazorserver`. What a template consumer actually builds, runs, and deploys.
_Avoid_: Instance, output, the app (when the distinction from Template matters)

**Palette**:
A curated, coordinated set of Tailwind CSS custom properties (e.g. Rose, Blue, Violet) selected once via the `--palette` parameter when the Template is generated. Fixed for the lifetime of the Generated App — there is no in-app UI to change it.
_Avoid_: Theme, color scheme, skin

**Theme**:
The light/dark rendering mode. Chosen by the visitor at runtime via a toggle in the Generated App, persisted in `localStorage`, and defaulting to the OS `prefers-color-scheme` on first visit.
_Avoid_: Palette, mode, color scheme

**Visitor**:
An unauthenticated caller of the Generated App. Can see public pages (e.g. Home) but not the Profile or Admin pages.
_Avoid_: Guest, anonymous user

**User**:
A Visitor who has authenticated through Auth0. Can see the Profile page, showing their own claims.
_Avoid_: Account, member

**Admin**:
A User whose Auth0 claims include the Admin role. The only audience for the Admin page, which demonstrates role-based authorization.
_Avoid_: Administrator, superuser
