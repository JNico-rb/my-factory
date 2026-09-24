# Optimización de tokens para agentes de IA de programación en repositorios de millones de líneas (Claude Code y ecosistema, sept. 2026)

La palanca que más ahorra no es un plugin. Es la disciplina de contexto: mantener mínimo el contexto base (CLAUDE.md escueto y por capas, skills bajo demanda), aislar la exploración en subagentes, navegar por símbolos (LSP) en lugar de por texto y limpiar la sesión entre tareas. Las herramientas externas "ahorra-tokens" rinden mucho menos de lo que anuncian cuando alguien mide la factura real.

## TL;DR

- **Lo que funciona de verdad (evidencia oficial o independiente):**
  - CLAUDE.md jerárquico y corto, con los flujos especializados movidos a skills.
  - Subagentes de exploración con modelo barato.
  - Plugins de *code intelligence* (LSP) para buscar por símbolo.
  - Hooks que filtran la salida de tests y logs.
  - `/clear` entre tareas no relacionadas.
  - Modo plan antes de implementar.
  - Aprovechar el *prompt caching*, con un prefijo estable y sin cambiar de modelo a mitad de sesión.
- **Hay que desconfiar de las cifras de marketing:**
  - RTK anuncia un 60–90% de ahorro; JetBrains midió un +7,6% de coste a esfuerzo bajo y ±0% a esfuerzo alto.
  - El 40% de claude-context y el 68% de JetBrains Context son benchmarks de los propios fabricantes.
  - Serena no ha publicado un A/B controlado.
- **El debate grep vs. búsqueda semántica sigue abierto.** Anthropic apuesta por la búsqueda agéntica (grep + LSP, sin índice). Cursor mide un +12,5% de precisión al añadir búsqueda semántica sobre grep. Un estudio de PwC encuentra que grep suele superar a la recuperación vectorial. Mi recomendación: empezar con grep + LSP + buenos mapas en CLAUDE.md, y añadir un índice semántico solo si las métricas propias muestran bucles de búsqueda caros.

## Key Findings

### 1. El contexto es el recurso escaso, no solo el dinero

- **Coste y calidad crecen con el contexto.** La documentación oficial de Claude Code dice que "Token costs scale with context size" y que el rendimiento del LLM se degrada a medida que la ventana se llena.
- **El deterioro empieza mucho antes de llenar la ventana.** El informe "Context Rot" de Chroma Research (Hong, Troynikov y Huber, julio de 2025) evaluó 18 LLM, entre ellos GPT-4.1, Claude 4, Gemini 2.5 y Qwen3, y concluyó que su rendimiento se vuelve "increasingly unreliable as input length grows", incluso en tareas triviales.
  - Implicación: una ventana de 1M de tokens no sustituye la gestión de contexto.
- **El coste de referencia es modesto; los desvíos vienen de malos hábitos.** La documentación oficial "Manage costs effectively" de Claude Code cifra el coste medio en despliegues empresariales en torno a 13 USD por desarrollador y día activo (150–250 USD al mes), y el 90% de los usuarios queda por debajo de 30 USD al día.
  - Según Anthropic, el gasto inesperadamente alto suele venir de sesiones largas nunca limpiadas o de dejar Opus como modelo por defecto.

### 2. La postura oficial de Anthropic para monorepos grandes

En su guía de mayo de 2026 para bases de código grandes, Anthropic describe así el funcionamiento de Claude Code: navega "como un ingeniero", recorriendo el sistema de ficheros, usando grep y siguiendo referencias, sin índice. Anthropic argumenta que los pipelines RAG se quedan desfasados cuando miles de ingenieros hacen commits.

La contrapartida que la propia Anthropic reconoce: esto solo funciona bien si Claude tiene suficiente contexto inicial para saber dónde buscar. Los patrones que recomienda:

- **CLAUDE.md escueto y por capas.** El raíz solo contiene punteros y trampas críticas; cada subdirectorio lleva sus convenciones locales. Claude los carga de forma aditiva al subir por el árbol.
- **Iniciar la sesión en el subdirectorio relevante, no en la raíz.** Los CLAUDE.md superiores se siguen cargando igualmente.
- **Comandos de test y lint acotados por subdirectorio.** Ejecutar la suite completa por un cambio en un servicio provoca timeouts y llena el contexto de salida irrelevante.
- **Excluir código generado, artefactos de build y terceros** mediante reglas `permissions.deny` versionadas en `.claude/settings.json`.
- **Mapas del código en markdown** cuando la estructura de directorios no se explica sola.
- **Servidores LSP para buscar por símbolo y no por cadena.** Un grep de un nombre común puede devolver miles de coincidencias; LSP filtra antes de leer nada. Anthropic lo llama una de las inversiones de mayor valor en bases de código multilenguaje. Según el blog de Anthropic "How Claude Code works in large codebases", una empresa de software empresarial (no nombrada) desplegó integraciones LSP en toda la organización antes de adoptar Claude Code, específicamente para que la navegación en C y C++ fuera fiable a escala.
- **Subagentes de solo lectura** que mapean un subsistema y escriben sus hallazgos en un fichero; después, el agente principal edita.
- **Revisar la configuración cada 3–6 meses.** Las instrucciones escritas para modelos anteriores pueden estorbar a los nuevos.

### 3. Qué dice la evidencia sobre los ficheros de contexto (AGENTS.md / CLAUDE.md)

- **ETH Zürich / LogicStar**, "Evaluating AGENTS.md" (arXiv 2602.11988, versión 2 de junio de 2026):
  - Los ficheros de contexto no mejoran en general la tasa de éxito y elevan el coste de inferencia más de un 20% de media.
  - Los generados por LLM (vía `/init`) causan caídas en 5 de 8 configuraciones: −0,5% y −2% de media (no significativo), con un coste significativamente mayor, de +20% y +23%.
  - Los escritos por desarrolladores mejoran un 2,4% de media (no significativo) y superan a los generados por LLM por unos 7%.
  - Los agentes obedecen lo que el fichero dice: `uv` se usa 1,6 veces por tarea cuando se menciona, frente a menos de 0,01 cuando no.
  - En cambio, los ficheros no sirven como vista general del repositorio: no reducen los pasos hasta tocar el fichero relevante.
- **Lulla et al.** (arXiv 2601.20404, enero de 2026; Codex con gpt-5.2-codex, 124 PRs pequeños):
  - Con AGENTS.md, el tiempo mediano baja un 28,64% y los tokens de salida un 16,58%, con una tasa de finalización comparable.
  - No es de Princeton, aunque algunos blogs lo atribuyan así.
  - Mide tokens de salida, no el coste total, lo que explica en parte la aparente contradicción con ETH.
- **Conclusión práctica:**
  - Escribir los ficheros a mano y con mínimos: comandos, herramientas no obvias, restricciones.
  - No volcar descripciones de arquitectura que el agente puede inferir.
  - No aceptar a ciegas un `/init` autogenerado.
  - Anthropic recomienda mantener CLAUDE.md por debajo de 200 líneas.

### 4. Grep vs. búsqueda semántica: evidencia en conflicto

| Fuente | Resultado | Tipo de evidencia |
|---|---|---|
| Anthropic (Claude Code) | Búsqueda agéntica sin índice; RAG se desfasa en monorepos activos | Diseño de producto / experiencia con clientes |
| Cursor (nov. 2025) | Búsqueda semántica + grep: +12,5% de precisión media (6,5%–23,5% según modelo); +2,6% de retención de código en repos grandes | Benchmark interno + A/B del fabricante |
| PwC, "Is Grep All You Need?" (arXiv 2605.15184) | grep supera a la recuperación vectorial en general; cambiar de harness mueve el techo tanto como cambiar de recuperador | Académico, pero sobre LongMemEval (memoria conversacional, no código) |
| Zilliz (claude-context) | ~40% menos tokens a calidad de recuperación equivalente | Benchmark del propio fabricante |
| JetBrains Context (jul. 2026) | Hasta −68% de turnos, −59% de latencia, −48% de coste (205 tareas SWE-bench, 175 de monorepo interno, 1.953 de localización) | Fabricante; solo cifras "hasta", sin varianza ni desglose |

Mi lectura: la combinación híbrida (grep + LSP + semántica opcional) es la opción más defendible. Ninguna cifra de ahorro de índices semánticos tiene todavía una réplica independiente.

### 5. Las herramientas "ahorra-tokens" bajo medición independiente

- **RTK (Rust Token Killer).** JetBrains lo probó con Claude Code 2.1.201, claude-sonnet-5 y 86 tareas de SkillsBench, en pruebas pareadas por tarea (425 ejecuciones, unos 320 USD).
  - Coste: +7,6% a esfuerzo bajo (p=0,004) y +0,1% a esfuerzo alto. La calidad no cambió.
  - El hook solo ve alrededor de un 20% de la salida de herramientas, porque las herramientas nativas Read y Grep no pasan por Bash.
  - El techo teórico de ahorro calculado fue de unos 3% de los tokens de entrada.
  - El propio `rtk gain` reportó 96,2 millones de tokens "ahorrados" mientras la factura subía.
  - Lección general: el ahorro que reporta una herramienta es una afirmación sobre su contrafactual, no sobre tu factura.
- **Skill "caveman".** Anuncia −65%; JetBrains midió −8,5%.
- **Serena.** Su mantenedor afirma ahorros notables y que "RTK... savings were very marginal compared to savings with Serena". Aun así, las cifras de 50–90% que circulan son anecdóticas: todavía no hay un A/B controlado publicado.
  - Contrapartida: la propia definición de herramientas de Serena ocupa unos 24k tokens según el fork serena-slim. El diferimiento de herramientas MCP en Claude Code mitiga parte de ese coste.

## Details

### Tabla comparativa de herramientas

| Herramienta | Qué hace | Madurez / popularidad | Evidencia de ahorro | Veredicto |
|---|---|---|---|---|
| **Plugins de code intelligence (LSP) oficiales de Claude Code** | Ir a definición y encontrar referencias; errores de tipo tras cada edición | Oficial, integrado vía plugins | Anthropic: una llamada "go to definition" sustituye a grep + leer varios ficheros candidatos | **Imprescindible** en lenguajes tipados |
| **Subagentes (Explore, custom)** | Aíslan la exploración en su propio contexto y devuelven un resumen | Oficial | Documentado por Anthropic; ojo al multiplicador de tokens de los sistemas multiagente | **Imprescindible** |
| **Hooks (PreToolUse / Stop)** | Filtran la salida (p. ej., solo los tests fallidos) y proponen actualizaciones de CLAUDE.md | Oficial | Anthropic: filtrar un log de 10.000 líneas lleva el contexto de decenas de miles de tokens a cientos | **Alta prioridad** |
| **Skills** | Conocimiento y flujos que se cargan bajo demanda (divulgación progresiva); pueden limitarse a rutas | Oficial | Reduce el contexto base frente a meterlo todo en CLAUDE.md | **Alta prioridad** |
| **Serena (oraios)** | MCP con LSP: recuperación y edición a nivel de símbolo, memoria | Popular y activo; recomienda no instalarlo desde marketplaces | Anecdótica; sin A/B publicado | Útil en refactors multi-fichero en lenguajes no cubiertos por los plugins LSP oficiales; medir antes de estandarizar |
| **claude-context (Zilliz)** | MCP de búsqueda híbrida BM25 + vectores, chunking por AST, indexación incremental con Merkle | ~11,8k estrellas (jun. 2026), versión 0.x, 115 issues abiertos; unas 1.073 descargas npm/semana | Unos 40%, benchmark propio sin réplica | Probar solo si grep y LSP fallan; por defecto envía código a APIs de embeddings y a Zilliz Cloud |
| **JetBrains Context** | Índice semántico en la nube y búsqueda multi-repo para Claude Code, Codex y Junie | Acceso anticipado (jul. 2026), gratis con JetBrains AI | Hasta −48% de coste, cifra del fabricante | Interesante para organizaciones multi-repo; exige aceptar embeddings en la nube |
| **Repomix** | Empaqueta el repositorio en un fichero; `--compress` con tree-sitter (~70% menos tokens) y `--token-budget` | ~26k estrellas, MIT | Compresión real, pero con pérdida; genera instantáneas que se desfasan | Para instantáneas acotadas (onboarding de subagentes, chats externos), **no** para monorepos enteros |
| **Aider repo map / RepoMapper MCP** | Mapa tree-sitter + PageRank ajustado a un presupuesto de tokens (1k por defecto) | Maduro (Aider) | Enfoque probado; en monorepos con miles de commits diarios, la búsqueda en vivo es preferible | Buena idea para generar el mapa inicial que va en CLAUDE.md |
| **ast-grep** | Búsqueda y reescritura estructural por AST desde la CLI | Maduro | Sin cifras de tokens; reduce falsos positivos frente a grep | Útil vía Bash o skill para codemods |
| **context7** | Documentación actualizada de librerías vía MCP | Popular | Sin benchmark de tokens; evita alucinar APIs | Opcional; preferir CLI y documentación local cuando existan |
| **RTK** | Proxy que comprime la salida de comandos Bash | Muy popular (más de 79k estrellas en GitHub según el blog de Quesma, cuyo propio benchmark también concluyó que el gran ahorro reportado "did not mean cheaper tasks") | **Medido: no ahorra** en trabajo agéntico real | No priorizar; como mucho, inocuo a esfuerzo alto |
| **ccusage / Claude-Code-Usage-Monitor** | Informes de tokens y coste a partir de los JSONL locales | ccusage ~17k estrellas; soporta muchas CLIs | Herramienta de medición, no de ahorro | **Imprescindible** para la línea base, junto a `/usage`, `/context` y OpenTelemetry |

### Técnicas de coste (Claude Code)

- **Prompt caching.**
  - Tarifas: escribir en caché cuesta 1,25× la entrada normal (TTL de 5 min) o 2× (TTL de 1 h); leer cuesta 0,1×. En Opus 5.5 la lectura baja al 5% y en Fable 5.1 y Mythos 5.1 al 2,5%.
  - En sesiones largas, la mayor parte de la entrada son relecturas cacheadas. Por eso comprimir la salida de herramientas ahorra menos de lo que parece.
  - Qué hacer: mantener un prefijo estable (nada volátil al inicio de CLAUDE.md), no cambiar de modelo ni de herramientas MCP a mitad de sesión y vigilar la línea `Prompt cache (main)` de `/usage`.
  - El TTL es de 1 hora en suscripción y de 5 minutos por defecto con API. Una pausa más larga fuerza a reprocesar todo el contexto.
- **Modelo por tarea.**
  - Sonnet para la mayoría del trabajo; Opus para arquitectura y razonamiento complejo.
  - Poner `model: haiku` en los subagentes de tareas simples.
  - Atención: desde la v2.1.198, el subagente Explore integrado hereda el modelo de la sesión, con Opus como tope. Para mantener la exploración barata hay que definir un subagente propio llamado `Explore` con `model: haiku`.
- **Pensamiento extendido.**
  - Los tokens de pensamiento se facturan como salida.
  - Bajar `/effort`, o usar `MAX_THINKING_TOKENS` en modelos con presupuesto fijo, para tareas simples.
- **MCP.**
  - Las definiciones de herramientas MCP se difieren por defecto.
  - Aun así, las CLIs (`gh`, `aws`, `gcloud`) son más eficientes en contexto que los servidores MCP.
  - Desactivar los servidores que no se usen con `/mcp` y auditar el consumo con `/context`.
- **Compactación.**
  - `/compact` con instrucciones de qué conservar, o una sección "Compact instructions" en CLAUDE.md.
  - `/compact` es en sí una petición grande; `/clear` no cuesta nada.
- **Agent teams y multiagente.**
  - Los equipos de agentes usan unas 7 veces más tokens que una sesión estándar en modo plan.
  - Según Anthropic Engineering ("How we built our multi-agent research system"), los agentes usan unas 4 veces más tokens que un chat y los sistemas multiagente unas 15 veces más.
  - Anthropic advierte que los dominios muy interdependientes, como gran parte de la programación, encajan peor.
  - Los subagentes ahorran contexto *principal*, no necesariamente tokens *totales*.
- **Consumo en segundo plano.** Tareas programadas, mensajes entre sesiones y comprobaciones de objetivos reenvían el contexto completo aunque la sesión esté ociosa.

## Recommendations

### Priorizadas por impacto vs. esfuerzo

| # | Acción | Impacto | Esfuerzo |
|---|---|---|---|
| 1 | Medir la línea base: `/usage`, `/context`, ccusage y, en equipos, OpenTelemetry | Alto (sin medir, todo es fe) | Bajo |
| 2 | `/clear` entre tareas no relacionadas; sesiones cortas y acotadas; prompts específicos con fichero y función | Alto | Bajo |
| 3 | Recortar CLAUDE.md a menos de 200 líneas escritas a mano; mover flujos especializados a skills (con alcance por ruta) | Alto | Bajo |
| 4 | Instalar plugins de code intelligence (LSP) para los lenguajes del monorepo | Alto | Bajo–medio |
| 5 | `permissions.deny` versionado para código generado, vendor y artefactos de build | Medio–alto | Bajo |
| 6 | Hooks PreToolUse que filtren la salida de tests, builds y logs (solo fallos, `head`) | Medio–alto | Bajo |
| 7 | Subagente `Explore` propio con `model: haiku` y formato de salida breve; subagentes de verificación | Alto en contexto principal | Bajo |
| 8 | CLAUDE.md por subdirectorio con comandos de test y lint acotados; mapa del repo en la raíz | Alto en monorepos | Medio |
| 9 | Modo plan para tareas complejas; corregir pronto (Esc, `/rewind`) | Medio–alto (evita retrabajo) | Bajo |
| 10 | Sonnet por defecto y Opus solo a demanda; ajustar `/effort` | Medio–alto | Bajo |
| 11 | Plugin interno de equipo (skills + hooks + MCP) distribuido vía marketplace gestionado, con un responsable (DRI) | Medio (consistencia) | Medio |
| 12 | Pilotar un índice semántico (claude-context local con Ollama + Milvus, o JetBrains Context) **solo** si las métricas muestran bucles de grep caros; A/B pareado | Incierto | Medio–alto |
| — | **No priorizar:** RTK, "caveman" y compresores genéricos de salida; empaquetar el monorepo entero con repomix | Bajo o negativo | — |

### Flujo de trabajo recomendado, paso a paso

1. **Preparar el repositorio (una vez):**
   - CLAUDE.md raíz con punteros, trampas y un mapa de directorios de una línea por carpeta.
   - CLAUDE.md por paquete con sus comandos de test, lint y build.
   - Reglas `permissions.deny` para lo generado.
   - Plugins LSP instalados.
   - Skills por dominio.
   - Hooks de filtrado.
   - Subagente Explore con Haiku.
   - Todo ello empaquetado como plugin del equipo.
2. **Arrancar en el subdirectorio del paquete afectado**, no en la raíz.
3. **Explorar:** "usa subagentes para investigar X". El subagente devuelve un resumen o lo escribe en un fichero (`notes/plan-X.md`), de modo que el contexto principal queda limpio.
4. **Planificar en modo plan (Shift+Tab):** revisar y anotar el plan. En tareas grandes, guardar un spec en un fichero.
5. **Implementar en incrementos pequeños con una verificación explícita:** primero un test (TDD) o un resultado esperado, después ejecutar solo los tests del paquete, con salida filtrada.
6. **Verificar** con un subagente revisor adversarial, o con un segundo Claude en el patrón Writer/Reviewer.
7. **Cerrar:** commit, un hook Stop que proponga mejoras a CLAUDE.md y `/clear` antes de la siguiente tarea. Si hay que continuar al día siguiente, usar `/rename` y `/resume`, o un resumen escrito en fichero, en lugar de arrastrar la sesión.
8. **Para migraciones masivas:** ejecuciones paralelas de `claude -p` por fichero o módulo, cada una con su propio contexto, en vez de una sesión gigante.
9. **Revisar mensualmente** los datos de ccusage y `/usage` (atribución por skill, subagente y MCP; fallos de caché). Revisar la configuración cada 3–6 meses o tras cada cambio de modelo.

## Caveats

- **Casi toda la evidencia de ahorro viene de los fabricantes:** Zilliz, JetBrains Context, Cursor, RTK y Serena. Las únicas mediciones independientes y rigurosas encontradas (las de JetBrains sobre RTK y caveman, y la de ETH sobre AGENTS.md) muestran ahorros muy inferiores a los anunciados, o costes mayores.
- **Los benchmarks no representan monorepos de millones de líneas.** SWE-bench, SkillsBench y LongMemEval usan tareas pequeñas y repositorios Python. La propia Anthropic admite que el enfoque de CLAUDE.md jerárquico puede romperse con cientos de miles de carpetas o con control de versiones que no sea git.
- **El estudio ETH cambió de conclusión entre versiones:** la v1 decía que los ficheros de contexto *reducen* el éxito; la v2 dice que el efecto no es significativo. Solo cubre Python.
- **Los detalles de producto cambian rápido.** Precios de caché por modelo, el modelo de Explore y el diferimiento de MCP han cambiado en 2026. Conviene verificarlos con `claude --version` y la documentación actual.
- **Ahorrar contexto principal no equivale a ahorrar tokens totales.** Subagentes y equipos pueden subir el total mientras mejoran la calidad. Hay que optimizar el coste por tarea completada y correcta, no los tokens por mensaje.
- **Sin telemetría propia no hay forma de validar nada de lo anterior.** Cualquier herramienta nueva debe entrar con un A/B pareado sobre tareas reales del propio repositorio.