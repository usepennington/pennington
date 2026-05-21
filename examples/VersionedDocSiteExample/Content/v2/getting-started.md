---
title: Getting started (v2)
description: "Install Humanizer.Core 2.14.1 and humanize your first string."
uid: v2.getting-started
order: 20
---

Pin to the current major in your project file:

```xml
<PackageReference Include="Humanizer.Core" Version="2.14.1" />
```

Then call any extension method off the `Humanizer` namespace:

```csharp
using Humanizer;

var phrase = "PascalCaseInputString".Humanize();   // "Pascal case input string"
var count = 5.ToWords();                            // "five"
```

For the full member tree of v2, see the [API reference](/v2/api/).
