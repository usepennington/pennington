# SubPathDeployableExample

Deliberately tiny DocSite host. The teaching surface lives in the sibling deployment fixtures — keeping the host minimal makes `BaseUrlHtmlRewriter`'s prefix behaviour observable when built with a sub-path `baseUrl`.

## Teaching surface (not Program.cs)

- `.github/workflows/deploy.yml` — GitHub Pages workflow
- `staticwebapp.config.json` — Azure Static Web Apps config
- `netlify.toml` — Netlify build config
- `nginx.conf` — self-hosted nginx config
- `web.config` — IIS / Windows hosting config

## Concepts

- `dotnet run -- build /my-sub-path` exercising `BaseUrlHtmlRewriter`
- Static-host configs for the four major deploy targets
- Why the nested `/guides/first-page/` route exists (deep links observable)

## Referenced from

- `docs/.../how-to/deployment/static-build.md`
- `docs/.../how-to/deployment/self-host.md`
- `docs/.../how-to/deployment/adapt-for-other-hosts.md`
- `docs/.../how-to/deployment/base-url.md`
- `docs/.../how-to/deployment/github-pages.md`
