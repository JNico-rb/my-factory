# Repo de aprendizaje

Este repo sirve para aprender conceptos generales. Cada pregunta es una **lección**: el objetivo es entender el concepto con hechos verificables.

## Fuentes

Responde desde la **fuente primaria**: documentación oficial del proyecto o producto, la especificación o estándar (RFC, ECMA, W3C, ISO…) o el código fuente. Consúltala en cada respuesta con WebFetch/WebSearch, también cuando creas conocer ya la respuesta.

## Entorno del usuario

Jaime usa Claude Code como **extensión de VS Code** en Windows. Explica cada paso para ese entorno: el panel de la extensión, sus botones y los comandos `/` del chat.

- Lo que solo existe en el CLI (`claude --bg`, `claude agents`, `claude attach`, `claude --resume`…) se lo das como comando para el terminal integrado de VS Code (`` Ctrl+` ``), en PowerShell 7.
- Si no sabes cómo se hace algo en la extensión, mira antes la documentación oficial: https://code.claude.com/docs/en/vs-code (índice de páginas: https://code.claude.com/docs/llms.txt).

## Skills en beta (sandbox)

Este repo es también el sandbox para probar las skills `in-progress` de `mattpocock/skills`, copiadas en `.claude/skills/` desde el commit `6f7f783` (2026-11-26). No se actualizan solas. Solo se invocan a mano con `/<nombre>`.

Dónde guarda cada una su trabajo:

- `/loop-me`: specs en `workflows/*.md` y notas del mundo del usuario en `NOTES.md`.
- `/writing-fragments`: fragmentos en `escritura/fragmentos/<tema>.md`.
- `/writing-shape` y `/writing-beats`: leen de `escritura/fragmentos/` y escriben artículos en `escritura/articulos/<tema>.md`.
- `/chief-of-staff`: su wiki es `wiki/` (índice en `wiki/index.md`).
- `/claude-handoff`, `/software-factory`: no escriben en el repo.

Las skills que referencian otras con rutas relativas (`../retro/SKILL.md`, `../to-spec/SKILL.md`, `../to-tickets/SKILL.md`, `../implement-spec/SKILL.md`, `/grilling`, `codebase-design`) las encuentran en el plugin `mattpocock-skills`: invócalas con la herramienta Skill como `mattpocock-skills:<nombre>`.

## Repos guardados

`UTILITIES.md` cataloga por necesidad los repos con estrella de Jaime (skills, herramientas, cursos), con frescura y veredicto. Consúltalo antes de proponer una herramienta externa; sus reglas de mantenimiento están al principio del fichero.

## Mods de Claude Code

Los mods propios viven en `mods/<nombre>/`, uno por carpeta, y este repo es su marketplace local (`.claude-plugin/marketplace.json`, nombre `my-factory`). Se instalan como plugins (`<nombre>@my-factory`) y se encienden o apagan en `enabledPlugins` de `~/.claude/settings.json`. Claude Code los lee desde aquí: edita en `mods/`, no en la copia de `~/.claude`. Cómo añadir uno: `mods/README.md`.

## Agent skills

### Issue tracker

Issues y specs en markdown local, bajo `.scratch/<feature>/`. See `docs/agents/issue-tracker.md`.

### Triage labels

Las cinco etiquetas canónicas por defecto (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: `GLOSSARY.md` y `docs/adr/` en la raíz. See `docs/agents/domain.md`.
