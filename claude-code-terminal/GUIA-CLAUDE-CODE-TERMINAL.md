# Claude Code en terminal (Windows Terminal + PowerShell 7)

Guía práctica para quien viene de la app de escritorio. Contrastada con la documentación oficial (code.claude.com/docs) el 2026-11-20, versión 2.1.3xx.

Pulsa `?` con el prompt vacío para ver todos los atajos dentro de Claude Code.

---

## 1. Lo esencial

| Quiero… | Cómo |
|---|---|
| **Pegar una foto** | **Alt+V**. En Windows nativo Ctrl+V no pega imágenes. Aparece `[Image #1]` y puedes nombrarla: "qué falla en [Image #1]". También puedes arrastrar la imagen o escribir su ruta. |
| **Borrar todo lo escrito o pegado** | **Ctrl+C** una vez. Si te arrepientes, pulsa **↑** justo después y lo recuperas. |
| Borrar y guardar en el historial | **Esc Esc** con texto escrito |
| Borrar un bloque `[Pasted text #1 +120 lines]` | **Ctrl+W** lo quita entero. **Ctrl+Y** lo restaura. |
| **Activar una skill con texto ya pegado** | **Ctrl+S** guarda tu texto y deja el prompt vacío. Escribe `/skill args` y pulsa Enter. Después, **Ctrl+S** con el prompt vacío te devuelve el texto. |

**Skills y texto pegado:** un `/comando` solo se ejecuta si va **al principio** del mensaje. Si lo escribes a mitad del texto, Claude lo lee como una indicación y no lo ejecuta directamente. Otra opción es empezar el mensaje con `/skill` y poner tu texto detrás, porque todo eso pasa a ser el argumento de la skill (`$ARGUMENTS`).

Se pueden encadenar skills en un mismo mensaje: `/write-tests /fix-issue 123`.

Un texto pegado de más de 800 caracteres o más de 3 líneas se muestra como un bloque plegado, pero se envía entero. Para textos muy grandes es mejor guardarlos en un fichero y pedirle a Claude que lo lea.

---

## 2. Edición del prompt

| Atajo | Qué hace |
|---|---|
| Shift+Enter / Ctrl+J / `\` + Enter | Salto de línea. Shift+Enter funciona sin configurar nada en Windows Terminal 1.25 o posterior. |
| **Ctrl+G** | Abre el prompt en un editor externo (útil para prompts largos) |
| **Ctrl+R** | Busca en el historial |
| ↑ / ↓ | Mensajes anteriores |
| Ctrl+_ | Deshace |
| Ctrl+U | Borra hasta el inicio de la línea |
| Ctrl+W / Ctrl+Backspace | Borra la palabra o ruta anterior |
| Ctrl+A / Ctrl+E | Va al inicio o al final de la línea |
| Alt+B / Alt+F | Salta una palabra hacia atrás o hacia delante |
| Ctrl+S | Guarda y recupera un borrador (*stash*) |
| `@ruta` | Autocompleta rutas y añade ese fichero al contexto |
| `!comando` | Ejecuta un comando de shell y Claude ve la salida |

---

## 3. Control de la sesión

| Atajo | Qué hace |
|---|---|
| **Shift+Tab** | Cambia de modo: Manual → aceptar ediciones → plan → auto. Si no responde, usa **Alt+M**. |
| **Esc** | Interrumpe a Claude sin perder lo que ha hecho |
| **Esc Esc** (prompt vacío) | Abre `/rewind` para volver atrás en código o conversación |
| Enter mientras trabaja | Pone tu mensaje en cola y no le interrumpe |
| Ctrl+Enter / Ctrl+X Ctrl+S | Envía ya los mensajes en cola |
| **Ctrl+O** | Muestra el detalle de lo que está haciendo (transcript) |
| Ctrl+T | Muestra la lista de tareas de Claude |
| Ctrl+B | Manda un comando o agente largo a segundo plano |
| Ctrl+X Ctrl+K | Para todos los subagentes en segundo plano |
| Alt+P | Cambia de modelo sin perder el prompt |
| Alt+O | Activa o desactiva el fast mode |
| Ctrl+L | Redibuja la pantalla (no borra la conversación) |
| Ctrl+D ×2 | Sale |

---

## 4. Comandos más útiles

**Sesiones**
- `/resume`: retoma una conversación anterior.
- `/clear`: empieza de cero.
- `/compact [foco]`: resume la conversación cuando el contexto se llena.
- `/rewind`: vuelve atrás.
- `/branch` y `/fork`: abren una rama de la conversación.

**Contexto y coste**
- `/context`: cuánto contexto llevas.
- `/usage`: consumo y límites.
- `/btw`: pregunta rápida que no entra en el historial.
- `/model`, `/effort`, `/fast`.

**Ajustes**
- `/config`, `/theme`, `/output-style`, `/statusline`, `/keybindings`, `/permissions`, `/hooks`, `/terminal-setup`, `/tui fullscreen`.

**Proyecto**
- `/init`: crea el CLAUDE.md.
- `/memory`: edita la memoria.
- `/add-dir`: añade otra carpeta a la sesión.
- `/cd`: cambia de carpeta.

**Calidad**
- `/diff`, `/code-review`, `/simplify`, `/security-review`, `/doctor` (si algo falla), `/export`, `/copy`.

**Extensiones**
- `/plugin`, `/reload-plugins`, `/mcp`, `/skills`, `/agents`.

**Ya no existen**
- `/vim`: el modo vim se activa en `/config` → Editor mode.
- `/pr-comments` y `/ultraplan`.

Desde PowerShell, para retomar la última conversación:

```powershell
claude -c
```

Para elegir qué conversación retomar:

```powershell
claude -r
```

---

## 5. Personalización (lo que ya tengo configurado)

Todo está en `C:\Users\jaime.rodriguez\.claude\`. Hay una copia de seguridad previa en `settings.json.bak-terminal-20261120`.

### Barra de estado
- **Script:** `statusline.ps1`.
- **Muestra:** `carpeta | rama git | modelo | ctx 63%`.
- **Colores del %:** verde por debajo del 50 %, amarillo hasta el 80 %, rojo a partir del 80 %.
- **Configuración:** clave `statusLine` en `settings.json`.

### Aviso cuando Claude termina o te necesita
- **Script:** `notify.ps1`, que lanza una notificación de Windows.
- **Hooks:**
  - `Stop`: avisa cuando termina un turno.
  - `Notification`: avisa cuando espera un permiso (`permission_prompt`) o lleva unos 60 s parado (`idle_prompt`).
- **Solo en terminal:** en la app de escritorio no hace nada, porque detecta `CLAUDE_CODE_ENTRYPOINT=claude-desktop`.
- **Si molesta demasiado:** borra el bloque `Stop` de `hooks` en `settings.json` y te quedas solo con los avisos de permiso o inactividad.

### Más ideas
1. **Atajos propios:** `/keybindings` crea o abre `keybindings.json`. Los cambios se aplican sin reiniciar. Ejemplo para que Ctrl+E abra el editor externo:
   ```json
   {
     "$schema": "https://www.schemastore.org/claude-code-keybindings.json",
     "bindings": [
       { "context": "Chat", "bindings": { "ctrl+e": "chat:externalEditor" } }
     ]
   }
   ```
2. **Temas:** `/theme`. Los temas propios van en `~/.claude/themes/<nombre>.json`, con `base` y `overrides`.
3. **Modo vim:** `/config` → Editor mode.
4. **Menos peticiones de permiso:** la skill `/fewer-permission-prompts` propone qué comandos permitir siempre.
5. **Plugins:** `/plugin` abre el marketplace oficial (`claude-plugins-official`), que ya viene añadido.
6. **VS Code:** la extensión `anthropic.claude-code` integra Claude en el editor.
   - Ctrl+Esc salta entre el editor y Claude.
   - Alt+K inserta una referencia `@` con números de línea.
   - En el terminal integrado de VS Code, ejecuta `/terminal-setup` para que funcione Shift+Enter.
7. **Configuración compartida:** CLAUDE.md global, skills y `settings.json` son los mismos en terminal y en escritorio.

---

## 6. Cuidado en Windows

- **Fotos:** se pegan con Alt+V, no con Ctrl+V.
- **Shift+Tab:** si no responde, usa **Alt+M**.
- **Ctrl+Z:** no suspende el proceso (eso es de Linux y macOS).
- **Backspace que borra palabras enteras:** añade `"CLAUDE_CODE_BS_AS_CTRL_BACKSPACE": "0"` en el bloque `env` de `settings.json`.
- **Parpadeo o saltos de scroll:** usa `/tui fullscreen`. Ya está activado con `"tui": "fullscreen"`.
- **Rutas en `settings.json`** (hooks y statusline): escríbelas con barras `/`, no con `\`.
- **`settings.json`:** es JSON estricto, sin comentarios ni comas finales. Después de editarlo, comprueba con `/status` que ha cargado bien.

---

## Documentación oficial
- Modo interactivo y atajos: https://code.claude.com/docs/en/interactive-mode
- Comandos: https://code.claude.com/docs/en/commands
- Terminal: https://code.claude.com/docs/en/terminal-config
- Keybindings: https://code.claude.com/docs/en/keybindings
- Status line: https://code.claude.com/docs/en/statusline
- Hooks: https://code.claude.com/docs/en/hooks-guide
- Settings: https://code.claude.com/docs/en/settings
- Skills: https://code.claude.com/docs/en/skills
