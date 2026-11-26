# UTILITIES — repos guardados, por necesidad

Catálogo de los repos con estrella en [github.com/nico-rb?tab=stars](https://github.com/nico-rb?tab=stars), ordenados por **para qué sirven**. Se mantiene a mano: las estrellas nuevas no aparecen solas.

**Última revisión:** jueves 19/11/2026 · 11:08 (Europe/Madrid) — 33 repos.

## Cómo leer y mantener este fichero

Cada entrada:

```
- **[nombre](url)** · `tipo` — qué es.
  Úsalo cuando: …
  ⭐ estrellas · último commit AAAA-MM-DD <frescura> · <veredicto>
  > notas con fecha (solo en 🧪, ⏸️, ❌)
```

**Frescura** (último commit en la rama por defecto, respecto a la fecha de revisión):
🟢 activo, ≤ 30 días · 🟡 lento, 31–180 días · 🔴 parado, > 180 días · 🗄️ archivado.

**Veredicto** (relación de Jaime con el repo):
✅ lo uso / instalado · 🧪 probado · 📌 pendiente de probar · ⏸️ aplazado · ❌ descartado (por ahora).

**Reglas para el agente que actualice este fichero:**

- 🧪, ⏸️ y ❌ llevan siempre una nota con fecha que explique **por qué** y qué recordar (ej.: `> ⏸️ 2026-11-19 — no soporta Windows; revisar si sacan versión nativa`). Si falta, pregunta a Jaime antes de escribirla.
- Las notas se **acumulan**: añade la nueva debajo, no borres las anteriores.
- "Qué es" y "Úsalo cuando" salen del README del repo, no de la descripción corta de GitHub.
- Para refrescar: consulta `GET https://api.github.com/users/nico-rb/starred?per_page=100` con la cabecera `Accept: application/vnd.github.star+json` (da `starred_at`, `stargazers_count`, `pushed_at`, `archived`). Repos nuevos → entrada con 📌 en su categoría; actualiza estrellas, fecha, frescura y la línea de **Última revisión**.
- Si dejas de tener estrella en un repo, no borres la entrada: muévela a [Fuera de estrellas](#fuera-de-estrellas) con su nota.

## Índice

1. [Ingeniería de software con agentes](#1-ingeniería-de-software-con-agentes)
2. [Diseño web y UI](#2-diseño-web-y-ui)
3. [Diagramas y visuales](#3-diagramas-y-visuales)
4. [Vídeo, voz y contenido](#4-vídeo-voz-y-contenido)
5. [Contexto, tokens y conocimiento del código](#5-contexto-tokens-y-conocimiento-del-código)
6. [Harness y gestión de agentes](#6-harness-y-gestión-de-agentes)
7. [Aprender](#7-aprender)
8. [Otros](#8-otros)

---

## 1. Ingeniería de software con agentes

Metodologías, flujos y calidad del código que escribe el agente.

- **[mattpocock/skills](https://github.com/mattpocock/skills)** · `skills / plugin` — skills pequeñas y componibles para ingeniería real (spec, tickets, TDD, diagnóstico, code review, grilling). Predomina el humano en el bucle (HITL): tú decides, el agente ejecuta.
  Úsalo cuando: quieras control fino del proceso sin adoptar un framework que lo imponga.
  ⭐ 327.7k · último commit 2026-11-18 🟢 · ✅ instalado (plugin `mattpocock-skills`)

- **[addyosmani/agent-skills](https://github.com/addyosmani/agent-skills)** · `skills / plugin` — skills de calidad "de producción" con 10 comandos que siguen el ciclo de vida: `/spec`, `/plan`, `/build`, `/test`, `/review`, `/webperf`, `/a11y`, `/ship`…
  Úsalo cuando: quieras un ciclo completo guiado por comandos, con foco en tests, rendimiento web y accesibilidad.
  ⭐ 119.0k · último commit 2026-11-18 🟢 · 📌

- **[bmad-code-org/BMAD-METHOD](https://github.com/bmad-code-org/BMAD-METHOD)** · `framework` — método ágil de desarrollo con IA de principio a fin (brief → spec → arquitectura → build) que se adapta al tamaño del trabajo. Estable v6.12; v7 en alfa con cambios frecuentes.
  Úsalo cuando: quieras que el proceso lo marque el método (lo contrario de mattpocock/skills). Requiere Node 20.12+, Python 3.10+ y `uv`.
  ⭐ 55.5k · último commit 2026-11-19 🟢 · 📌

- **[DietrichGebert/ponytail](https://github.com/DietrichGebert/ponytail)** · `skill` — hace que el agente escriba solo el código necesario ("el senior vago"), sin recortar validación, errores, seguridad ni accesibilidad. Publica benchmarks propios (−53 % código, −45 % tokens).
  Úsalo cuando: el agente genere código inflado o sobreingeniería.
  ⭐ 194.5k · último commit 2026-10-29 🟢 · 📌

- **[cloudflare/security-audit-skill](https://github.com/cloudflare/security-audit-skill)** · `skill` — auditoría de seguridad en 6 fases con agentes aislados: reconocimiento, búsqueda guiada por cobertura, verificación independiente de cada hallazgo y salida en JSON validado.
  Úsalo cuando: necesites auditar un repo con hallazgos verificados, no una lista de sospechas.
  ⭐ 75.5k · último commit 2026-09-14 🟡 · 📌

- **[morluto/rea](https://github.com/morluto/rea)** · `MCP` — "Reverse Engineer Anything": MCP de ingeniería inversa, desde el comportamiento de una app hasta binarios nativos, sin el código fuente.
  Úsalo cuando: quieras entender cómo funciona una funcionalidad de otra app para replicarla. Instala solo desde fuentes oficiales (el README avisa de copias).
  ⭐ 469.0k · último commit 2026-11-19 🟢 · 📌

## 2. Diseño web y UI

Para que las webs que genera el agente no parezcan hechas por IA.

- **[vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills)** · `skills` — colección oficial de Vercel: `react-best-practices` (más de 40 reglas de rendimiento React/Next), `web-design-guidelines` (revisión de UI y accesibilidad) y `vercel-optimize` (coste y rendimiento de un proyecto en Vercel).
  Úsalo cuando: escribas o revises React/Next.js, o audites una UI.
  ⭐ 34.5k · último commit 2026-11-12 🟢 · ✅ instalado (`react-best-practices`, `web-design-guidelines`)

- **[pbakaus/impeccable](https://github.com/pbakaus/impeccable)** · `skill` — vocabulario de diseño para el agente: dirigir con `polish` / `bolder`, revisar con `critique` / `audit` y detectar el "AI slop" (Inter, degradados morados, tarjetas dentro de tarjetas).
  Úsalo cuando: quieras iterar el diseño de una página dando órdenes de estilo.
  ⭐ 113.1k · último commit 2026-11-18 🟢 · 📌

- **[Nutlope/hallmark](https://github.com/Nutlope/hallmark)** · `skill` — skill de diseño de Together AI con 50 temas y verificaciones en un navegador real contra los rasgos típicos de una web hecha por IA, en niveles (Floor / Reflex / Finish), con un revisor final de contexto limpio.
  Úsalo cuando: quieras una página con un estilo concreto y garantías mínimas (contraste, móvil, foco).
  ⭐ 57.7k · último commit 2026-11-11 🟢 · 📌

- **[Leonxlnx/taste-skill](https://github.com/Leonxlnx/taste-skill)** · `skill` — webs corporativas a nivel figma.com / x.ai: un sistema de diseño, fotos reales, microinteracciones y comprobación en navegador. Requiere Node 22+ y Chrome/Edge.
  Úsalo cuando: necesites una web de empresa o landing con acabado profesional.
  ⭐ 118.6k · último commit 2026-11-08 🟢 · 📌

- **[GoogleChrome/modern-web-guidance](https://github.com/GoogleChrome/modern-web-guidance)** · `skill / CLI` — buenas prácticas web actuales y compatibilidad de navegadores, mantenidas por los equipos de Chrome, Edge, Firefox y MDN, para que el agente no use patrones antiguos.
  Úsalo cuando: el agente meta JavaScript donde hoy hay una API nativa de la plataforma web.
  ⭐ 23.1k · último commit 2026-11-17 🟢 · 📌

## 3. Diagramas y visuales

- **[tt-a1i/archify](https://github.com/tt-a1i/archify)** · `skill` — convierte una idea, un plan o un repo en un HTML interactivo explorable (mapas de arquitectura con enlaces al código, itinerarios, mapas de aprendizaje).
  Úsalo cuando: quieras **explorar** algo de forma interactiva, por ejemplo el mapa de un codebase.
  ⭐ 97.9k · último commit 2026-11-19 🟢 · 📌

- **[cathrynlavery/diagram-design](https://github.com/cathrynlavery/diagram-design)** · `skill / plugin` — 51 tipos de diagrama editorial en un único HTML + SVG, con la paleta y las tipografías de tu marca (sin Mermaid).
  Úsalo cuando: necesites un diagrama **para presentar o publicar** (arquitectura, secuencia, cuadrante).
  ⭐ 68.3k · último commit 2026-11-19 🟢 · 📌

## 4. Vídeo, voz y contenido

- **[latent-spaces/brag](https://github.com/latent-spaces/brag)** · `skill / plugin` — `/brag` convierte tu proyecto en un vídeo de lanzamiento corto con música, animación y texto para compartir. En Opus/Sonnet 5.5 y Fable 5.2 pasa solo a `/brag-slim`, que no necesita Hyperframes.
  Úsalo cuando: quieras enseñar algo que has construido.
  ⭐ 22.0k · último commit 2026-11-15 🟢 · ✅ instalado (`brag`, `brag-slim`)

- **[heygen-com/hyperframes](https://github.com/heygen-com/hyperframes)** · `framework / app` — HTML + CSS + animaciones → MP4 deterministas. Tiene CLI, skills para agentes y app de escritorio (Studio). Es el motor del `/brag` clásico.
  Úsalo cuando: quieras vídeo editable más allá de un lanzamiento (explicativos, demos).
  ⭐ 86.7k · último commit 2026-11-19 🟢 · 📌

- **[debpalash/VoiceStudio](https://github.com/debpalash/VoiceStudio)** · `app` — estudio de voz local, alternativa a ElevenLabs: clonar y diseñar voces, doblar vídeo, dictado, transcripción y narración. Expone una API local y MCP.
  Úsalo cuando: necesites voz o transcripción sin mandar audio a la nube.
  ⭐ 113.8k · último commit 2026-11-18 🟢 · 📌

- **[avi691/linkedin-agent-skill](https://github.com/avi691/linkedin-agent-skill)** · `skills` — 11 skills para llevar una cuenta de LinkedIn (posts, comentarios, respuestas, puntuación del perfil, plan semanal) más un "humanizer" que quita los rasgos de texto de IA. No publica nada sin tu ok.
  Úsalo cuando: quieras escribir en LinkedIn con tu voz.
  ⭐ 189 · último commit 2026-10-06 🟡 · 📌

## 5. Contexto, tokens y conocimiento del código

Para que el agente gaste menos y entienda mejor el repo.

- **[rtk-ai/rtk](https://github.com/rtk-ai/rtk)** · `CLI (Rust)` — proxy que comprime la salida de comandos (tests, diffs, logs, builds) antes de que la lea el agente: hasta un 90 % menos. `rtk gain` muestra el ahorro.
  Úsalo cuando: las salidas de comandos se coman el contexto. Es "instalar y olvidar".
  ⭐ 87.1k · último commit 2026-11-19 🟢 · 📌

- **[Graphify-Labs/graphify](https://github.com/Graphify-Labs/graphify)** · `skill / CLI` — `/graphify` convierte el proyecto (código, docs, PDFs) en un grafo de conocimiento que el agente consulta en vez de usar grep. El código se analiza en local con tree-sitter, sin LLM.
  Úsalo cuando: el repo sea grande y el agente pierda tiempo buscando.
  ⭐ 150.3k · último commit 2026-11-17 🟢 · 📌

- **[repowise-dev/repowise](https://github.com/repowise-dev/repowise)** · `MCP / dashboard` — indexa código, historial de git y tests en local. El agente consulta el índice (la mitad de llamadas) y tú tienes un dashboard de salud, riesgo y código muerto.
  Úsalo cuando: quieras lo mismo que graphify más analítica del repo. Elige uno de los dos, no ambos.
  ⭐ 8.2k · último commit 2026-11-19 🟢 · 📌

- **[muratcankoylan/Agent-Skills-for-Context-Engineering](https://github.com/muratcankoylan/Agent-Skills-for-Context-Engineering)** · `skills` — colección de skills sobre context engineering: curar el contexto, diseñar bucles de agente y evaluar su comportamiento.
  Úsalo cuando: diseñes tu propio agente o sistema multiagente.
  ⭐ 18.4k · último commit 2026-10-12 🟡 · 📌

## 6. Harness y gestión de agentes

Herramientas alrededor de Claude Code y otros agentes.

- **[vercel-labs/skills](https://github.com/vercel-labs/skills)** · `CLI` — `npx skills`: instalar, buscar, actualizar y quitar skills en más de 80 agentes (Claude Code, OpenCode, Codex, Cursor…). Catálogo en skills.sh.
  Úsalo cuando: instales una skill que no venga como plugin. Requiere Node 22.20+.
  ⭐ 40.0k · último commit 2026-11-17 🟢 · 📌

- **[jarrodwatts/claude-hud](https://github.com/jarrodwatts/claude-hud)** · `plugin` — status line para Claude Code: uso de contexto, límites de uso, herramientas activas, agentes y todos. Requiere Node 22.18+.
  Úsalo cuando: quieras ver qué hace la sesión sin preguntar.
  ⭐ 29.3k · último commit 2026-11-10 🟢 · 📌

- **[affaan-m/ECC](https://github.com/affaan-m/ECC)** · `plugin` — sistema de optimización del harness: skills, "instincts", memoria, seguridad e investigación, para 7 harnesses. Instala solo desde fuentes oficiales (plugin `ecc@ecc`).
  Úsalo cuando: quieras un harness muy cargado ya configurado. Choca con montar el tuyo pieza a pieza.
  ⭐ 306.8k · último commit 2026-11-17 🟢 · 📌

- **[HarnessRouter/harnessrouter](https://github.com/HarnessRouter/harnessrouter)** · `servicio self-hosted` — una sola API (compatible con OpenAI Responses) para usar Claude Code, Codex, OpenCode y otros 28 harnesses como backend de tu producto.
  Úsalo cuando: construyas un producto que lance agentes y quieras poder cambiar de harness.
  ⭐ 4.4k · último commit 2026-11-19 🟢 · 📌

## 7. Aprender

- **[shanraisshan/claude-code-best-practice](https://github.com/shanraisshan/claude-code-best-practice)** · `guía` — mapa de todas las piezas de Claude Code (subagentes, comandos, skills, hooks, MCP, plugins, memoria, settings) con buenas prácticas y enlaces.
  Úsalo cuando: dudes de dónde va algo o cómo se configura en Claude Code.
  ⭐ 69.2k · último commit 2026-11-19 🟢 · 📌

- **[walkinglabs/learn-harness-engineering](https://github.com/walkinglabs/learn-harness-engineering)** · `curso` — curso por proyectos sobre harness engineering (instrucciones, herramientas, entorno, estado, feedback), con análisis de cómo lo hacen Claude Code, Codex, Pi y DeepSeek.
  Úsalo cuando: quieras entender por qué funciona un harness, no solo usarlo.
  ⭐ 25.5k · último commit 2026-11-15 🟢 · 📌

- **[undefined-ui/second-brain-os](https://github.com/undefined-ui/second-brain-os)** · `curso + plugin` — curso de agentes en 7 módulos, 5 handbooks (grafos, gates, harnesses, loops, evals), un proyecto de "segundo cerebro" en markdown y 114 recursos revisados.
  Úsalo cuando: quieras una ruta guiada de AI engineering aplicada a agentes.
  ⭐ 1.6k · último commit 2026-11-03 🟢 · 📌

- **[rohitg00/ai-engineering-from-scratch](https://github.com/rohitg00/ai-engineering-from-scratch)** · `curso` — 542 lecciones en 20 fases: implementar desde cero las tripas de modelos, pipelines de recuperación y runtimes de agentes. Puede convertir tu agente en tutor.
  Úsalo cuando: quieras entender los fundamentos construyéndolos.
  ⭐ 79.6k · último commit 2026-11-18 🟢 · 📌

- **[HKUDS/DeepTutor](https://github.com/HKUDS/DeepTutor)** · `app` — tutor personalizado con IA: rutas de aprendizaje, cuadernos, evaluaciones, lecciones en vídeo y exportación a Anki. Tiene app de escritorio.
  Úsalo cuando: quieras estudiar un tema con seguimiento, fuera del editor.
  ⭐ 60.7k · último commit 2026-11-18 🟢 · 📌

- **[google-research/harness2](https://github.com/google-research/harness2)** · `investigación` — framework de Google Research para la mejora recursiva del harness: el agente refina su propio entorno por tarea y acumula lecciones, sin tocar los pesos del modelo.
  Úsalo cuando: quieras ver hacia dónde va el estado del arte en harnesses. Es investigación, no una herramienta diaria.
  ⭐ 24.4k · último commit 2026-11-02 🟢 · 📌

- **[guyernest/advanced-rag](https://github.com/guyernest/advanced-rag)** · `notebooks` — notebooks Jupyter del curso de edX sobre RAG avanzado: embeddings, chunking semántico, recuperación contextual, Reverse HyDE, búsqueda híbrida.
  Úsalo cuando: estudies RAG. Los conceptos siguen valiendo, pero el código puede haberse quedado antiguo.
  ⭐ 394 · último commit 2025-01-03 🔴 · 📌

## 8. Otros

- **[jarrodwatts/jev-trader](https://github.com/jarrodwatts/jev-trader)** · `ejemplo` — bot que decide comprar o vender en cada bloque de Monad (par MON-USDC en Kuru) y retransmite cada decisión a un dashboard por SSE. Sin clave privada corre en simulación.
  Úsalo cuando: quieras un ejemplo de agente en tiempo real con streaming de eventos.
  ⭐ 3.1k · último commit 2026-10-13 🟡 · 📌

## Fuera de estrellas

Repos de los que quitaste la estrella pero cuya nota merece la pena guardar. Vacío por ahora.
