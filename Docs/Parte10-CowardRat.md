# Parte 10: Coward Rat

## Integración

Coward Rat se crea desde `Assets/Levels/Dungeon01.txt` mediante el símbolo `R`. La celda elegida es columna 9, fila 3 (índices desde cero), posición mundial `(-12, 3)`. El inicio de Axy sigue en `(-17, 0)`: la distancia inicial es aproximadamente 5,83 unidades, por lo que la rata comienza en Wander.

`DungeonLevelLoader` valida el nuevo símbolo y la referencia al prefab antes de generar objetos. Entrega su referencia existente al Rigidbody2D de Axy a `GameObjectFactory.Create()`. La Factory llama a `CowardRatController.Initialize()` antes del primer paso de física. No se utiliza un tag Player ni se crea otro jugador.

La Factory de `DungeonTestScene` ya tiene asignado `Assets/Prefabs/CowardRat.prefab`. Esa es la única referencia añadida a la escena; SampleScene y todos los objetos y prefabs anteriores se conservan. Los mapas sin R no requieren el prefab nuevo. Repetir LoadLevel conserva su comportamiento idempotente.

El nivel genera ahora 172 paredes, una salida y tres enemigos bajo GeneratedDungeon, manteniendo al Axy existente. No se implementó la Parte 11.

## Componentes y comportamiento

`CowardRatController` decide Wander/Flee, calcula la orientación en grados y la velocidad en unidades por segundo. Ejecuta su FixedUpdate antes de EnemyPhysics mediante DefaultExecutionOrder(-10).

`EnemyPhysics` aplica la velocidad mediante Rigidbody2D.linearVelocity durante FixedUpdate. También proporciona consultas Rigidbody2D.Cast usando el collider del cuerpo y excluyendo triggers; no decide el comportamiento ni reproduce sonidos.

El prefab tiene tag Enemy, Rigidbody2D dinámico, gravedad cero, rotación física bloqueada, detección continua y CircleCollider2D sólido de radio 0,35. Un hijo Visual reutiliza el sprite circular del proyecto, con forma ovalada y color gris/marrón. El Controller gira únicamente ese hijo.

Wander mantiene la orientación actual y selecciona variaciones de hasta 45 grados cada 0,5 segundos. Gira progresivamente hacia la orientación elegida con un máximo de 90 grados por segundo. No elige una dirección independiente en cada frame.

Flee se activa cuando la distancia euclidiana a Axy es menor o igual a Detection Distance. En espacio libre utiliza la diferencia rata menos jugador, normalizada, multiplicada por Flee Speed. A más de esa distancia vuelve a Wander. Si las posiciones coinciden, conserva una dirección válida mediante el comportamiento de orientación; no normaliza una dirección nula.

Antes de avanzar, el Controller consulta si hay paredes en su dirección. Cuando encuentra una pared que bloquea el movimiento, elige una tangente libre. Si ambas tangentes están bloqueadas, intenta separarse según la normal o regresar. Mantiene brevemente esa dirección para evitar alternar cada paso contra el mismo obstáculo. Esta evasión local tiene prioridad sobre la dirección ideal de Wander/Flee cuando existe una pared; no calcula rutas. El contacto con Axy sigue provocando derrota mediante PlayerOutcomeController y GlobalListener.

## Parámetros del Inspector

Todos pertenecen a CowardRatController en CowardRat.prefab:

| Campo | Valor inicial | Unidad |
| --- | --- | --- |
| Wander Speed | 2 | unidades/s |
| Flee Speed | 3 | unidades/s |
| Detection Distance | 5 | unidades |
| Wander Turn Speed | 90 | grados/s |
| Wander Change Interval | 0,5 | segundos |
| Wander Angle Range | 45 | grados |
| Wall Look Ahead | 0,5 | unidades desde la forma del collider |
| Wall Avoidance Duration | 0,35 | segundos |
| Visual | hijo Visual | referencia local del prefab |

La consulta de paredes también considera la distancia a recorrer durante el paso físico y un pequeño margen. Los valores no negativos y los intervalos mínimos se protegen al utilizarlos. La velocidad cero detiene el desplazamiento.

## Comprobación manual en Unity

1. Esperar a que Unity termine de importar los scripts y el prefab. Ejecutar Assets > Refresh si no aparecen y comprobar que Console no muestre errores nuevos.
2. Abrir CowardRat.prefab en Prefab Mode. Confirmar Rigidbody2D Dynamic, Gravity Scale 0, Freeze Rotation Z, Collision Detection Continuous y collider sin Is Trigger. Confirmar Visual y ambos scripts.
3. Abrir DungeonTestScene y comprobar Coward Rat Prefab en GameObjectFactory. No agregar una instancia manual de la rata a la jerarquía.
4. Entrar en Play: GeneratedDungeon debe tener 176 objetos de mapa y solo una rata. Verificar que los objetos originales sigan desactivados.
5. Mantener a Axy lejos para observar Wander. Acercarlo hasta una distancia de 5 o menos para observar Flee; alejarlo para comprobar el retorno a Wander.
6. Probar paredes, esquinas y extremos de pasillos. Confirmar que la rata cambia de dirección y no atraviesa paredes ni queda permanentemente empujando el mismo obstáculo. Esta comprobación requiere la física real del Editor.
7. Cambiar Wander Speed y Flee Speed en el prefab, guardar y volver a Play para verificar los valores en las instancias creadas por Factory.
8. Comprobar derrota por contacto con la rata, sonido existente del jugador, Gazer, Skeleton, flechas y victoria al alcanzar Exit. Después de cada resultado, pulsar R y comprobar que se vuelve a crear una única rata.

No hace falta asignar manualmente el jugador en el prefab: la referencia procede del cargador. No se modificaron Observer, GameManager, PlayerOutcomeController ni los enemigos anteriores. Las verificaciones automatizadas con APIs simuladas no sustituyen estas pruebas de Play Mode.
