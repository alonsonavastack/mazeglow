# 🗂️ MazeGlow — Estado actual del proyecto
> Última actualización: 28 de abril 2026

---

## ✅ LO QUE ESTÁ HECHO

### Scripts (todos en Assets/Scripts/)
- 16 scripts base + 2 nuevos de cámara:
  - `Core/GameManager.cs`, `Core/GameController.cs`, `Core/SceneLoader.cs`, `Core/CelebrationFX.cs`, `Core/LocalizationManager.cs`
  - `Core/MazeCameraController.cs` ← NUEVO — cámara con vista cenital + transición isométrica
  - `Core/CameraFollow.cs` ← creado pero ya NO SE USA (reemplazado por MazeCameraController)
  - `Maze/MazeGenerator.cs` (v6) — llama a MazeCameraController al generar laberinto
  - `Maze/PlayerController.cs` — movimiento por DPad/swipe, detección de exit, partículas
  - `UI/UIManager.cs`, `UI/SettingsManager.cs`, `UI/ThemeManager.cs`
  - `UI/DPadController.cs` — direcciones relativas a la cámara (arreglado)
  - `UI/CameraJoystick.cs` — creado pero NO SE USA (fue eliminado del proyecto)
  - `Achievements/AchievementManager.cs`, `Save/SaveManager.cs`
  - `Monetization/AdManager.cs` (con OnInterstitialClosed), `Audio/AudioManager.cs`
  - `Notifications/NotificationManager.cs`, `Cloud/PlayFabManager.cs`

### Unity — Escena Game funcionando
- Laberinto 3D generado proceduralmente (algoritmo DFS)
- Jugador (bola naranja) se mueve por el laberinto con DPad
- Cámara: vista cenital 2s al inicio → transición suave → isométrica siguiendo al jugador
- HUD: Nivel, Monedas, Pistas, Vidas
- Iluminación: ambiental + luz puntual pulsante en la salida (Exit amarillo)
- **Configuración actual de cámara (MazeCameraController en Main Camera):**
  - Overview Duration: 2 | Overview Height: 22 | Overview Tilt: 88
  - Iso Height: 6 | Iso Distance: 5 | Iso Tilt: 35 | Iso Yaw: 20 (ajustando)
  - Transition Duration: 1.5 | Follow Smooth: 5

### Escenas creadas
- MainMenu (índice 0) — Canvas con JUGAR → SceneLoader.GoToGame ✅, CONFIGURACIÓN → GoToSettings ✅
- Game (índice 1) — laberinto jugable funcionando
- Results (índice 2) — UI básica, botón SIGUIENTE sin conectar
- Settings (índice 3) — UI básica, botones sin conectar

### Objetos en escena Game (Hierarchy)
- Main Camera + MazeCameraController (Player asignado)
- Directional Light
- EventSystem
- GameCanvas → PanelHUD, PanelGameOver (desactivado), DPad
- GameController
- Player (con PlayerController, Rigidbody, BoxCollider)
- MazeGenerator
- GameManager
- CelebrationFX

---

## ❌ LO QUE FALTA

### 🔴 Cámara — ajuste fino pendiente
- Iso Yaw ajustándose para centrar el laberinto en pantalla
- Al moverse el jugador, la cámara a veces deja parte del laberinto fuera de cuadro

### Botones sin conectar
- `BotonReintentar` → SceneLoader.GoToGame
- `BotonVerAnuncio` → AdManager.ShowRewarded
- `BotonSiguiente` (Results) → SceneLoader.GoToGame
- `BotonRegresar` (Settings) → SceneLoader.GoToMenu
- Toggles de Settings → SettingsManager

### GameObjects pendientes
- `_Managers` en escena Game con: SaveManager, AdManager, AudioManager, ThemeManager, LocalizationManager, AchievementManager, NotificationManager

### Assets externos pendientes de importar
- Google Mobile Ads SDK
- PlayFab SDK
- Unity Ads (Package Manager)
- Mobile Notifications (Package Manager)

### Configuración pendiente
- Player Settings: Bundle ID `com.tunombre.mazeglow`, Min API 23, IL2CPP, ARM64
- AdMob IDs reales en AdManager.cs
- Unity Ads Game ID en AdManager.cs
- PlayFab Title ID en PlayFabManager.cs
- URL Política de Privacidad en SettingsManager.cs

---

## 📋 PRÓXIMOS PASOS EN ORDEN

1. **Terminar ajuste de cámara** — Iso Yaw para centrar laberinto
2. **Conectar botones restantes** — Regresar, Siguiente, Reintentar, Ver Anuncio
3. **Agregar _Managers a escena Game** con todos los singletons
4. **Conectar UIManager** a los textos del HUD (TextNivel, TextMonedas, etc.)
5. **Probar flujo completo**: MainMenu → Game → completar nivel → Results → volver
6. **Instalar paquetes externos** — Google Mobile Ads, PlayFab, Unity Ads
7. **Configurar Player Settings** para Android
8. **Primera build en Android** para probar en dispositivo real
9. **Pulido visual** — efectos de partículas, sonidos, animaciones

---

## 🔧 DATOS TÉCNICOS
- Unity: 6000.4.4f1
- Plataforma objetivo: Android
- Bundle ID: com.tunombre.mazeglow
- Correo soporte: alonso.nava086@gmail.com
- Desarrollado en: Mac (Apple Silicon)
- Render: Standard (Built-in Pipeline)
