# Parte 9: generación del nivel con Factory

Esta etapa agrega `GameObjectFactory` y `DungeonLevelLoader`. Los scripts están preparados para configurar desde Unity. `SampleScene`, la cámara, los prefabs, el mapa y los scripts anteriores permanecen sin cambios. La generación real y los ajustes posteriores de enemigos todavía requieren pruebas en el Editor.

## Archivo y simbología

`Assets/Levels/Dungeon01.txt` es un TextAsset de texto UTF-8, sin cabecera ni leyenda dentro del mapa. Contiene 13 filas de 43 caracteres. Las filas se leen de arriba hacia abajo y las columnas de izquierda a derecha.

| Símbolo | Significado | Acción |
| --- | --- | --- |
| `#` | Pared sólida | Instanciar `Assets/Prefabs/Wall.prefab` |
| `.` | Espacio transitable | No crear un objeto |
| `P` | Inicio del jugador | Reubicar el Rigidbody2D del Axy existente |
| `G` | Blind Gazer | Instanciar `Assets/Prefabs/BlindGazer.prefab` |
| `S` | Drunk Skeleton | Instanciar `Assets/Prefabs/DrunkSkeleton.prefab` |
| `E` | Salida | Instanciar `Assets/Prefabs/Exit.prefab` |

El cargador acepta LF, CRLF o CR, un BOM inicial opcional y saltos de línea al final. Rechaza archivos vacíos, filas de distinto ancho, símbolos desconocidos y mapas sin exactamente un `P` y un `E`. Valida todo antes de mover al jugador o crear el contenedor. No admite espacios ni comentarios dentro del mapa.

## Funcionamiento y coordenadas

`DungeonLevelLoader.Start()` llama a `LoadLevel()`. El cargador obtiene el componente `GameObjectFactory` del mismo GameObject y utiliza sus cuatro prefabs. La Factory selecciona el prefab según el carácter y llama a `Instantiate`, conservando sus componentes, rotación y escala. No reconstruye enemigos ni modifica sus variables.

Los objetos se agrupan bajo un contenedor raíz llamado `GeneratedDungeon`, con transformación identidad. La posición del objeto DungeonLevelLoader no desplaza el mapa. Cada celda mide una unidad de Unity. Para índices desde cero:

`x = columna - (ancho - 1) / 2`, `y = (alto - 1) / 2 - fila`, `z = 0`.

El cálculo usa mitades decimales, por lo que también centra mapas con dimensiones pares. En Dungeon01 equivale a `x = columna - 21`, `y = 6 - fila`.

| Objeto | Columna | Fila | Posición `(x, y)` |
| --- | --- | --- | --- |
| Axy | 4 | 6 | `(-17, 0)` |
| Gazer | 21 | 6 | `(0, 0)` |
| Skeleton | 36 | 6 | `(15, 0)` |
| Salida | 40 | 3 | `(19, 3)` |

Las tres habitaciones tienen interiores de 11 por 11 celdas, en columnas 1 a 11, 16 a 26 y 31 a 41, y filas 1 a 11. Los pasillos ocupan columnas 12 a 15 y 27 a 30, en filas 5 a 7.

El recorrido absoluto del Gazer entre `y = -3` y `y = 3` queda dentro de la segunda habitación. Skeleton tiene espacio para su radio de aparición de flechas de 3 unidades. Otro mapa debe respetar esos límites mientras no se configure el comportamiento de los enemigos en la etapa siguiente.

`LoadLevel()` es idempotente: mientras exista el contenedor, otra llamada conserva los objetos y sus estados, sin duplicarlos ni reubicar otra vez a Axy. No es un reinicio de partida ni restablece GlobalListener. Para reiniciar después del resultado se conserva R, que recarga la escena mediante GameManager; la nueva instancia del cargador genera el nivel nuevamente. Las flechas actuales del Skeleton siguen siendo creadas por su script original, fuera del contenedor del mapa; se descargan junto con la escena al reiniciar.

## Preparar una escena de prueba en Unity

1. Esperar a que Unity importe los scripts. Usar Assets > Refresh si no aparecen y comprobar que no haya errores de compilación.
2. Abrir SampleScene y usar File > Save As para guardar una copia como `Assets/Scenes/DungeonTestScene.unity`. Realizar el resto de los pasos únicamente en esa copia.
3. Crear un objeto vacío `OriginalLevel`. Mover dentro las cuatro paredes originales, Exit, BlindGazer y DrunkSkeletonArcher. Desactivar OriginalLevel antes de Play. No incluir Axy, GameManager, GlobalListener, Main Camera ni Global Light 2D en ese grupo.
4. Crear un objeto vacío `DungeonLevel`. Agregar DungeonLevelLoader: RequireComponent agrega automáticamente GameObjectFactory si falta. Ambos componentes deben estar en el mismo objeto.
5. En DungeonLevelLoader, asignar Level File a `Assets/Levels/Dungeon01.txt` y Player al Rigidbody2D de Axy de esa escena.
6. En GameObjectFactory, asignar Wall Prefab, Exit Prefab, Blind Gazer Prefab y Drunk Skeleton Prefab a los cuatro recursos de Assets/Prefabs con esos nombres. No arrastrar los objetos originales de la jerarquía a estos campos.
7. Conservar activos Axy con sus componentes, un solo GameManager y un solo GlobalListener. En DrunkSkeleton.prefab comprobar que SkeletonShooting conserve Arrow Prefab apuntando a `Assets/Prefabs/Arrow.prefab`.
8. Configurar la cámara únicamente en la copia y guardar. En File > Build Profiles > Scene List, agregar y habilitar DungeonTestScene para que GameManager pueda recargarla por nombre al presionar R. El perfil Windows actual usa la lista global de escenas.
9. Entrar en Play. Debe aparecer un contenedor GeneratedDungeon con 172 paredes, una salida y dos enemigos, sin una copia nueva de Axy. Verificar movimiento, colisiones, audio, victoria/derrota, un solo resultado y reinicio con R.

Si se mantiene activo el nivel original, sus paredes y enemigos interferirán con el generado. SampleScene debe permanecer como respaldo y no necesita ningún cambio.

## Cámara ortográfica

La cámara original sigue con tamaño 5. En DungeonTestScene usar Main Camera en posición `(0, 0, -10)`, rotación cero y proyección Orthographic. La mazmorra ocupa 43 por 13 unidades, incluyendo el tamaño de las paredes.

Con aspecto 16:9, Orthographic Size 13 permite ver las tres habitaciones completas con margen. Para un margen de una unidad y `aspecto = ancho de la ventana / alto de la ventana`, el mínimo es:

`max(13 / 2 + 1, (43 / 2 + 1) / aspecto)`.

| Aspecto de Game | Orthographic Size sugerido |
| --- | --- |
| 16:9 | 13 |
| 16:10 | 15 |
| 4:3 | 17 |
| 9:16 | 40 |

Fijar la relación de aspecto en la ventana Game durante las primeras pruebas. Con Free Aspect, volver a comprobar el encuadre si se redimensiona la ventana. No se agregó ajuste automático de cámara.

## Compatibilidad y trabajo pendiente

Los prefabs conservan sus tags y colliders. PhysicsController y PlayerOutcomeController siguen enviando notificaciones a GlobalListener. AudioController y GameManager mantienen sus suscripciones. Los enemigos conservan sus scripts, sus valores serializados y la referencia a Arrow. La Factory no crea instancias adicionales de Axy, GameManager, GlobalListener ni AudioController.

Quedan pendientes la importación e integración manual en DungeonTestScene, las pruebas reales en Play Mode, la confirmación del encuadre y la etapa posterior de configuración de velocidad y variables temporales de enemigos. La Parte 9 todavía no está terminada.
