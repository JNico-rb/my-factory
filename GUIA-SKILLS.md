# Guía de onboarding: skills de Matt Pocock

Fuente: los `SKILL.md` del plugin `mattpocock-skills` 1.3.1 y de `.claude/skills/`. El mapa oficial de cómo se combinan está en `/mattpocock-skills:ask-matt`. Si dudas qué skill toca, pregúntale a esa skill.

## 1. El modelo mental en 1 minuto

Casi todo el stack es **un flujo principal** de idea a código, con atajos para entrar en él:

```
/grill-with-docs ──► (/prototype si hay dudas que solo se resuelven con código)
       │
       ├─ trabajo pequeño ─► /implement  (usa /tdd y acaba con /code-review)
       │
       └─ trabajo grande ──► /to-spec ─► /to-tickets ─► /implement por ticket (con /clear entre tickets)
                                                     └► /implement-spec  (todos en paralelo con subagentes)
                                                              │
                                                       /pr ─► /retro
```

Atajos de entrada:

- **`/triage`**: issues que llegan en bruto, como un bug que te reporta otra persona.
- **`/diagnosing-bugs`**: algo roto que no se ve a la primera.
- **`/wayfinder`**: un proyecto tan grande y difuso que no cabe en una sesión.

Glosario mínimo:

- **Grilling**: entrevista implacable. El agente averigua los hechos y tú tomas las decisiones. Cada pregunta trae una respuesta recomendada; contesta "sí" para aceptarla.
- **`GLOSSARY.md` y ADRs**: la memoria del proyecto. Las crea `/grill-with-docs` sobre la marcha, en la raíz y en `docs/adr/`.
- **Tracer-bullet ticket**: un ticket que atraviesa todas las capas de punta a punta, en lugar de un ticket por capa. Cada uno declara qué tickets lo bloquean.
- **Tracker local**: en este repo los tickets son ficheros en `.scratch/<feature>/issues/NN-<slug>.md`. Lo configura [docs/agents/issue-tracker.md](docs/agents/issue-tracker.md).

Casi todas se lanzan **solo a mano**: escribes `/nombre` y el agente no las invoca por su cuenta. Las del plugin llevan prefijo (`/mattpocock-skills:to-spec`). Las beta de `.claude/skills/` no (`/loop-me`).

## 2. Preparación (una vez)

Ejecuta en PowerShell, desde la raíz del repo:

```powershell
python -m pip install pytest
git add -A
git commit -m "Base: skills en beta y configuración de Matt Pocock"
```

- `pytest`: `/tdd` necesita un runner de tests. No tienes Node, así que los proyectos de prueba van en Python.
- El commit: `/code-review` revisa los cambios desde un commit fijo, y `/implement-spec` trabaja con ramas y worktrees. Ninguna de las dos funciona sin un commit base.

**Choque de nombres:** `/teach` y `/loop-me` usan los dos un `NOTES.md` en el directorio donde se lanzan. Lanza `/loop-me` desde la raíz. Para `/teach`, abre una sesión aparte en una subcarpeta (ver proyecto 6).

## 3. Proyectos de prueba, en orden

Cada proyecto prueba un flujo. Están ordenados para que cada uno reutilice lo que dejó el anterior.

### Proyecto 0: el router (5 min)

```
/mattpocock-skills:ask-matt Quiero hacer una CLI pequeña en Python para llevar los libros que presto a amigos. ¿Por dónde empiezo?
```

**Qué observar:** te recomienda una ruta por el flujo principal. Úsala cuando no sepas qué skill toca.

### Proyecto 1: flujo principal completo, «Préstamos de libros»

Una CLI en Python para registrar libros prestados, a quién y cuándo vencen. Es pequeña, pero tiene vocabulario de dominio con miga: préstamo, devolución, vencido, renovación.

1. `/mattpocock-skills:grill-with-docs Quiero una CLI en Python para registrar libros que presto a amigos`
   - Contesta las rondas de preguntas. **Observa** cómo aparecen `GLOSSARY.md` y alguna ADR en `docs/adr/`.
2. `/mattpocock-skills:to-spec`, **sin limpiar el contexto**. Genera `.scratch/<feature>/spec.md`.
3. `/mattpocock-skills:to-tickets`. Genera `.scratch/<feature>/issues/01-….md`, `02-…` y siguientes, cada uno con su `Blocked by:`.
4. Por cada ticket, empezando por los que no tienen bloqueos:
   `/clear` y después `/mattpocock-skills:implement .scratch/<feature>/issues/01-<slug>.md`
   - **Observa** el ciclo de `/tdd`: un test en rojo, el código mínimo para ponerlo en verde, y vuelta a empezar. Al final, `/code-review` revisa en dos ejes: si sigue los estándares del repo y si cumple la spec.
5. `/mattpocock-skills:retro`, en la misma sesión del último ticket. Propone cambios en el **entorno** (checks automáticos, punteros, estándares), no en el código.

**Variante paralela:** repite los pasos 1 a 3 con otra funcionalidad, por ejemplo «avisos de préstamos vencidos». Después lanza `/mattpocock-skills:implement-spec .scratch/<feature>/spec.md` y observa cómo reparte los tickets entre subagentes, cada uno en su propio worktree, y los integra en una rama.

### Proyecto 2: entrar por los atajos (bugs e issues)

Sobre el código del proyecto 1:

- **`/triage`:** crea a mano un issue «sucio», como si te lo hubiera reportado otra persona. Por ejemplo, `.scratch/bugs/issues/01-fechas.md` con el texto «a veces sale vencido un libro que presté ayer», sin más detalle. Lanza `/mattpocock-skills:triage` y observa cómo lo verifica, lo clasifica, te pregunta lo que falta y lo deja listo para un agente (`ready-for-agent`). Después, `/implement` sobre ese ticket.
- **`/diagnosing-bugs`:** mete a propósito un bug sutil, por ejemplo un off-by-one en el cálculo de días con cambio de mes. Dile «diagnostica: los préstamos de fin de mes vencen un día antes». **Observa** que no lanza teorías hasta tener un comando que reproduzca el fallo en rojo. Remata con `/retro`.

### Proyecto 3: `/prototype` + `/handoff`

Una pregunta de diseño que no se resuelve hablando: «¿qué estados tiene un préstamo y qué transiciones hay?».

1. En medio del proyecto 1, lanza `/mattpocock-skills:handoff prototipar la máquina de estados del préstamo`.
2. Abre una sesión nueva, pásale ese fichero y lanza `/mattpocock-skills:prototype`. Hace código desechable en un directorio aparte.
3. Lanza `/handoff` con lo aprendido de vuelta a la sesión original.

### Proyecto 4: `/wayfinder` (opcional, el más pesado)

Solo para algo que de verdad sea grande y difuso. Por ejemplo: «un sistema que cada semana recopile lo que he aprendido en este repo, me haga un resumen y me examine».

`/mattpocock-skills:wayfinder`. Produce un **mapa** (`.scratch/<esfuerzo>/map.md`) con tickets de **decisión**, no de construcción. Se resuelven uno a uno. Cuando se despeja la niebla, se pasa a `/to-spec`.

### Proyecto 5: las skills beta

| Skill                  | Prueba concreta                                                                                                                                                                                      | Con qué la combinas                                                                           |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| `/loop-me`           | `/loop-me` sin argumentos: te entrevista sobre tu semana y detecta tareas repetitivas que se podrían delegar. Escribe en `NOTES.md` y `workflows/*.md`.                                       | Un workflow ya cerrado puede pasar por `/to-spec` → `/to-tickets` para construirlo.       |
| `/writing-fragments` | `/writing-fragments escritura/fragmentos/skills.md` sobre «qué me ha sorprendido probando estas skills».                                                                                        | Material de partida: las salidas de `/retro` de los proyectos 1 y 2.                         |
| `/writing-shape`     | `/writing-shape escritura/fragmentos/skills.md`: te pregunta dónde guardar el artículo; dile `escritura/articulos/skills.md`. Discute el formato de cada párrafo.                             | Sale del montón que dejó `/writing-fragments`.                                             |
| `/writing-beats`     | Lo mismo con el mismo montón, pero eligiendo "beats" al estilo «elige tu propia aventura». Compara el artículo que sale con el de `/writing-shape`.                                            | Igual que `/writing-shape`.                                                                  |
| `/chief-of-staff`    | `/chief-of-staff Objetivo: dejar la CLI de préstamos terminada, con avisos y exportación a CSV`. Delega en subagentes y guarda la memoria en `wiki/`.                                          | Dentro usa `retro`, `to-spec`, `to-tickets` e `implement-spec`, que vienen del plugin. |
| `/claude-handoff`    | Al final de una sesión larga: `/claude-handoff seguir con el ticket 03`. Lanza `claude --bg` y lo ves con `claude agents` en PowerShell.                                                      | Es la alternativa en segundo plano a `/handoff`.                                             |
| `/software-factory`  | Junto con `/mattpocock-skills:grill-me`: diseña sobre el papel una factoría de agentes para tu equipo (Azure DevOps cubre tickets, git, runners y orquestador). Solo diseño, no construye nada. | Usa `/grill-me` para las preguntas.                                                          |

### Proyecto 6: aprendizaje y utilidades sueltas

- **`/teach`**, en una sesión aparte. Ejecuta esto en PowerShell:

  ```powershell
  New-Item -ItemType Directory -Force "C:\Users\jaime.rodriguez\my-projects\learning\aprender\git-interno"
  Set-Location "C:\Users\jaime.rodriguez\my-projects\learning\aprender\git-interno"
  claude
  ```

  Después lanza `/mattpocock-skills:teach cómo funciona git por dentro`. Crea `MISSION.md`, lecciones en HTML y registros de lo aprendido. Es la que mejor encaja con este repo.
- **`/research`**: `/mattpocock-skills:research qué diferencia hay entre plugin y skill suelta en Claude Code`. Lo investiga un agente en segundo plano con fuentes primarias y deja un `.md` con citas.
- **`/wait-what`**: lánzalo cuando una respuesta te suene a jerga. Te la repite en lenguaje claro.
- **`/to-questionnaire`**: cuando la decisión depende de otra persona, por ejemplo de tu equipo. Te genera un cuestionario para mandárselo.
- **`/improve-codebase-architecture`**: al terminar el proyecto 1, sobre la CLI. Genera un informe HTML con oportunidades de "profundizar" módulos.

## 4. Consejos

- **Higiene de contexto:** mantén `/grill-with-docs`, `/to-spec` y `/to-tickets` en la misma ventana. Haz `/clear` entre cada `/implement`.
- **Después de cada proyecto, haz commit.** Así `/code-review` tiene un punto fijo con el que comparar, y tú puedes deshacer lo que salga mal.
- Las skills beta pueden cambiar o desaparecer. Apunta lo que falla: el autor pide feedback explícitamente.
