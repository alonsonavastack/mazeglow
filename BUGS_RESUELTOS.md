### 📝 NOTAS Y DECISIONES TOMADAS

- Se eligió **algoritmo DFS (Recursive Backtracker)** para generar laberintos porque produce caminos con muchos giros, ideal para dificultad progresiva
- Los anuncios recompensados **no se eliminan** aunque el jugador pague — solo los forzados (intersticiales), para no quitar la opción de ganar vidas/monedas extra
- El modelo de "suscripción sin renovación automática" fue elegido para ser más honesto con el jugador y evitar cancelaciones/chargebacks
- Los logros semanales se reinician **los lunes**, no los domingos, porque es más intuitivo para el jugador hispanohablante
- Se migró de **2D a 3D** porque el objeto Player ya era 3D en Unity y causaba conflictos con SpriteRenderer
- La detección del Exit usa **OnTriggerEnter 3D** con flag `levelCompleted` para evitar doble disparo

---

### 🐛 BUGS RESUELTOS

| # | Bug | Solución aplicada |
|---|-----|-------------------|
| 1 | SpriteRenderer conflicto con MeshFilter | Migración completa a 3D |
| 2 | Emojis 🪙💡 no renderizaban en LiberationSans | Reemplazados por texto simple |
| 3 | Exit duplicado (estático en editor + dinámico) | ClearMaze() destruye TODOS los hijos |
| 4 | NextLevelRoutine esperaba 30s (callback inexistente) | Eliminado el wait, avanza en 2s |
| 5 | OnLevelComplete llamado 2 veces | Flag `levelCompleted` en Player y GameController |
| 6 | Laberinto no generaba (referencias null) | GameController auto-busca referencias con FindAnyObjectByType |
| 7 | Escena Game sin GameManager al abrir directo | BootstrapGame.cs crea managers automáticamente |
| 8 | CS0019: ?? entre Material y Shader | Reemplazado por if/else explícito |
