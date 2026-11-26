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

`foundry-gasto` (0.5.0) arranca un contador en segundo plano que **no** se apaga solo al ponerlo en
`false`: si estuvo encendido, páralo a mano o reinicia el equipo:

```powershell
Get-CimInstance Win32_Process -Filter "Name='pwsh.exe'" | Where-Object CommandLine -like '*contador.ps1*' | ForEach-Object { Stop-Process -Id $_.ProcessId }
```

## Editar un mod

Claude Code lee el mod desde esta carpeta, no desde una copia: edita aquí y ejecuta
`/reload-plugins` en una sesión (o reinicia) para que coja los cambios. Validación:

```powershell
claude plugin validate .\mods\foundry-gasto
```

## Añadir un mod

1. Crea `mods/<nombre>/` con `.claude-plugin/plugin.json` y `hooks/hooks.json`.
2. Añade una entrada en `.claude-plugin/marketplace.json` con `"source": "./mods/<nombre>"`.
3. `claude plugin marketplace update my-factory`, y después `claude plugin install <nombre>@my-factory`.
4. Añade su fila a la tabla de arriba.

## Zips para compañeros

`dist/` guarda los zips que se pasan a otras personas: se instalan sin marketplace, con el
`instalar.ps1` y el `LEEME.md` que llevan dentro (por `CLAUDE_CODE_PLUGIN_DIRS`, sin interruptor).

| Zip | Versión | Nota |
|---|---|---|
| `dist/foundry-gasto-0.5.0.zip` | 0.5.0 | El mismo mod que `foundry-gasto/`. Su contador no se apaga solo al desactivar el mod. |

## Instalar el marketplace en otro equipo

```powershell
claude plugin marketplace add C:\ruta\a\my-factory
```

```powershell
claude plugin install foundry-gasto@my-factory
```
