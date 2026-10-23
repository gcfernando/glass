---
layout: default
title: "Glass.Message — Documentation and Guides"
description: "A modern Windows Forms MessageBox library with async dialogs, inputs, progress bars and toast notifications."
permalink: /documentation/
---

# Glass.Message Documentation

Find installation, WinForms dialog examples and the source contracts behind Glass.Message.

<a id="explore-more"></a>
<a id="markdown-source-library"></a>
<a id="read-online"></a>

## 🧭 Choose a guide

| Task | Guide |
|---|---|
| Add the library to a Windows Forms app | [Quick start](README.md#-quick-start) and [installation](README.md#-installation) |
| Migrate MessageBox calls safely | [Overview and argument ordering](README.md#-the-one-line-pitch) |
| Use inputs, async dialogs, progress or toasts | [Usage guide](README.md#-complete-usage-guide) and [API overview](README.md#-api-overview) |
| Customize themes and defaults | [Configuration](README.md#-configuration) and [styling](README.md#-styling-and-customization) |
| Build and verify changes | [Development and testing](README.md#-running-the-demo) and [project requirements](README.md#-requirements) |
| Read the package-facing guide | [NuGet README](README.nuget.md) |
| Review historical release changes | [Changelog](CHANGELOG.md) |

## 🔗 Source references

[Public facade](Glass.Message/GlassMessage.cs) · [Fluent builder](Glass.Message/GlassBuilder.cs) · [Progress controller](Glass.Message/GlassProgressController.cs) · [Release workflow](.github/workflows/release.yml)

Dialog creation belongs on the WinForms UI thread. The progress controller supports marshaled worker updates; this does not make all dialog APIs callable from arbitrary threads.

## 🌐 Published documentation

This index is also rendered by the repository's GitHub Pages site. Relative links above point to the authored repository documents; package READMEs target NuGet readers and may use absolute GitHub links.

[Repository](https://github.com/gcfernando/glass) · [Project overview](README.md)
