# Notas: el mundo de Jaime

Notas en bruto para `/loop-me`. Los términos canónicos están en **negrita**.

## Trabajo

- Es consultor en Qaracter y forma parte del **Equipo IA**.
- Construye assets de IA para el **catálogo QRG** (Azure DevOps) y prepara **demos** para clientes.
- Sobre todo desarrolla **proyectos** de IA:
  - **QONBOARDING** (el actual): `C:\Users\jaime.rodriguez\projects\WORKSPACE-QRG-QONBOARDING`.
    - Es un workspace orquestador cuyo remoto está en Azure DevOps (`QaracterProjects / QRG - QONBOARDING`).
    - La rama del workspace es `user/jaime.rodriguez`.
    - Los clones hijos son `APP-QONBOARDING-BACKEND` (ahora en `demos`), `APP-QONBOARDING-FRONTEND` (ahora en `dev`) e `INFRA-AZ-QRG-QONBOARDING` (`main`).
    - La lista de repos está en `ado/ado-project.json`; `bootstrap.ps1` mantiene los clones al día.
    - `.\bootstrap.ps1 -Status` muestra la fase, el trabajo abierto y los restos de trabajo terminado.
    - Skills propias del workspace: `/where` (dónde estamos y qué toca), `/start-task`, `/prepare-pr` y `/close-task`.
    - Regla del proyecto: después de cada merge a `dev` se abre una PR `dev` → `demos`.
    - El **board** de Azure DevOps (Epic > Issue > Task) es la verdad del trabajo. Teams y Planner van por encima.
  - **ValidAI**: `C:\Users\jaime.rodriguez\projects\ValidAI`. No es un repo git en la raíz; contiene `validai/` y `my-local-docs/`.

## Canales

- **Teams**: los canales del Equipo IA y los chats.
- **Outlook**: el correo.
- Todavía no ha probado si un agente puede leer su Teams o su Outlook de empresa.
- Skill `teams365-equipo-ia` (en `~/.claude/skills/`, y la fuente en SharePoint `Equipo IA - Documents/General/_Herramientas/claude-skills/`):
  - Usa Microsoft Graph con permisos delegados ("Teams 365 - Claude Toolkit") y **solo funciona en el Team Equipo IA**.
  - Lee los **posts de los canales** (`Get-RecentPosts [-MentioningMe]`), el Roadmap de Planner, las ausencias y el calendario.
  - **No lee chats 1:1 ni chats de grupo**: no tiene el scope `Chat.Read`. Tampoco lee el correo.
- **Tareas del team lead**: le llegan **por un chat 1:1 de Teams** con el lead, no por el board.
- Para leer chats hace falta el permiso delegado `Chat.Read` de Graph (para listar chats basta `Chat.ReadBasic`).
  - Microsoft indica que no requiere consentimiento de administrador por defecto, aunque la política del tenant puede exigirlo.
  - La app del skill (`ClientId ef6b0833-…`, tenant de Qaracter) no pide ese scope.
  - El skill lo mantienen los **propietarios del Team Equipo IA**. Jaime no sabía que la app existía ni quién la controla.
- Regla de Jaime: investigar sin saltarse controles. No se firma ni se pide ningún consentimiento sin su permiso.
- **Decisión (2026-11-20)**: de momento Teams se queda como está. Jaime le dice a Claude en el prompt lo que tiene que hacer ese día.
- El **team lead** de QONBOARDING es **Fernando**.
- En el PC está instalado `Microsoft.Graph.Authentication` 2.41.2; `Microsoft.Graph.Teams` no.
- En el workspace, `bootstrap.ps1` respeta `.claude/settings.local.json`: no lo borra. Ese fichero está en su `.gitignore`.

## Rutina diaria (tal como la cuenta)

1. Llega a la oficina, abre el ordenador y abre Claude, Teams y Outlook.
2. Abre el proyecto actual (QONBOARDING) y **trae los cambios** que han hecho los demás.
3. Mira las **tareas que le ha mandado el team lead** y empieza a aplicarlas.

- Trabaja de lunes a viernes y empieza hacia las 9:00. Apaga o no deja el PC preparado, así que prefiere un trigger por evento.
- No usa `/where` ni `bootstrap.ps1 -Status`.
- El workspace ya tiene un hook `SessionStart` (`.claude/hooks/session-start.ps1 -Full`) que imprime la fase.
- **Traer cambios** significa dos cosas: hacer pull de los clones y **entender** qué han mergeado los demás y si toca lo suyo.

## Permisos del agente

- Puede abrir PRs.
- **Nunca** envía nada ni hace merge sin su consentimiento.
- No sabe qué otros límites hay; está por explorar.
