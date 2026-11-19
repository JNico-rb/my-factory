# Mods

Mods de Claude Code propios, uno por carpeta. Este repositorio es también un marketplace local
(`.claude-plugin/marketplace.json` en la raíz), así que cada mod se instala como un plugin más y se
enciende o apaga con un interruptor.

| Mod | Qué hace |
|---|---|
| [`foundry-gasto`](foundry-gasto/) | Panel con el gasto de Azure AI Foundry: tu gasto personal con límites por día, semana y mes que cortan al llegar, y el presupuesto del grupo y la suscripción. |

## Encender y apagar

En `%USERPROFILE%\.claude\settings.json`, dentro de `enabledPlugins`:

```json
"foundry-gasto@my-factory": true
```

`false` lo apaga. Hace falta cerrar y volver a abrir Claude Code para que cargue o deje de cargar.
El contador de `foundry-gasto` lee ese interruptor y se apaga solo, en unos 10 segundos, al ponerlo
en `false`.

## Editar un mod

Claude Code lee el mod desde esta carpeta, no desde una copia: edita aquí y ejecuta
`/reload-plugins` en una sesión (o reinicia) para que coja los cambios. Pruebas y validación:

```powershell
claude plugin test .\mods\foundry-gasto
```

```powershell
claude plugin validate .\mods\foundry-gasto
```

## Añadir un mod

1. Crea `mods/<nombre>/` con `.claude-plugin/plugin.json` y `hooks/hooks.json`.
2. Añade una entrada en `.claude-plugin/marketplace.json` con `"source": "./mods/<nombre>"`.
3. `claude plugin marketplace update my-factory`, y después `claude plugin install <nombre>@my-factory`.
4. Añade su fila a la tabla de arriba.

## Instalar el marketplace en otro equipo

```powershell
claude plugin marketplace add C:\ruta\a\my-factory
```

```powershell
claude plugin install foundry-gasto@my-factory
```
