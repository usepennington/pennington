---
title: "Why I Switched to Linux for .NET Development"
date: 2026-04-01
author: "Alex Chen"
description: "My experience moving from Windows to Fedora for daily .NET development"
tags: ["linux", "workflow", "dotnet"]
---

After years on Windows, I switched my daily development machine to Fedora. Here's why and what I learned.

## The Motivation

Honestly? Docker performance. Running containers on Linux is noticeably faster than Docker Desktop on Windows. For a microservices project, that difference adds up fast.

## What Worked Great

- .NET SDK installs cleanly and works perfectly
- JetBrains Rider is excellent on Linux
- Git operations are faster
- Terminal experience is first-class (no more Windows Terminal workarounds)

## What Took Adjustment

- Some .NET MAUI tooling is Windows/Mac only
- Had to find alternatives for a few Windows-specific tools
- Font rendering is different (better, once you configure it)

> [!TIP]
> If you're not ready for a full switch, WSL2 gives you most of the Linux benefits while staying on Windows. The .NET SDK works great in WSL2.

Overall, I'm happy with the switch and don't plan to go back.
