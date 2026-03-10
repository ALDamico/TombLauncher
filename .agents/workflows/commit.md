---
description: How to commit changes following project conventions
---

# Commit

> **IMPORTANT**: NEVER commit unless the user explicitly asks you to. Do NOT commit proactively after making changes — always wait for the user to say "commit", "committa", or similar.

The project uses **Conventional Commits**.

## Format

```
<type>(<optional scope>): <short description>

[optional body]
```

## Types

| Type | Usage |
|------|-------|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Refactoring without behavior change |
| `style` | Aesthetic change (UI/CSS) |
| `docs` | Documentation |
| `test` | Adding or modifying tests |
| `chore` | Maintenance, dependencies, configuration |

## Common scopes

`notifications`, `navigation`, `search`, `settings`, `ui`, `data`, `installer`, `localization`

## Examples

```
feat(search): add filter by game engine
fix(notifications): resolve animation crash on dismiss
refactor(navigation): simplify NavigationManager locking
```

## Steps

1. Check status:
```bash
git status
```

2. Stage changes:
```bash
git add -A
```

3. Commit:
```bash
git commit -m "type(scope): description"
```
