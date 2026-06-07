# Contributing to Tomb Launcher

Thank you for your interest in contributing! Here's everything you need to know.

## Who can contribute
Whether you're a newcomer or a seasoned developer, all contributors are welcome.

For newcomers in particular, you're welcome to work on one of the issues marked as "good first issue", but I suggest you get in touch with me before starting to write code.

I expect contributors to have a basic understanding of object-oriented programming — enough to understand technical feedback during code review.

The only limit I put on contributions are PRs that are entirely AI-generated. Don't even bother opening one. I can tell when you do.

## Review timeline

Tomb Launcher is a hobby project maintained in spare time — reviews may take a few days to a week. Please don't bump your PR if you haven't heard back within a couple of days.

---

## Getting started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

**IDE**: JetBrains Rider or Visual Studio. Development using VS Code is possible but you won't have access to an Avalonia extension (see below).

**Extensions**: For Rider users, [AvaloniaRider](https://plugins.jetbrains.com/plugin/14839-avaloniarider). For Visual Studio Users, [AvaloniaVS](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaVS) 

```bash
git clone https://github.com/ALDamico/TombLauncher.git
cd TombLauncher
dotnet build TombLauncher.slnx
dotnet run --project src/TombLauncher
```

Run the tests:
```bash
dotnet test
```

---

## What's welcome

- **Bug fixes** — always welcome, no prior discussion needed
- **New download sources** — community sites that host Tomb Raider custom levels
- **Translations** — see [Adding a language](#adding-a-language) below
- **Feature improvements** — enhancements to existing features; open an issue first for anything non-trivial

**Please don't open PRs for:**
- macOS support
- Features that haven't been discussed in an issue first
- New heavy dependencies without prior agreement
- AI features: Laura is a niche experimental feature and expanding it is not a current priority.

When in doubt, open an issue before writing code.

---

## Workflow

Open your pull request against `develop` — **never target `master` directly**.

```
feature/your-feature → (PR) → develop → (release) → master
```

---

## Pull request descriptions

For bug fixes and small changes, a single sentence explaining *why* is enough.

For more significant features, include:
- What the feature does and why it's useful
- Any design decisions worth highlighting
- How to test it manually

---

## Commit messages

This project uses [Conventional Commits](https://www.conventionalcommits.org/).

```
type(scope): short description (#issue)

- What changed and why
```

Common types: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`

Keep the subject under 72 characters.

---

## Adding a language

Translations are the easiest way to contribute and don't require deep knowledge of the codebase.

> **Note:** The existing translations for French, Spanish, German, Polish, and Czech were generated with an LLM. Human-reviewed corrections are especially welcome for these languages.

**Steps:**

1. Copy `src/TombLauncher.Localization/Localization/en-US.axaml` to a new file named after your locale (e.g. `pt-PT.axaml`).
2. Translate the string values, keeping the `x:Key` attributes unchanged.
3. Open a PR with a brief description of the language added.

The app discovers language files automatically from the `Localization` folder — no registration needed.

---

## Tests

Don't add tests just to have tests — a test that doesn't fail under any realistic broken implementation isn't useful.

Some areas (e.g. code that reads game executables) may not be unit-testable due to licensing constraints. In those cases, mention it in the PR description.

---

## Code conventions

- Language: English (class names, methods, variables)
- All user-facing strings must be localized — add keys to **all** language files
- UI icons: `PackIconRemixIconKind` (Remix Icon set)
- No linting/formatting tooling — this project follows the [.NET Coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- `<Nullable>enable</Nullable>` is set on all projects
- Primary constructors are fine for data classes/records with 5 or fewer public properties. If a data class has more than 5 properties, declare them in the classic style. Anything else should *never* use primary constructors.
- DI registrations: services, viewmodels and everything that belongs to the DI mechanism *must* be registered in the relevant extension methods. Never register dependencies directly in `App.axaml.cs`. The existing methods should cover all bases already. If you believe the addition of a new extension method is warranted, please get in touch first.
- MVVM: You must use a clean MVVM separation when writing UI code. 
    - Do not mix UI responsibilities with application logic. 
    - Avoid code-behind in view code when it's not strictly necessary.
    - Prefer the use of converters instead of transforming values in the ViewModel.
    - The project uses CommunityToolkit.Mvvm. Observable properties should be implemented as partial properties annotated with the ObservableProperty attribute. Commands should be implemented as private methods annotated with the RelayCommand attribute.

