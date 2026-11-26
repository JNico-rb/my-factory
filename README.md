# my-factory

Repo personal de aprendizaje y experimentación con [Claude Code](https://code.claude.com/docs) (extensión de VS Code, Windows).

Sirve para tres cosas:

1. **Lecciones.** Cada pregunta es una lección que se responde desde la fuente primaria (documentación oficial, especificación o código fuente). Las reglas están en [`CLAUDE.md`](CLAUDE.md).
2. **Sandbox de skills.** Aquí se prueban las skills en beta de [`mattpocock/skills`](https://github.com/mattpocock/skills), copiadas en [`.claude/skills/`](.claude/skills/).
3. **Mods propios.** Plugins de Claude Code en [`mods/`](mods/). El repo es su marketplace local (`my-factory`).

## Qué hay dentro

| Ruta | Contenido |
|---|---|
| [`CLAUDE.md`](CLAUDE.md) | Instrucciones del proyecto para Claude: fuentes, entorno y convenciones |
| [`GUIA-SKILLS.md`](GUIA-SKILLS.md) | Guía de onboarding de las skills de Matt Pocock: el flujo de idea a código |
| [`UTILITIES.md`](UTILITIES.md) | Catálogo de repos guardados (skills, herramientas, cursos) por necesidad, con veredicto |
| [`claude-code-terminal/`](claude-code-terminal/) | Guía del uso de Claude Code desde el terminal |
| [`.claude/skills/`](.claude/skills/) | Skills en beta: `chief-of-staff`, `claude-handoff`, `loop-me`, `software-factory`, `writing-fragments`, `writing-shape`, `writing-beats` |
| [`mods/`](mods/) | Mods propios. Hoy, `foundry-gasto`: panel con el gasto de Azure AI Foundry y límites que cortan al llegar |
| [`workflows/`](workflows/) | Specs de flujos de trabajo (`/loop-me`) |
| [`escritura/`](escritura/) | Fragmentos y artículos (`/writing-fragments`, `/writing-shape`, `/writing-beats`) |
| [`wiki/`](wiki/) | Wiki de `/chief-of-staff` |
| [`docs/agents/`](docs/agents/) | Convenciones para agentes: issues, etiquetas de triage y documentación de dominio |

## Puesta en marcha

Requisitos: VS Code con la extensión de Claude Code, Git para Windows y PowerShell 7.

```powershell
git clone https://github.com/JNico-rb/my-factory.git
cd my-factory
code my-factory.code-workspace
```

Las skills de `.claude/skills/` se cargan solas al abrir el proyecto y se invocan a mano desde el chat con `/<nombre>`.

### Instalar un mod

Con el repo abierto, añade el marketplace e instala el plugin desde el chat de Claude Code:

```
/plugin marketplace add ./
/plugin install foundry-gasto@my-factory
```

Se enciende y apaga en `enabledPlugins` de `%USERPROFILE%\.claude\settings.json`. Detalles y avisos de cada mod: [`mods/README.md`](mods/README.md).

## Notas

- Las skills en beta no se actualizan solas: son una copia fija de `mattpocock/skills` (commit indicado en `CLAUDE.md`).
- No se versionan secretos ni configuración personal: `.env*` y `.claude/settings.local.json` están en `.gitignore`.
