---
name: asp-ft-code-style
description: "C# code style of the Aspid.FastTools repository beyond .editorconfig - using order, member order, sealed/internal/static, attribute and blank-line layout, named arguments, comments and XML docs. Use before writing or reviewing any .cs file of the package, the samples, the generators or the analyzers, and when asked to format code like the rest of the repo. For work on this repository only."
---

# C# code style

Rules beyond `.editorconfig`. They apply to the package, the samples, the generators and the analyzers. The samples
add their own rules in the asp-ft-samples skill.

## File

- `using` directives above the namespace, sorted by ascending line length.
- `// ReSharper disable once CheckNamespace` on the line above the namespace.
- Namespaces are block-scoped in Unity code and file-scoped in the generators and analyzers.
- One top-level type per file, named after the file.

## Member order

1. Constants.
2. `static readonly` fields.
3. Serialized fields.
4. Other instance fields.
5. Auto-properties.
6. Constructors.
7. Properties with a body, and abstract properties.
8. Unity messages, in lifecycle order.
9. Public methods.
10. Private methods.
11. Static methods.

Within a group, order declarations by ascending line length.

## Types and members

- Classes are `sealed` unless designed for inheritance.
- `internal` for implementation, `public` for API.
- Interface members are declared `public`.
- A method that uses no instance state is `static`.
- `#region` only groups overload families in large extension classes.

## Layout

- One attribute per bracket pair.
- Type and method attributes go on their own lines.
- On a field, `[SerializeField]` and parameterless or single-value inspector attributes stay on the field line. Other
  attributes go on the line above.
- An expression-bodied method puts its body on the next line. A property keeps it on the `=>` line.
- Ternary `?` and `:` branches each go on their own line.
- An object initializer passed as an argument spans multiple lines.
- Blank lines separate declarations from the code that uses them, a statement from the `if` that checks its result,
  and the final `return` from the code before it.

## Naming

- Name every argument of a multi-argument call. The first may stay positional when the method name implies it.
- Name a lone literal or `bool` argument.
- Lambda arguments stay positional.
- A local never shadows an inherited member.
- A `bool` is named as a question: `is…`, `has…`, `can…`.

## Comments

- No comments, except where code is hard to follow without one: non-obvious algorithms, workarounds, `unsafe`, Burst.
  They explain why, in English.
- Config files get short section headers only.
- Public package API has XML docs: `<summary>`, `<param>`, `<returns>`, `<see cref/>`. Nothing else does.
