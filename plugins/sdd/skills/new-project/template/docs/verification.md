# verification.md

Cómo se comprueba que **el código es correcto** y que **los agentes se comportan de forma fiable**. El vocabulario está en `definitions.md`; las decisiones de diseño, en `architecture.md`. Lo redacta y lo mantiene la skill `sdd:verification`.

---

## Marco de clasificación (T/A/I/D/U)

Todo requisito, invariante o componente recibe **una letra**. La letra dice *cómo* se gana la confianza, no *cuánta*.

| Clase | Nombre | Se verifica… |
|---|---|---|
| **T** | Test | Ejecutando el sistema con entradas concretas |
| **A** | Analysis | Razonando estáticamente: tipos, análisis estático, ejecución simbólica, prueba formal |
| **I** | Inspection | Leyendo y juzgando: una persona o un modelo crítico |
| **D** | Demonstration | Observando la operación correcta en un escenario realista |
| **U** | Unverifiable | No hay método aplicable, o no compensa su coste |

- **U es una decisión, no un olvido.** Se escribe en *Riesgos aceptados (U)* con nombre y motivo.
- **La letra no es un ascenso.** A no es «mejor» que T: se elige el método más barato que dé la garantía necesaria.

## Alcance

Se redacta con la skill `sdd:verification` al escribir la primera spec.

## Verificación de código

| # | Qué se verifica | Técnica | Clase | Herramienta / dónde vive |
|---|---|---|---|---|

## Verificación de proceso (agentes)

| # | Qué se verifica | Técnica | Clase | Herramienta / dónde vive |
|---|---|---|---|---|

## Riesgos aceptados (U)

| # | Qué no se verifica | Por qué | Mitigación parcial |
|---|---|---|---|

## Puertas de calidad

<!-- GAP: los comandos que tienen que estar en verde para aprobar un cambio (los de lint, tipos y tests del Stack de AGENTS.md). -->
