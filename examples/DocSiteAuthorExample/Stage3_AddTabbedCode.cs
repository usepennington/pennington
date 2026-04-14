namespace DocSiteAuthorExample;

/// <summary>
/// Stage 3 — Stage 2 plus a tabbed code group. Marking two or more adjacent
/// fenced code blocks with <c>tabs=true</c> and a <c>title="…"</c> fence
/// argument groups them into a single ARIA tablist rendered by
/// <c>TabbedCodeBlockRenderer</c>. Stage 3 mirrors the markdown that ships
/// in <c>Content/guides/authoring.md</c>. Tutorial prose extracts the body
/// of <see cref="Source"/> via <c>xmldocid,bodyonly</c>.
/// </summary>
public static class Stage3
{
    /// <summary>The stage-3 markdown — adds a tabbed code group.</summary>
    public static string Source() => """
        ---
        title: Authoring a doc page
        description: Populate DocSiteFrontMatter, add an alert, and group code samples into tabs.
        tags:
          - authoring
          - front-matter
          - markdown
        sectionLabel: Guides
        order: 20
        ---

        # Authoring a doc page

        ## Callouts

        > [!NOTE]
        > Alerts render with a coloured left border and an icon matching the kind.

        ## Tabbed code groups

        ```bash tabs=true title="dotnet CLI"
        dotnet add package Pennington
        ```

        ```powershell tabs=true title="PowerShell"
        Install-Package Pennington
        ```

        ```xml tabs=true title="csproj"
        <PackageReference Include="Pennington" Version="*" />
        ```
        """;
}
