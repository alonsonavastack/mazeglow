# 🌿 MazeGlow — Prompt Maestro del Proyecto
> Documento de referencia completo para el desarrollo del juego MazeGlow en Unity + C# para Android.
> **Última actualización:** Versión 2.2 — Estado actual del proyecto agregado.

---

## 🎮 TIPO DE JUEGO

Juego de laberintos infinitos, estilo casual, con gráficos limpios y bonitos. Debe tener efectos visuales y sonoros iguales a los de Candy Crush: explosiones, brillos, partículas, animaciones suaves, sonidos alegres al ganar, al moverse y al completar niveles.

---

## ⚙️ FUNCIONALIDADES PRINCIPALES

1. Generación automática de laberintos infinitos, que aumenten de dificultad conforme avanza el jugador
2. Sistema de vidas, pistas y monedas como moneda del juego
3. Controles sencillos y fluidos para celular (toque / swipe)
4. Pantallas de: menú principal, selección de nivel, juego, resultados y configuración
5. Instrucción visual de "Toca para mover" en la primera sesión del jugador (onboarding)
6. Selector de idioma: español e inglés como mínimo, con arquitectura lista para agregar más
7. Modo oscuro y modo claro, con toggle en configuración (el jugador elige)

---

## 🏆 SISTEMA DE LOGROS (LO MÁS IMPORTANTE)

Sistema completo de logros dividido en 3 categorías con sus respectivas recompensas:

### 📅 LOGROS DIARIOS
Se reinician todos los días a la medianoche.
- Ejemplos: "Jugar 5 niveles hoy", "Completar 3 niveles sin chocar", "Entrar 3 días seguidos", "Usar 2 pistas"
- Recompensas: monedas, vidas extra

### 🗓️ LOGROS SEMANALES
Se reinician todos los lunes.
- Ejemplos: "Jugar 30 niveles en la semana", "Conseguir 100 estrellas", "Completar 5 niveles difíciles", "Entrar todos los días de la semana"
- Recompensas: monedas, objetos especiales

### 🏆 LOGROS GENERALES / PERMANENTES
Nunca se borran.
- Ejemplos: "Completar 100 niveles", "Alcanzar nivel 50", "Ver 50 anuncios", "Conseguir todos los logros diarios por 1 mes"
- Recompensas: grandes cantidades de monedas, quitar anuncios temporalmente

### Requisitos de cada logro:
- Nombre, descripción, barra de progreso, icono, animación de desbloqueo
- Entrega automática de recompensas
- Notificaciones push cuando hay logros disponibles para reclamar

---

## 🗄️ SISTEMA DE ALMACENAMIENTO

### Local
- Usar JSON + PlayerPrefs para guardar TODO el progreso: niveles alcanzados, monedas, vidas, estado de logros, fechas de inicio de sesión, preferencias de configuración (idioma, modo oscuro, sonidos, vibraciones)
- Debe funcionar completamente sin internet

### Nube
- Integración lista para conectar con **PlayFab de Microsoft**
- Funciones: respaldar datos, recuperar progreso al cambiar de celular, enviar notificaciones push remotas
- Incluir configuración básica lista para activar cuando se desee
- **Login con Google Account y Google Play Games**: el jugador puede conectar su cuenta para respaldar su progreso. Mostrar modal de "Conectar cuenta" con botones: "Iniciar sesión con Google" e "Iniciar sesión con Play Games"

---

## 💰 SISTEMA DE MONETIZACIÓN

### Google AdMob
- ✅ Anuncios recompensados: para ganar vidas, pistas, monedas extra
- ✅ Anuncios intersticiales: entre niveles o al perder
- ✅ Banners: visibles en menús y pantalla de juego

### Unity Ads
- Configurado como complemento para maximizar ganancias, compitiendo con AdMob

### Eliminar anuncios — Modelo de suscripción por tiempo (NO pago único)
Ofrecer 3 planes de compra desde la pantalla de Configuración > "Eliminar anuncios":
- **1 mes** — $86.00 MXN
- **3 meses** — $200.00 MXN (~$66.67 MXN/mes)
- **6 meses** — $340.00 MXN (~$56.67 MXN/mes)

Aclaración visible al jugador: *"La compra no se renueva automáticamente"*

Beneficios al comprar:
- ✅ Sin anuncios forzados entre niveles
- ✅ Los anuncios de pistas y recargas de vidas siguen disponibles (opcionales y recompensados)

Botón de **"Restaurar compras"** obligatorio: permite al jugador recuperar una compra anterior si cambia de celular o reinstala el juego. Requerido por Google Play.

---

## ⚙️ PANTALLA DE CONFIGURACIÓN (Settings)

La pantalla de configuración debe tener las siguientes secciones y opciones:

### Sección 1 — Preferencias
| Opción | Tipo | Descripción |
|--------|------|-------------|
| Idioma | Selector (→) | Abre sub-pantalla para elegir idioma. Mínimo: Español, English |
| Vibraciones | Toggle on/off | Activa/desactiva la vibración háptica del dispositivo |
| Sonidos | Toggle on/off | Activa/desactiva todos los efectos de sonido y música |
| Modo oscuro | Toggle on/off | Cambia entre tema oscuro y tema claro en toda la app |

### Sección 2 — Cuenta
| Opción | Tipo | Descripción |
|--------|------|-------------|
| Conexión de cuenta | Toggle / botón | Abre modal para conectar con Google o Play Games |

### Sección 3 — Compras
| Opción | Tipo | Descripción |
|--------|------|-------------|
| Eliminar anuncios | Toggle / botón | Abre modal con los 3 planes de suscripción |
| Restaurar compras | Botón | Restaura compras previas del jugador vía Google Play Billing |

### Sección 4 — Comunidad
| Opción | Tipo | Descripción |
|--------|------|-------------|
| Califícanos | Botón | Abre la página del juego en Google Play Store |
| Escríbenos | Botón | Abre el cliente de correo con email de soporte prellenado: alonso.nava086@gmail.com |

### Sección 5 — Legal
| Opción | Tipo | Descripción |
|--------|------|-------------|
| Privacidad | Botón | Abre la Política de Privacidad en un WebView o navegador externo |

> ⚠️ **IMPORTANTE:** La Política de Privacidad es **OBLIGATORIA** para publicar en Google Play, especialmente si el juego usa AdMob (que recolecta datos para publicidad). Ver sección "Política de Privacidad" al final de este documento para instrucciones de cómo crearla gratis.

---

## ✨ EFECTOS Y ESTILO VISUAL

- Todos los efectos visuales (partículas, brillos, explosiones, confeti de celebración) basados en paquetes **gratuitos de la Asset Store** — indicar cuáles descargar
- Celebración al completar nivel: confeti animado + texto "¡Impresionante!" u otras frases aleatorias
- Sonidos y música alegres — indicar sitios para descargarlos gratis
- Interfaz limpia, colorida, estilo Candy Crush, fácil de leer y usar en pantallas táctiles
- Soporte para **modo oscuro** (fondo #1a1a2e, azul/violeta) y **modo claro** (fondo blanco/gris claro)
- Tipografía clara y grande, apta para todas las edades

---

## 🔔 SISTEMA DE NOTIFICACIONES

- Notificaciones locales (no requieren internet) para:
  - Logros disponibles para reclamar
  - Vidas recargadas
  - Recordatorio diario para mantener la racha
- Pedir permiso al primer inicio con modal nativo de Android: "¿Permitir que MazeGlow te envíe notificaciones?"
- Notificaciones remotas vía PlayFab (configuración base lista para activar)

---

## 📌 ENTREGABLES ESPERADOS

1. Estructura completa de carpetas del proyecto Unity
2. Todos los scripts de C# organizados y comentados
3. Instrucciones paso a paso para:
   - Importar todo en Unity
   - Configurar AdMob y Unity Ads (poner los IDs propios)
   - Configurar Google Play Billing (para los planes de suscripción)
   - Configurar Login con Google / Play Games
   - Configurar PlayFab para respaldo en la nube
   - Crear y publicar la Política de Privacidad (ver sección abajo)
4. Lista de assets gratuitos recomendados para efectos, gráficos y sonidos
5. Explicación sencilla de cómo:
   - Modificar los laberintos o agregar niveles
   - Agregar más logros o cambiar recompensas
   - Cambiar los precios de los planes de suscripción
   - Cambiar el correo de soporte en "Escríbenos"

---

## 🔐 POLÍTICA DE PRIVACIDAD — Cómo crearla gratis (sin página web propia)

Google Play exige una URL pública con la Política de Privacidad del juego. Como MazeGlow aún no tiene sitio web, usar este método gratuito:

### Opción recomendada: GitHub Pages (gratis y permanente)
1. Crear una cuenta gratuita en https://github.com
2. Crear un repositorio nuevo llamado `mazeglow-privacy`
3. Subir un archivo `index.html` con el texto de la política de privacidad
4. Activar GitHub Pages en la configuración del repositorio (Settings > Pages)
5. La URL resultante será algo como: `https://tunombre.github.io/mazeglow-privacy`
6. Esa URL se pone en Google Play Console y dentro del juego (botón Privacidad)

### Alternativa: Google Sites (aún más fácil)
1. Ir a https://sites.google.com con tu cuenta de Gmail (alonso.nava086@gmail.com)
2. Crear un sitio nuevo llamado "MazeGlow Privacy"
3. Pegar el texto de la política de privacidad
4. Publicar — Google asigna una URL pública gratuita automáticamente

### Contenido mínimo que debe tener la Política de Privacidad:
- Qué datos recolecta el juego (datos de uso, identificadores de publicidad vía AdMob)
- Para qué se usan esos datos (mostrar anuncios personalizados)
- Con quién se comparten (Google AdMob, Unity Ads)
- Cómo el usuario puede optar por no recibir anuncios personalizados
- Datos de contacto del desarrollador: alonso.nava086@gmail.com
- Fecha de vigencia de la política

> 💡 Se puede usar el generador gratuito de políticas de privacidad en: https://app-privacy-policy-generator.nisrulz.com (específico para apps de Android/iOS)

---

## 🎯 OBJETIVO FINAL

Juego funcional listo para publicar en Google Play. El objetivo es generar entre **$5 y más de $100 MXN diarios** mediante monetización con anuncios y compras dentro del juego.

**Timeline estimado: 6-7 semanas**

---

## 📁 DATOS DEL PROYECTO

| Campo | Valor |
|-------|-------|
| Nombre del juego | MazeGlow |
| Plataforma | Android |
| Motor | Unity (C#) |
| Bundle ID | com.tunombre.mazeglow |
| Estilo visual | Natural / Mágico (bosques, cristales, luz) |
| Audiencia | Todos los públicos |
| Idiomas | Español (principal), English |
| Correo de soporte | alonso.nava086@gmail.com |
| URL Política de Privacidad | Pendiente — ver sección "Política de Privacidad" para crearlo gratis |

---

## 📊 ESTADO ACTUAL DEL PROYECTO
> Última actualización: 27 de abril de 2026

### ✅ COMPLETADO

#### Documentación
- [x] Prompt maestro del proyecto (este archivo) — Versión 2.2
- [x] Guía de inicio paso a paso (`GUIA_INICIO.md`)

#### Scripts de C# generados (16 archivos)
- [x] `Core/GameManager.cs` — Singleton central, vidas, monedas, configuración
- [x] `Core/GameController.cs` — Flujo de partida: iniciar, ganar, perder, entre niveles
- [x] `Core/SceneLoader.cs` — Cambio de escenas con transición
- [x] `Core/CelebrationFX.cs` — Confeti y efectos visuales al ganar nivel
- [x] `Core/LocalizationManager.cs` — Sistema de idiomas (español / inglés)
- [x] `Maze/MazeGenerator.cs` — Generación automática de laberintos (algoritmo DFS)
- [x] `Maze/PlayerController.cs` — Movimiento por swipe táctil + control de teclado
- [x] `Achievements/AchievementManager.cs` — Logros diarios, semanales y permanentes (8 logros definidos)
- [x] `Save/SaveManager.cs` — Guardado local en JSON + PlayerPrefs
- [x] `UI/UIManager.cs` — HUD, paneles, popup de logros, celebración
- [x] `UI/SettingsManager.cs` — Pantalla de configuración completa (5 secciones)
- [x] `UI/ThemeManager.cs` — Modo oscuro y modo claro en tiempo real
- [x] `Audio/AudioManager.cs` — Pool de sonidos + música de fondo
- [x] `Monetization/AdManager.cs` — AdMob + Unity Ads (todos los formatos)
- [x] `Notifications/NotificationManager.cs` — Notificaciones locales Android
- [x] `Cloud/PlayFabManager.cs` — Respaldo en nube con PlayFab (base lista)

#### Estructura de carpetas
- [x] `Assets/Scripts/` con 9 subcarpetas organizadas por módulo
- [x] `Assets/Scenes/`, `Assets/Prefabs/`, `Assets/Sprites/`, `Assets/Audio/`, `Assets/Fonts/`, `Assets/Plugins/`

---

### 🔄 EN PROGRESO / PENDIENTE DE HACER EN UNITY

#### Semana 1 — Configuración del proyecto
- [ ] Crear proyecto Unity 2D y copiar los scripts generados
- [ ] Crear las 4 escenas: MainMenu, Game, Results, Settings
- [ ] Configurar Build Settings (Android) y Player Settings
- [ ] Crear objeto `_Managers` y asignar todos los singletons

#### Semana 2 — Laberinto jugable
- [ ] Crear prefabs de Wall, Floor, Player, Exit
- [ ] Conectar MazeGenerator con los prefabs en el Inspector
- [ ] Probar generación de laberinto en el editor
- [ ] Probar movimiento del jugador con teclado y simular swipe

#### Semana 3 — Interfaz de usuario
- [ ] Diseñar y construir el Canvas del HUD (corazones, monedas, nivel)
- [ ] Pantalla de MainMenu con botón Jugar y navegación
- [ ] Panel de Game Over con botón Reintentar y Ver anuncio
- [ ] Pantalla de Configuración (5 secciones) conectada a SettingsManager
- [ ] Modal de "Conectar cuenta" (Google / Play Games)
- [ ] Modal de "Eliminar anuncios" (3 planes)
- [ ] Popup de logro desbloqueado con animación

#### Semana 4 — Efectos y audio
- [ ] Descargar e importar assets de partículas gratuitos (Asset Store)
- [ ] Asignar ParticleSystems a PlayerController y CelebrationFX
- [ ] Descargar efectos de sonido gratuitos (freesound.org / mixkit.co)
- [ ] Asignar AudioClips al AudioManager en el Inspector
- [ ] Implementar modo oscuro / claro visual completo con ThemeManager

#### Semana 5 — Monetización
- [ ] Crear cuenta en Google AdMob y obtener IDs reales
- [ ] Importar Google Mobile Ads Unity Plugin
- [ ] Desactivar comentarios en AdManager.cs y pegar IDs
- [ ] Configurar Unity Ads (Game ID)
- [ ] Probar anuncios recompensados, intersticiales y banner
- [ ] Configurar Google Play Billing para los 3 planes de suscripción

#### Semana 6 — Logros, notificaciones y nube
- [ ] Instalar Mobile Notifications Package y activar NotificationManager
- [ ] Probar notificaciones locales en dispositivo Android real
- [ ] Instalar PlayFab SDK y activar login anónimo
- [ ] Probar guardado y carga desde la nube
- [ ] Agregar más logros según feedback de pruebas

#### Semana 7 — Pulido y publicación
- [ ] Crear ícono del juego y splash screen con estilo MazeGlow
- [ ] Crear Política de Privacidad (Google Sites o GitHub Pages)
- [ ] Crear cuenta de desarrollador en Google Play Console ($25 USD pago único)
- [ ] Subir APK / AAB firmado a Google Play
- [ ] Completar ficha de la app (descripción, capturas de pantalla, categoría)
- [ ] Enviar a revisión de Google Play

---

### ⚠️ PENDIENTES CRÍTICOS (bloquean la publicación)

| # | Tarea | Por qué es crítica |
|---|-------|--------------------|
| 1 | Crear Política de Privacidad con URL pública | Google Play la exige para apps con AdMob |
| 2 | Obtener IDs reales de AdMob | Sin esto los anuncios no aparecen |
| 3 | Cuenta de Google Play Console ($25 USD) | Sin esto no se puede publicar |
| 4 | Firmar el APK/AAB | Requerido por Google Play para distribución |

---

### 📝 NOTAS Y DECISIONES TOMADAS

- Se eligió **algoritmo DFS (Recursive Backtracker)** para generar laberintos porque produce caminos con muchos giros, ideal para dificultad progresiva
- Los anuncios recompensados **no se eliminan** aunque el jugador pague — solo los forzados (intersticiales), para no quitar la opción de ganar vidas/monedas extra
- El modelo de "suscripción sin renovación automática" fue elegido para ser más honesto con el jugador y evitar cancelaciones/chargebacks
- Los logros semanales se reinician **los lunes**, no los domingos, porque es más intuitivo para el jugador hispanohablante
