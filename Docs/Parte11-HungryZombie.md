# Parte 11: Hungry Zombie

## Integración

Hungry Zombie utiliza `Z` en `Assets/Levels/Dungeon01.txt`. La posición inicial es columna 39, fila 9 (índices desde cero), equivalente a `(18, -3)` en el mundo. Axy mantiene su inicio en `(-17, 0)`. La persecución atraviesa los dos pasillos y las tres habitaciones.

DungeonLevelLoader valida el archivo y las referencias antes de crear objetos. Construye NavigationGraph a partir de las mismas filas validadas, posiciona Axy y entrega jugador y grafo a GameObjectFactory.Create(). La Factory inicializa HungryZombieController antes de su primera actualización física. No se busca al jugador por tag ni se crea otro mapa manual.

La referencia a HungryZombie.prefab ya está agregada a la Factory de DungeonTestScene. Esa referencia es la única adición a la escena en esta etapa. Los mapas sin Z no requieren el nuevo prefab. La carga repetida mantiene su comportamiento idempotente.

El mapa tiene ahora 172 paredes, una salida y cuatro enemigos: Gazer, Skeleton, Coward Rat y Hungry Zombie. GeneratedDungeon contiene 177 instancias de mapa, además del Axy que ya existía en la escena. SampleScene y los recursos anteriores se conservan.

## Grafo y coordenadas

NavigationGraph es una clase C# normal. Conserva ancho y alto, y una matriz de referencias a GraphNode. No conserva una segunda copia editable del mapa ni inspecciona colliders para reconstruirlo.

- `#` y `S` no generan nodos transitables.
- `.`, `P`, `G`, `R`, `Z` y `E` generan nodos.
- Los enemigos móviles no modifican permanentemente el grafo.
- `GetNode(x, y)` recibe columna y fila y devuelve null para obstáculos o posiciones exteriores.
- `GetNeighbors(node)` devuelve hasta cuatro vecinos válidos: arriba, abajo, izquierda y derecha. Cada arista cuesta 1.

El nivel de 43 por 13 contiene 386 nodos navegables: se excluyen 172 paredes y la celda del Skeleton fijo. Se mantiene espacio entre los centros transitables y los colliders de tamaño actual.

CellToWorld, WorldToCell y GetNodeAtWorld centralizan las conversiones. DungeonLevelLoader delega ahora en CellToWorld; su fórmula anterior ya no está duplicada.

`worldX = columna - (ancho - 1) / 2`

`worldY = (alto - 1) / 2 - fila`

Cada celda mide una unidad y el origen sigue siendo mundial `(0, 0)`, independientemente del Transform de DungeonLevel. WorldToCell usa límites de celda con FloorToInt y no fuerza posiciones exteriores a entrar al mapa. Las dimensiones pares también conservan el centrado.

## A*

GraphNode contiene X, Y, GValue, HValue, F() y CameFrom. F() devuelve GValue + HValue.

AStarPathfinder recibe el grafo al construirse y FindPath(start, goal) recibe los nodos inicial y final. Cada búsqueda reinicia todos los costos a infinito, las heurísticas a cero y las referencias CameFrom a null. Después asigna GValue 0 al inicio y calcula su heurística.

La búsqueda utiliza una lista de pendientes y un conjunto de visitados. Selecciona el menor F, desempata con menor H, examina vecinos cardinales y actualiza GValue, HValue y CameFrom cuando mejora el costo. La heurística es Vector2.Distance entre las coordenadas de los nodos. Como la celda mide una unidad, representa la misma distancia euclidiana que en el mundo. Es admisible y consistente para cuatro vecinos de coste 1.

La reconstrucción sigue CameFrom desde el destino y revierte la lista: el resultado incluye inicio y objetivo en ese orden. Si el origen o destino es null, pertenece a otro grafo o no existe ruta, devuelve una lista vacía.

Las búsquedas son síncronas y secuenciales en el hilo principal. Cada controlador conserva su propia lista de ruta y solo usa las coordenadas inmutables de sus nodos al seguirla. El reinicio de costos durante otra búsqueda no modifica esas coordenadas ni el orden de las rutas ya devueltas.

## Controller y movimiento

HungryZombieController calcula cuándo buscar, qué nodo seguir, orientación y velocidad. EnemyPhysics no cambió: aplica la velocidad con Rigidbody2D.linearVelocity durante FixedUpdate, con gravedad cero y rotación física bloqueada.

El Controller utiliza DefaultExecutionOrder(-10), como Coward Rat, para entregar la velocidad antes de que EnemyPhysics la aplique. La orientación visual se aplica únicamente al hijo Visual del prefab.

El recorrido sigue centros de celdas y limita la velocidad de cada paso a la distancia restante dividida por fixedDeltaTime. Así no sobrepasa un nodo incluso con velocidades altas. La tolerancia de llegada permite avanzar al siguiente nodo sin oscilar alrededor del anterior. Al recalcular mientras avanza por un segmento recto, continúa hacia delante si ya está alineado, evitando regresar innecesariamente al centro anterior. Si necesita cambiar de dirección desde una posición desalineada, primero se centra para girar dentro de la celda.

Al completar el camino y compartir celda transitable con Axy, realiza la aproximación final a su posición actual sin otra búsqueda. Las paredes y el Skeleton se excluyen del recorrido. El cuerpo dinámico y el collider sólido siguen participando en la física normal.

## Frecuencia, fallos y recuperación

La primera actualización física calcula la ruta. Las siguientes búsquedas necesitan una causa y respetan el intervalo mínimo configurado:

- Axy cambia de celda.
- Una búsqueda anterior falló y el zombi cambia de celda inicial.
- El zombi fue desplazado fuera de su ruta.
- El siguiente nodo deja de ser válido.
- El cuerpo no avanza pese a tener velocidad solicitada durante Stuck Timeout.

Una ruta fallida se conserva como fallo para la pareja de celdas inicial/objetivo: no se reintenta por frame ni solamente porque venza el intervalo. Se vuelve a intentar cuando cambie alguna de esas celdas. Un destino inexistente detiene al zombi.

La detección de bloqueo compara desplazamiento real con el desplazamiento esperado. Tras el timeout permite recalcular, siempre respetando el intervalo. Si un enemigo móvil bloquea temporalmente el cuerpo, el seguimiento puede continuar cuando se libera el paso. No se implementó evasión avanzada de otros enemigos, ni modificación dinámica de la topología del mapa.

PathSearchCount es una propiedad de diagnóstico que permite comprobar la cantidad de búsquedas del controlador desde un depurador o las verificaciones automatizadas. No se imprime un mensaje por cada búsqueda.

## Parámetros del prefab

| Campo de HungryZombieController | Predeterminado | Unidad |
| --- | --- | --- |
| Speed | 2 | unidades/s |
| Repath Interval | 0,35 | segundos; mínimo 0,01 |
| Stuck Timeout | 1 | segundos; mínimo 0,1 |
| Node Arrival Distance | 0,02 | unidades; entre 0,001 y 0,1 |
| Visual | hijo Visual | referencia local |

HungryZombie.prefab reutiliza el recurso circular del proyecto, con forma ovalada y color verde. Tiene tag Enemy, Rigidbody2D Dynamic, Gravity Scale 0, Freeze Rotation Z, Collision Detection Continuous y CircleCollider2D sólido de radio 0,35. EnemyPhysics es el mismo componente de la Parte 10. CowardRatController, CowardRat.prefab, Gazer, Skeleton y Arrow no se modificaron.

PlayerOutcomeController sigue notificando derrota al recibir una colisión con tag Enemy. GlobalListener, GameManager, AudioController, la victoria y el reinicio con R conservan su implementación.

## Verificaciones automatizadas

Desde la raíz del proyecto se pueden repetir las pruebas del grafo y A* con:

```powershell
python Tests/run_navigation_checks.py
```

El ejecutor requiere Python, un SDK de .NET y el Editor de Unity ya instalado. No descarga paquetes ni modifica la configuración del proyecto. Si Unity está instalado en otra ubicación, pasar la ruta a Editor/Data como primer argumento. Los resultados compilados se guardan exclusivamente bajo Temp/Part11Validation, ignorado por Git.

Tests/NavigationChecks.cs compila junto con las tres clases reales de navegación y usa las estructuras matemáticas de UnityEngine.CoreModule. Verifica caminos rectos, obstáculos, Skeleton, destinos inaccesibles, ausencia de diagonales, reconstrucción, origen igual a destino, nodos nulos o ajenos, reinicio de datos, coordenadas pares e impares y el mapa real. Compara además 100 rutas con una búsqueda BFS de referencia independiente del algoritmo A*.

En esta implementación pasaron 10.553 comprobaciones de navegación. También pasaron 1.957 comprobaciones del Controller y la integración usando APIs de Unity simuladas, más las 150 comprobaciones simuladas de regresión de Coward Rat. Los 21 scripts del proyecto compilan contra las referencias instaladas de Unity. Las advertencias de campos serializados asignados por Inspector no son errores de compilación.

Estas comprobaciones no ejecutan Unity Editor ni Play Mode, y la simulación de seguimiento no incluye el solucionador físico nativo.

## Pruebas manuales pendientes en Unity

1. Esperar la importación de scripts y prefabs. Usar Assets > Refresh si no aparecen y comprobar Console.
2. Abrir HungryZombie.prefab y verificar cuerpo dinámico, collider sólido, ambos componentes y referencia Visual. En DungeonTestScene, confirmar Hungry Zombie Prefab en GameObjectFactory.
3. Entrar en Play sin crear una instancia manual adicional. Confirmar un solo Hungry Zombie en GeneratedDungeon, con 177 objetos de mapa, y OriginalLevel desactivado.
4. Mantener a Axy en la primera habitación y observar que el zombi parte de la tercera, rodea las paredes y cruza ambos pasillos. Comprobar que no atraviese al Skeleton ni se quede oscilando en esquinas.
5. Mover Axy a otras celdas y comprobar el cambio de recorrido. Dentro de la misma celda, comprobar su aproximación final. Probar distintas velocidades y el intervalo de búsqueda desde el prefab.
6. Probar colisiones temporales con enemigos móviles, recuperación después de empujes y bloqueo. La resolución real de estas colisiones debe validarse en Play Mode.
7. Confirmar derrota al tocar el zombi, audio existente y un solo resultado por partida. Pulsar R y comprobar que se generan de nuevo una sola rata y un solo zombi.
8. Verificar Wander/Flee de Coward Rat, patrullaje del Gazer, flechas del Skeleton y victoria al llegar a Exit.

## Limitaciones

El grafo es estático durante la partida y representa la geometría del TXT, no paredes añadidas manualmente fuera del mapa. Se asume la escala unitaria y los tamaños de collider actuales. Los enemigos móviles se resuelven mediante física, sin reservas de celdas ni evasión avanzada; un bloqueo permanente podría requerir intervención o ajustes futuros. El cambio de posición del Skeleton requeriría reconstruir su representación fija. No se agregaron NavMesh, corutinas, programación paralela ni paquetes externos.
