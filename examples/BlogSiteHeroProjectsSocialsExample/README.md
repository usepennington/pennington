# BlogSiteHeroProjectsSocialsExample

Populates the four homepage surfaces on `BlogSiteOptions`: `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks`. Demonstrates the built-in `SocialIcons` (`GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon`) as the icon render fragments.

## Concepts

- `HeroContent` headline block above the recent-posts list (renders as the page's `<h1>`)
- `Project` / `MyWork` cards in the home page sidebar (each section heading is an `<h2>` — the hero h1 + project h2 + posts-list h2 + about h2 hierarchy is the intentional BlogSite homepage outline)
- `SocialLink` + built-in `SocialIcons` render fragments — `GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, and `MastodonIcon`. The Mastodon icon is a generic Mastodon glyph; pass any instance URL (`https://hachyderm.io/@user`, `https://mastodon.social/@user`, etc.) and the icon renders the same — the URL is what identifies the user's home server.
- `HeaderLink` entries for the top-nav

## Tutorial stages

`Stage1_HeroOnly.cs` → `Stage2_AddProjects.cs` → `Stage3_AddSocialsAndHeader.cs`. Verified the tutorial at `docs/.../tutorials/blogsite/hero-projects-socials.md` fences all three via `csharp:xmldocid` against `M:BlogSiteHeroProjectsSocialsExample.Stage{1,2,3}.Run`.

## Referenced from

- `docs/.../tutorials/blogsite/hero-projects-socials.md`
- `docs/.../how-to/feeds/blogsite-homepage.md`
