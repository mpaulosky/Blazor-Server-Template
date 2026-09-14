# Blazor Server Template

A `dotnet new` template (short name `blazorserver`) that scaffolds a Blazor Web App running in
Interactive Server render mode, with Tailwind CSS theming and Auth0 authentication/authorization
already wired in.

This repository builds and publishes the **Template** — the installable `dotnet new` package.
Running `dotnet new blazorserver` produces the **Generated App**, a separate, ready-to-run project
that a consumer builds, runs, and deploys.

## What you get

- **Blazor Web App, Interactive Server** — no separate API or Shared project; components call
  service/domain code directly (see [`docs/adr/0001`](docs/adr/0001-web-only-project-layout.md)).
- **Tailwind CSS v4** — built with the standalone Tailwind CLI binary at compile time, no Node/npm
  required.
- **A fixed color palette, chosen at generation time** — pick one of six palettes (Blue, Rose,
  Violet, Emerald, Amber, Slate) with the `--palette` parameter; it's baked into the generated CSS
  with no in-app switcher (see [`docs/adr/0002`](docs/adr/0002-palette-fixed-at-generation-time.md)).
  Light/dark mode is a separate, runtime toggle available to every visitor.
- **Auth0 authentication/authorization** — Visitor, User (authenticated), and Admin (role-based)
  pages out of the box.
- **Test projects** — `Web.Tests.Unit` (xUnit.v3), `Web.Tests.Bunit` (bUnit component tests), and
  `Web.Tests.E2E` (Playwright).
- Ready-to-use `.editorconfig`, `.gitignore`, `GitVersion.yml`, `NuGet.config`, markdownlint, and
  yamllint configuration.

## Prerequisites

- [.NET SDK 10.0.401](content/global.json) or later (the pinned version generated apps use)
- Bash (Linux/macOS) or PowerShell (Windows) — used to download the standalone Tailwind CLI binary
  on first build
- An [Auth0](https://auth0.com/) application (Domain, Client ID, Client Secret) if you want
  authentication to work

## Installing the template

Install directly from NuGet.org:

```bash
dotnet new install Mpaulosky.BlazorServerTemplate
```

Or install from a local clone of this repository:

```bash
git clone https://github.com/mpaulosky/Blazor-Server-Template.git
cd Blazor-Server-Template
dotnet new install ./content
```

## Creating a new project

```bash
dotnet new blazorserver -n MyApp --palette Rose
```

| Parameter    | Description                                                                    | Default |
| ------------ | -------------------------------------------------------------------------------- | ------- |
| `-n`, `--name` | Name of the generated project/output directory                                 | —       |
| `--palette`  | Color palette: `Blue`, `Rose`, `Violet`, `Emerald`, `Amber`, `Slate`             | `Blue`  |

This creates a `MyApp/` directory containing the generated solution. Change into it and restore
packages:

```bash
cd MyApp
dotnet restore
```

## Running the generated app

```bash
dotnet run --project src/Web
```

On first build, the Tailwind CSS standalone CLI is downloaded automatically and used to build
`wwwroot/app.css` from `Styles/app.css` and the chosen palette — no Node/npm step required.

### Configuring Auth0

Set the following in `src/Web/appsettings.Development.json` (gitignored) or via user secrets —
do not commit real credentials to `appsettings.json`:

```json
{
  "Auth0": {
    "Domain": "",
    "ClientId": "",
    "ClientSecret": ""
  }
}
```

## Running the tests

```bash
dotnet test
```

This runs the unit tests (`tests/Web.Tests.Unit`) and bUnit component tests
(`tests/Web.Tests.Bunit`). The Playwright E2E suite (`tests/Web.Tests.E2E`) requires the app to be
running and browsers installed (`pwsh tests/Web.Tests.E2E/bin/Debug/net10.0/playwright.ps1 install`
after first build).

## Repository layout

```
content/                    The template's source, i.e. what `dotnet new blazorserver` generates
  .template.config/          Template metadata (template.json) — parameters, exclusions, renames
  src/Web/                   The Blazor Web App
  tests/                     Unit, bUnit, and E2E test projects
  build/                     Tailwind CLI download scripts
docs/
  adr/                       Architecture Decision Records
  CONTRIBUTING.md
  CODE_OF_CONDUCT.md
  SECURITY.md
CONTEXT.md                   Domain vocabulary for this repository
Mpaulosky.BlazorServerTemplate.csproj   Packs `content/` into the NuGet template package
```

## Developing this template

Changes to the template live under `content/`. To iterate locally without publishing to NuGet:

```bash
dotnet new install ./content --force
dotnet new blazorserver -n TestApp -o /tmp/TestApp
```

Uninstall when done:

```bash
dotnet new uninstall ./content
```

To pack the NuGet package:

```bash
dotnet pack Mpaulosky.BlazorServerTemplate.csproj
```

See [`docs/CONTRIBUTING.md`](docs/CONTRIBUTING.md) for contribution guidelines and
[`CONTEXT.md`](CONTEXT.md) for the domain vocabulary used throughout this repository (Template vs.
Generated App, Palette vs. Theme, Visitor/User/Admin).

## License

[MIT](LICENSE)
