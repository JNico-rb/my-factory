# Workflow: arranque del día

Estado: **borrador**. Quedan preguntas abiertas (ver al final).

## Loop

De lunes a viernes, hacia las 9:00, Jaime abre el workspace de QONBOARDING (`C:\Users\jaime.rodriguez\projects\WORKSPACE-QRG-QONBOARDING`) y hace tres cosas:

1. Trae los cambios de los demás.
2. Entiende qué ha cambiado y si le afecta.
3. Mira qué le ha pedido Fernando (el team lead) para elegir por dónde empezar.

El alcance es **solo QONBOARDING**.

## Trigger

- Es un **event**: la primera sesión de Claude Code del día en el workspace, abierta desde la extensión de VS Code.
- Solo se dispara de lunes a viernes, antes de las 12:00, y si ese día todavía no se ha ejecutado.
- Además, Jaime puede lanzarlo a mano con `/buenos-dias` cuando quiera.
- Ningún fichero versionado del proyecto se modifica. El mecanismo concreto está pendiente (pregunta abierta 1).

## Pasos (sin checkpoint hasta el final)

1. **Ventana temporal**: va desde la marca de la última ejecución hasta ahora. Si no hay marca, cubre las últimas 24 h laborables.
2. **Traer cambios**:
   - Hace `git fetch` del workspace y de cada clon listado en `ado/ado-project.json`.
   - Solo hace fast-forward. Si una rama local ha divergido o tiene cambios sin commitear, no la toca y lo anota en el brief.
3. **Entender cambios**: lista las PRs completadas en esa ventana en cada repo y, de cada una, el autor, el título, el enlace y los ficheros tocados.
   - Una PR **te afecta** si toca ficheros que tienes modificados o en una rama tuya sin mergear, o si toca un Issue asignado a ti.
4. **Tus tareas**:
   - Lee los items del board asignados a Jaime.
   - El workflow **no lee Teams**. Las peticiones de Fernando las escribe o pega Jaime en el prompt (ver checkpoint).
5. **Guardar marca**: escribe la marca de la ejecución en un fichero local que no esté versionado.

## Checkpoint (único): el brief

Aparece en el chat de Claude y se lee en menos de un minuto:

- **Qué ha cambiado**:
  - Primero las PRs que te afectan, explicando por qué.
  - Después el resto, una línea por PR con su enlace.
  - También los clones que no se han podido actualizar.
- **Tus tareas en el board**, ordenadas por prioridad y cada una con su enlace.
- Al final, una pregunta: «¿Qué te ha pedido Fernando hoy?»

Jaime responde con lo que tiene que hacer ese día. Claude cruza esa respuesta con el board, propone por dónde empezar y lanza `/start-task` cuando Jaime da el visto bueno. Si a la tarea le falta su item en el board, lo crea `/start-task`.

## Límites

- El workflow solo lee y hace fast-forward.
- No envía nada, no publica en Teams y no hace merge.

## Preguntas abiertas

1. Cuál es el mecanismo del trigger y dónde vive el skill: `settings.local.json` con un skill personal, o un mod.
