# Optimización de tokens en proyectos LLM: herramientas, funciones y estrategias (sep. 2026)

Lo que más ahorra sin bajar calidad es **meter menos contexto irrelevante** con prompt caching, limpieza y compactación del contexto, subagentes aislados y recuperación precisa de código. Comprimir texto de forma agresiva ahorra menos. Muchas veces la calidad **mejora**, porque todos los modelos rinden peor cuanto más largo es el contexto ("context rot").

## TL;DR
- **Haz primero, casi gratis y sin riesgo:** caching con un prefijo estable (lecturas a 0,1× en Anthropic), `/clear` entre tareas, CLAUDE.md corto, Sonnet/Haiku por defecto, desactivar los MCP que no uses, y Batch API (−50%) para todo lo que no sea interactivo.
- **Después, lo de mayor impacto estructural:** limpiar resultados de herramientas y compactar (Anthropic midió −84% de tokens en 100 turnos), tool search y ejecución de código con MCP (−85% a −98,7% en definiciones y datos intermedios), y subagentes que devuelvan resúmenes de 1–2 K tokens.
- **Mide antes de creer:** las cifras de "60–90%" de muchas herramientas son autoinformadas. JetBrains midió rtk con una prueba A/B y no encontró ahorro neto. Instala ccusage o Langfuse, y valida cada cambio con tus propias evaluaciones.

## Key Findings (evidencia sobre el tradeoff)
- **Más contexto ≠ mejor.** El informe técnico de Chroma "Context Rot" (julio de 2025; Kelly Hong, Anton Troynikov y Jeff Huber) evaluó 18 modelos punteros (GPT-4.1, Claude 4, Gemini 2.5, Qwen3). Concluye que "performance grows increasingly unreliable as input length grows", incluso en tareas triviales. Recortar contexto suele ahorrar y además mejorar.
- **Gestión de contexto (Anthropic, evaluación interna):** limpiar contexto más la herramienta de memoria dio +39% de rendimiento, y la limpieza sola +29%. En 100 turnos consumió −84% de tokens. Son cifras propias de Anthropic, no independientes.
- **Multiagente es caro:** según el artículo de ingeniería de Anthropic "How we built our multi-agent research system" (13 jun 2025), "agents typically use about 4× more tokens than chat interactions, and multi-agent systems use about 15× more tokens than chats". En esa misma evaluación interna, Opus 4 como agente principal con subagentes Sonnet 4 superó en un 90,2% a Opus 4 trabajando solo. El uso de tokens explica el 80% de la varianza de rendimiento en BrowseComp. Solo compensa en tareas paralelizables y de alto valor. Anthropic dice que no encaja con tareas de mucha dependencia entre agentes, como gran parte de la programación.
- **Compresión de prompts (LLMLingua):** hasta 20× con ~1,5 puntos de caída en GSM8K. LongLLMLingua dio +17,1% de rendimiento con 4× menos tokens. El efecto depende de la tarea, así que valídalo.
- **Compactación activa en agentes de código (arXiv 2601.07190):** −22,7% de tokens en SWE-bench Lite con la misma tasa de éxito (3/5). La muestra es pequeña (N=5) y en una instancia el consumo subió +110%.
- **Enrutado de modelos (RouteLLM, ICLR 2025):** −85% de coste manteniendo el 95% de la calidad de GPT-4 en MT-Bench, pero solo −45% en MMLU y −35% en GSM8K. Depende totalmente de tu mezcla de consultas, y la pérdida de calidad es silenciosa.
- **Nivel de esfuerzo:** en el lanzamiento de Opus 4.5, Anthropic informó que en esfuerzo medio igualó el mejor resultado de Sonnet 4.5 en SWE-bench Verified con un 76% menos de tokens de salida. En esfuerzo alto lo superó por 4,3 puntos con un 48% menos de tokens.

## 1. Herramientas / plug-ins / MCP

| Herramienta | Qué hace | Impacto estimado | Madurez / riesgo | Link |
|---|---|---|---|---|
| **Serena** (MCP/plugin) | Navegación y edición por símbolos vía LSP (find_symbol, referencias) en lugar de leer ficheros enteros | Alto en repos grandes; nulo en proyectos pequeños o nuevos | Popular, MIT. Instálalo según su Quick Start, no desde marketplaces (el propio proyecto lo advierte) | https://github.com/oraios/serena |
| **claude-context** (Zilliz, MCP) | Búsqueda semántica híbrida (BM25 + vectores) sobre el código, con chunking por AST | Según el blog de Zilliz, "Token usage dropped by over 40%, without any loss in recall" (evaluación propia, sin réplica independiente) | Popular. Envía el código a proveedores de embeddings y a Milvus/Zilliz | https://github.com/zilliztech/claude-context |
| **Aider repo map** | Mapa del repo con tree-sitter + PageRank dentro de un presupuesto (`--map-tokens`, 1 K por defecto) | Alto para dar estructura sin leer ficheros | Muy maduro. Existen ports como MCP (RepoMapper) | https://aider.chat/docs/repomap.html |
| **Repomix** (`--compress`) | Empaqueta el repo; con tree-sitter deja solo firmas, tipos e imports. Cuenta tokens y admite `--token-budget` | ~−70% tokens (con pérdida: quita los cuerpos de las funciones) | Maduro. La compresión es experimental | https://repomix.com/guide/code-compress |
| **rtk** (Rust Token Killer) | Hook o proxy que comprime la salida de git, tests y logs antes de que llegue al LLM | Anuncia 60–90%; JetBrains midió +7,6% de coste en esfuerzo bajo y ±0% en alto, con la misma calidad | Muy popular. Solo actúa sobre Bash y su contador sobreestima el ahorro | https://github.com/rtk-ai/rtk |
| **LLMLingua / LongLLMLingua / LLMLingua-2** (Microsoft) | Compresión de prompts con un modelo pequeño; integrado en LangChain y LlamaIndex | 2–20×; ideal para RAG y documentos largos | Investigación sólida. Riesgo en código e instrucciones exactas | https://github.com/microsoft/LLMLingua |
| **RouteLLM** | Router entrenado que decide entre un modelo fuerte y uno débil por consulta | −35% a −85% según benchmark | Open source (LMSYS). Calibra el umbral con tus datos | https://github.com/lm-sys/routellm |
| **ccusage** | CLI local que analiza los logs de Claude Code, Codex, etc. por día, sesión o proyecto | Medición, no ahorro | Muy popular. Claude Code borra los logs a los 30 días (`cleanupPeriodDays`) | https://ccusage.com/ |
| **Langfuse** (+ plugin para Claude Code) | Trazas con tokens, cache reads y coste por prompt o feature | Medición | Maduro, autoalojable | https://github.com/langfuse/Claude-Observability-Plugin |
| **Helicone / LiteLLM / Portkey** | Gateway o proxy: coste por request o clave, presupuestos, caché | Medición + límites | Maduros. LiteLLM no está auditado por Anthropic | https://docs.litellm.ai/docs/proxy/virtual_keys |
| **Plugins de code intelligence** (Claude Code) | LSP oficial: "go to definition" en vez de grep + leer varios ficheros | Medio-alto en lenguajes tipados | Oficial | https://code.claude.com/docs/en/costs |

## 2. Funciones de plataforma

| Función | Qué hace | Impacto | Link |
|---|---|---|---|
| **Prompt caching (Anthropic)** | Cachea el prefijo (tools → system → mensajes). Escritura 1,25× (5 min) o 2× (1 h); lectura 0,1×. Hasta 4 breakpoints o modo automático | −70% a −90% del input repetido. Un cambio en tools o system invalida todo lo que viene después | https://platform.claude.com/docs/en/build-with-claude/prompt-caching |
| **Prompt caching (OpenAI)** | Automático. Mínimo 1.024 tokens en GPT-5.6+; descuento de hasta 90%. En GPT-5.6+ la escritura cuesta 1,25× y la lectura 0,1×. `prompt_cache_key` ayuda al enrutado solo en modelos anteriores a GPT-5.6 | Alto con un prefijo estable | https://developers.openai.com/api/docs/guides/prompt-caching |
| **Batch API** (Anthropic / OpenAI) | Procesamiento asíncrono (<24 h) al 50%; acumulable con caching | −50% (hasta ~90% combinado con caché) | https://www.respan.ai/articles/anthropic-message-batches-api |
| **Context editing** (`clear_tool_uses_20250919`) | Borra resultados de herramientas antiguos al superar un umbral | −84% en 100 turnos (dato de Anthropic) | https://platform.claude.com/docs/en/build-with-claude/context-editing |
| **Compactación en servidor** (beta, Claude 4.6+) | Claude resume los turnos antiguos en el servidor. Modo bajo demanda (`compact-2026-09-04`) o por umbral (`compact_20260112`, 150 K por defecto, mínimo 50 K) con `instructions` personalizables | No hay cifra publicada; mantiene el contexto pequeño y funciona con caching | https://platform.claude.com/docs/en/build-with-claude/compaction |
| **Memory tool** | Memoria en ficheros del lado del cliente, persistente entre sesiones | +39% combinada con context editing | https://claude.com/blog/context-management |
| **Tool Search Tool** (`defer_loading`) | Carga las herramientas bajo demanda | 77 K → 8,7 K tokens (−85%); la precisión de Opus 4 subió del 49% al 74% | https://anthropic.com/engineering/advanced-tool-use |
| **Programmatic Tool Calling / ejecución de código con MCP** | El modelo escribe código que llama a las tools; los datos intermedios no pasan por el contexto | −37% en tareas complejas; 150 K → 2 K (−98,7%) en el ejemplo de Anthropic. Requiere sandbox | https://www.anthropic.com/engineering/code-execution-with-mcp |
| **Effort** (`output_config.effort`) | Controla los tokens totales (texto, tools, thinking). Niveles low a max | Medium −76% de output en SWE-bench (Opus 4.5) | https://platform.claude.com/docs/en/build-with-claude/effort |
| **Claude Code: `/clear`, `/compact <foco>`, `/context`, `/usage`, plan mode, `/model`** | Limpiar entre tareas, compactar con instrucciones, ver qué ocupa el contexto | Alto (es el hábito de mayor impacto según la documentación) | https://code.claude.com/docs/en/costs |
| **Claude Code: hooks y skills** | Un hook PreToolUse filtra logs o tests (p. ej., solo los errores); una skill da contexto de arquitectura sin explorar | De decenas de miles de tokens a cientos por log | https://code.claude.com/docs/en/costs |
| **Claude Code: subagentes con `model: haiku`** / `CLAUDE_CODE_SUBAGENT_MODEL` | Aíslan la exploración y usan un modelo barato | Alto | https://code.claude.com/docs/en/costs |
| **claude.ai Projects: modo RAG** | Se activa solo cuando el conocimiento se acerca al límite del contexto; hasta 10× más capacidad | Automático (planes de pago) | https://support.claude.com/en/articles/11473015-retrieval-augmented-generation-rag-for-projects |

## 3. Estrategias por problema

**Contexto grande**
- Carga lo mínimo y búscalo "justo a tiempo" (referencias, rutas) en lugar de volcarlo todo por adelantado.
- Pon el contenido estable al principio (caché) y el variable al final.
- Prefiere CLIs (`gh`, `aws`) a MCP: no añaden listado de herramientas.
- **Ojo:** `.claudeignore` **no existe** en Claude Code; es una alucinación muy difundida. Usa `permissions.deny` con `Read(...)` en `.claude/settings.json`. Hay bugs reportados en los que deny no se aplicó, así que no lo trates como una garantía de seguridad.

**Multiagente**
- Usa un orquestador y trabajadores con contexto limpio que devuelvan resúmenes de 1–2 K tokens.
- Aplica el patrón "artefacto": los subagentes escriben en ficheros y devuelven solo referencias.
- Mantén los equipos pequeños, usa Sonnet para los compañeros de equipo, escribe spawn prompts concisos y cierra los agentes que terminen.
- Úsalo solo en tareas paralelizables y pon límites de coste por ejecución (el patrón publicado no los trae).

**Código**
- Recupera por símbolos o AST (Serena, LSP, repo map) antes que leer ficheros completos.
- Pide ediciones como diff o search/replace, no como reescritura del fichero entero (Aider).
- Usa el modo arquitecto: un modelo fuerte planifica y uno barato edita.
- Filtra la salida de tests y logs con hooks.
- Usa plan mode (Shift+Tab) para evitar prueba y error.

**Conversaciones largas**
- `/clear` al cambiar de tarea.
- `/compact` tras hitos, con instrucciones sobre qué conservar; en la API, compactación y limpieza de resultados de herramientas.
- Notas estructuradas (NOTES.md, TODO) como memoria externa.
- Ten en cuenta que la caché de Claude Code expira a los 5 min de inactividad (TTL de 5 min según reportes de la comunidad). Cada reanudación en frío reescribe todo el prefijo a 1,25×.

**Prompt / salida**
- System prompt conciso y en secciones (XML o Markdown), con ejemplos canónicos y no listas de casos límite.
- Pide salida estructurada (JSON) y breve, limita `max_tokens` y baja el effort en tareas simples. La salida cuesta varias veces más que la entrada.

## Recomendaciones: quick-start priorizado
1. **Mide la línea base** (1 h): `/usage` y `/context` en Claude Code, ccusage, y Langfuse o Helicone en la API. Sin esto no sabrás si un cambio ahorra.
2. **Higiene de sesión** (0 riesgo): `/clear` entre tareas y CLAUDE.md corto. La documentación de Claude Code recomienda "target under 200 lines per CLAUDE.md file. Longer files consume more context and reduce adherence". Además, desactiva los MCP que no uses, usa Sonnet por defecto y Haiku en subagentes simples.
3. **Caching bien estructurado** en la API: prefijo estable, breakpoint al final del system; vigila la tasa de aciertos.
4. **Batch API** para evaluaciones, clasificación y procesos nocturnos (−50%, acumulable con caché).
5. **Limpieza de resultados de herramientas + compactación** en agentes largos, con instrucciones de compactación afinadas para tu caso.
6. **Tool search / `defer_loading`** si tienes más de ~10 herramientas; **ejecución de código con MCP** si mueves muchos datos entre herramientas.
7. **Recuperación de código precisa:** plugin LSP o Serena en repos grandes; claude-context si la búsqueda semántica aporta valor.
8. **Hooks** que filtren la salida de tests y logs. Prueba rtk con una A/B propia, no te fíes de su contador.
9. **Effort medium/low** y routing (RouteLLM o reglas) **solo con evaluaciones**, porque la pérdida de calidad es silenciosa.
10. **LLMLingua** solo para RAG o documentos largos en lenguaje natural; evítalo en código e instrucciones críticas.

## Caveats
- La mayoría de las cifras vienen de los propios proveedores (Anthropic, Zilliz, rtk, LMSYS) o son benchmarks específicos; úsalas como orden de magnitud.
- Los precios y multiplicadores de caché cambian por modelo; algunos modelos recientes de Anthropic tienen lecturas más baratas que 0,1×. Consulta la tabla vigente.
- La compactación y la compresión son con pérdida: pueden eliminar detalles cuya importancia solo se ve más tarde.
- Las funciones en beta (compactación, cabeceras de context management) pueden cambiar de nombre o de comportamiento.