![Utilities.Net](utilities-dotnet.png)
![coverage badge](https://github.com/StirlingLabs/Utilities.Net/blob/coverage/coverage/badge_combined.svg) [![CodeFactor](https://www.codefactor.io/repository/github/stirlinglabs/utilities.net/badge?s=016763d52aea57b89a59fe809f85552eb1f08055)](https://www.codefactor.io/repository/github/stirlinglabs/utilities.net)

### Common utilities used throughout the Stirling Labs C# codebase

## 🚀 How to install

If you've [configured GitHub Packages](/StirlingLabs/Logging/blob/master/docs/GitHubPackages.md), you can just:

```bash
> dotnet add PROJECT package StirlingLabs.Utilities
```

or just use NuGet however you would normally.

## 👀 What's included

Any stand-alone code that is used in more than one package is a contender to be included here. If you see something that you think should be included, please
[create an issue](/StirlingLabs/Utilities.Net/issues/new) or PR so we can discuss it.

## 🐣 Lifecycle

This should be the first place that common code is generalised. If it turns out that the implementation should be separated for some reason (licensing or some other optimisation) then it should be fully moved to the new package and then included back here via NuGet.  Users of Utilities.Net should not have to change their code during this process (if namespaces absolutely *have* to be modified, provide aliases).

If the reason for separating a module out ends, it should be *moved* back into Utilities as soon as it can be safely achieved (and the unecessary repo archived).  At no point should there be two code repositories being maintained; a single source of truth must exist at all times.
