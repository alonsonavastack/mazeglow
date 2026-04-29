# 🚀 MazeGlow — Guía de Inicio Paso a Paso

## ¿Qué scripts se crearon y para qué sirve cada uno?

| Archivo | Carpeta | Para qué sirve |
|---------|---------|----------------|
| `GameManager.cs` | Core | El cerebro del juego. Guarda vidas, monedas, nivel, configuración |
| `GameController.cs` | Core | Controla el flujo: iniciar nivel, ganar, perder, entre niveles |
| `SceneLoader.cs` | Core | Cambia de pantalla con transición suave |
| `CelebrationFX.cs` | Core | Confeti y efectos al ganar un nivel |
| `LocalizationManager.cs` | Core | Traducciones español/inglés |
| `MazeGenerator.cs` | Maze | Genera laberintos automáticos que crecen con el nivel |
| `PlayerController.cs` | Maze | Mueve al jugador con swipe táctil |
| `AchievementManager.cs` | Achievements | Logros diarios, semanales y permanentes |
| `SaveManager.cs` | Save | Guarda y carga TODO el progreso en JSON |
| `UIManager.cs` | UI | Controla HUD, paneles, popups, celebración |
| `SettingsManager.cs` | UI | Pantalla de Configuración completa |
| `ThemeManager.cs` | UI | Modo oscuro y modo claro |
| `AudioManager.cs` | Audio | Todos los sonidos y música del juego |
| `AdManager.cs` | Monetization | AdMob + Unity Ads (recompensados, intersticiales, banners) |
| `NotificationManager.cs` | Notifications | Notificaciones locales de Android |
| `PlayFabManager.cs` | Cloud | Respaldo en la nube con PlayFab |

---

## 📋 PASO 1 — Crear el proyecto en Unity

1. Abre Unity Hub
2. Clic en **New Project**
3. Plantilla: **2D Core**
4. Nombre: `MazeGlow`
5. Ubicación: tu Desktop (o donde prefieras)
6. Clic en **Create project**

---

## 📋 PASO 2 — Copiar los scripts al proyecto

1. En el Finder, abre la carpeta `MazeGlow/Assets/Scripts/` que generamos
2. Copia todas las carpetas (`Core`, `Maze`, `UI`, etc.) dentro de la carpeta `Assets` de tu proyecto Unity
3. Unity detectará automáticamente los scripts y los compilará

---

## 📋 PASO 3 — Crear las escenas

En Unity ve a **File > New Scene** y crea estas 4 escenas. Guárdalas en `Assets/Scenes/`:

| Nombre de escena | Descripción |
|-----------------|-------------|
| `MainMenu` | Pantalla de inicio con botón "Jugar" |
| `Game` | Pantalla donde se juega el laberinto |
| `Results` | Pantalla de resultados al terminar |
| `Settings` | Pantalla de configuración |

Luego ve a **File > Build Settings** y arrastra las 4 escenas al listado, en ese orden.

---

## 📋 PASO 4 — Configurar el GameManager (objeto persistente)

1. En la escena `MainMenu`, crea un **GameObject vacío** y nómbralo `_Managers`
2. Arrastra estos scripts al objeto `_Managers`:
   - `GameManager`
   - `SaveManager`
   - `SceneLoader`
   - `AudioManager`
   - `AchievementManager`
   - `AdManager`
   - `NotificationManager`
   - `LocalizationManager`
   - `ThemeManager`
   - `PlayFabManager`
3. Todos estos son **DontDestroyOnLoad**, es decir, vivirán en todas las escenas automáticamente

---

## 📋 PASO 5 — Configurar la escena de Juego

1. En la escena `Game`, crea un GameObject vacío → `MazeGenerator`
   - Agrega el script `MazeGenerator.cs`
   - Crea prefabs de Wall y Floor (rectángulos simples de colores) y asígnalos
2. Crea un GameObject → `Player`
   - Agrega el script `PlayerController.cs`
   - Agrega un `Collider2D` (para detectar colisiones)
3. Crea un GameObject vacío → `GameController`
   - Agrega el script `GameController.cs`
   - Asigna referencias: MazeGenerator, Player, CelebrationFX

---

## 📋 PASO 6 — Instalar paquetes externos

Ve a **Window > Package Manager** en Unity e instala:

| Paquete | Cómo encontrarlo |
|---------|-----------------|
| Mobile Notifications | Buscar "Mobile Notifications" |
| TextMeshPro | Buscar "TextMeshPro" (ya viene incluido) |

Descarga e importa manualmente:
- **Google Mobile Ads SDK**: https://github.com/googleads/googleads-mobile-unity/releases (descarga el .unitypackage más reciente)
- **PlayFab SDK**: https://github.com/PlayFab/UnitySDK/releases

---

## 📋 PASO 7 — Configurar AdMob

1. Ve a https://admob.google.com → crea cuenta → agrega app → Android → MazeGlow
2. Crea 3 bloques de anuncios: Recompensado, Intersticial, Banner
3. Copia los IDs y pégalos en `AdManager.cs` (las constantes al inicio del archivo)
4. En Unity: **Assets > Google Mobile Ads > Settings** → pega el App ID de AdMob
5. Mientras desarrollas, usa los IDs de prueba que ya están comentados en `AdManager.cs`

---

## 📋 PASO 8 — Configurar Unity Ads

1. Ve a https://dashboard.unity3d.com
2. Crea un proyecto → plataforma Android
3. Copia el **Game ID** y pégalo en `AdManager.cs` (constante `UNITY_GAME_ID`)
4. En Unity: **Window > Package Manager** → busca "Advertisement Legacy" → instala

---

## 📋 PASO 9 — Configurar Google Play Billing (compras)

1. En Google Play Console crea 3 productos de pago único (no suscripciones):
   - `mazeglow_no_ads_1month` — $86.00 MXN
   - `mazeglow_no_ads_3months` — $200.00 MXN
   - `mazeglow_no_ads_6months` — $340.00 MXN
2. Instala el plugin de Unity IAP: **Window > Package Manager** → "In App Purchasing"
3. Conecta los IDs en `SettingsManager.cs` (método `PurchasePlan`)

---

## 📋 PASO 10 — Configurar Player Settings para Android

Ve a **Edit > Project Settings > Player**:

| Campo | Valor |
|-------|-------|
| Company Name | Tu nombre |
| Product Name | MazeGlow |
| Package Name | com.tunombre.mazeglow |
| Minimum API Level | Android 6.0 (API 23) |
| Target API Level | Automatic |
| Scripting Backend | IL2CPP |
| Target Architectures | ARMv7 + ARM64 |

---

## 🎨 CÓMO MODIFICAR EL JUEGO

### Cambiar dificultad de los laberintos
Abre `MazeGenerator.cs` y cambia:
- `baseWidth` / `baseHeight` → tamaño inicial del laberinto
- `growEvery` → cada cuántos niveles crece

### Agregar un nuevo logro
Abre `AchievementManager.cs`, ve al método `InitializeAchievements()` y agrega:
```csharp
allAchievements.Add(new Achievement
{
    id          = "mi_nuevo_logro",    // ID único
    name        = "Mi Logro",
    description = "Descripción aquí",
    category    = AchievementCategory.Daily, // Daily, Weekly o Permanent
    targetEvent = AchievementEvent.LevelCompleted,
    targetCount = 10,
    rewardCoins = 100,
    rewardLives = 0
});
```

### Cambiar precios de los planes
Abre `SettingsManager.cs` → método `PurchasePlan` → cambia los `productIds`
Y en Google Play Console, edita los precios de los productos IAP.

### Cambiar el correo de soporte
Abre `SettingsManager.cs` → constante `SUPPORT_EMAIL`

### Agregar un nuevo idioma
Abre `LocalizationManager.cs` → agrega `{"fr", "texto en francés"}` en cada entrada del diccionario

---

## 📦 ASSETS GRATUITOS RECOMENDADOS

### Unity Asset Store (busca estos nombres exactos)
- **"Particle Pack"** — Unity Technologies — Gratis oficial
- **"Simple Confetti Particle Effects"** — Confeti colorido
- **"FREE Casual Game SFX Pack"** — Efectos de sonido casuales
- **"Simple FX - Cartoon Particles"** — Brillos y explosiones

### Sonidos (sitios gratuitos)
- https://freesound.org — Requiere cuenta gratuita, miles de opciones
- https://mixkit.co/free-sound-effects/ — Sin cuenta, listo para usar
- https://www.zapsplat.com — Requiere cuenta gratuita

### Fuentes gratuitas (Google Fonts)
- **Nunito** — Amigable, redonda, perfecta para juegos casuales
- **Poppins** — Limpia y moderna
- Descargar en: https://fonts.google.com

---

*Generado automáticamente para el proyecto MazeGlow*
*Desarrollador: alonso.nava086@gmail.com*
