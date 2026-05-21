---
title: Getting started (v1)
description: "Install Humanizer.Core 2.8.26 and humanize your first string."
uid: v1.getting-started
order: 20
---

Pin to the older major in your project file:

```xml
<PackageReference Include="Humanizer.Core" Version="2.8.26" />
```

Then call any extension method off the `Humanizer` namespace:

```csharp
using Humanizer;

var phrase = "PascalCaseInputString".Humanize();   // "Pascal case input string"
var count = 5.ToWords();                            // "five"
```

For the full member tree of v1, see the [API reference](/v1/api/).
