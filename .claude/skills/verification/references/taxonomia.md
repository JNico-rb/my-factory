# Taxonomía de verificación

Dos familias de técnicas y un marco de clasificación. Elige **una
técnica** y **una letra** por elemento verificado.

## 1. Verificación de producto (¿el código es correcto?)

- **Type checking** — comprobación automática de que los valores se usan
  de forma consistente con lo que las operaciones esperan de ellos (p.ej.
  nunca pasar un string donde se requiere un número).
- **Static analysis / SAST** — escanear el código fuente sin ejecutarlo,
  buscando coincidencias con patrones conocidos como malos
  (vulnerabilidades de seguridad, code smells, antipatrones).
- **Symbolic execution** — ejecutar el código con entradas simbólicas
  (placeholders) para derivar, mediante un solver SMT, las condiciones
  exactas y los contraejemplos concretos que lo romperían.
- **Formal verification / theorem proving** — demostrar matemáticamente
  que el código satisface una especificación para *todas* las entradas
  posibles, no solo las probadas o exploradas.
- **Unit / integration testing** — comprobar el comportamiento contra
  entradas de ejemplo concretas y salidas esperadas.
- **Property-based testing** — especificar una propiedad general que debe
  cumplirse para cualquier entrada, y generar muchas entradas
  automáticamente buscando una violación.
- **Mutation testing** — introducir bugs pequeños a propósito para
  comprobar si la suite de tests existente realmente los caza.
- **Contract testing** — verificar que la interfaz (forma de
  request/response) entre dos servicios se mantiene consistente, con
  independencia de las tripas de cada lado.

## 2. Verificación de proceso (¿el agente se comporta de forma fiable?)

- **Runtime observability / tracing** — instrumentar un agente para que su
  trayectoria real (llamadas a herramientas, tokens, latencia, errores)
  sea visible y consultable a posteriori.
- **Evals** — tests estructurados del comportamiento de un modelo o agente
  contra un dataset y un método de puntuación: golden-dataset,
  LLM-as-judge, task-completion, adversarial, live/online.
- **Sandboxed execution** — ejecutar el código del agente en un entorno
  aislado (contenedor, microVM) para que una acción mala falle sin
  consecuencias en vez de llegar a producción.
- **Guardrails** — políticas o filtros que restringen qué acciones o
  salidas puede producir un agente, *antes* de que actúe.
- **Human-in-the-loop review** — una persona aprueba, rechaza o edita las
  acciones de alta consecuencia, y esa decisión se realimenta como señal
  de entrenamiento.
- **Multi-agent verification** — critic/verifier (un segundo modelo revisa
  al primero), self-consistency (voto mayoritario entre ejecuciones
  repetidas), debate (dos modelos discuten y un juez decide), reflection
  (autocrítica y revisión), ensembles (combinar modelos distintos).
- **CI/CD integration** — pasar los cambios generados por agentes por el
  mismo pipeline, tests y revisión que el código escrito por humanos, más
  etiquetado de procedencia.
- **Progressive rollout** — desplegar un cambio tras un feature flag a un
  pequeño porcentaje del tráfico, monitorizado antes del despliegue
  completo.
- **Red-teaming / adversarial testing** — sondear deliberadamente en busca
  de fallos bajo un modelo de amenaza adversarial (prompt injection,
  cadenas de mal uso de herramientas, goal drift, exfiltración de datos),
  no solo el error ordinario.
- **Model checking** — explorar exhaustivamente los estados y transiciones
  alcanzables de un agente para verificar invariantes (p.ej. "nunca
  borrar antes de hacer backup"); el análogo de symbolic execution para
  flujos multiagente.

## 3. Marco de clasificación (Trust Spec)

- **T — Test** — verificado ejecutando el sistema contra entradas
  concretas.
- **A — Analysis** — verificado por razonamiento estático: tipos, SAST,
  symbolic execution o demostración formal.
- **I — Inspection** — verificado por un humano o un modelo crítico que lo
  lee y lo juzga.
- **D — Demonstration** — verificado observando el funcionamiento correcto
  en un escenario realista (staging, sandbox).
- **U — Unverifiable / Accepted Risk** — no aplica ningún método, o no
  compensa el coste; se nombra explícitamente en vez de dejarlo como una
  asunción silenciosa.

## Correspondencia técnica → clase (por defecto)

| Técnica | Clase |
|---|---|
| Type checking, SAST, symbolic execution, formal verification, model checking, guardrails | A |
| Unit/integration, property-based, mutation, contract testing, evals de golden-dataset y task-completion | T |
| Human-in-the-loop, LLM-as-judge, critic/verifier, reflection, code review | I |
| Sandboxed execution, progressive rollout, observability/tracing, red-teaming en staging | D |

Esta tabla es el punto de partida, no una regla. Una eval adversarial
ejecutada en staging es `D`; la misma eval en CI es `T`. Justifica la
letra cuando te apartes de la tabla.
