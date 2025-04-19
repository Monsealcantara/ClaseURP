using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWay : MonoBehaviour
{
    public WayCreator[] pathFollow;  // Aquí se asignarán las rutas
    public int currentWayPointID;
    public float rotSpeed;
    public float speed;
    public float reachDistance = 0.1f;

    public int currentPathIndex = 0; // Índice de la ruta actual
    public int way = 0;

    private int touchCount = 0; // Contador de toques
    private bool isInAir = true; // Para saber si el avión está volando
    private bool hasFallen = false; // Para saber si ya cayó al suelo

    private Animator animator;  // Variable para controlar el Animator

    Vector3 last_position;
    Vector3 current_position;

    void Start()
    {
        last_position = transform.position;
        currentWayPointID = 0;  // Inicializamos el primer punto de la ruta
        animator = GetComponent<Animator>();  // Obtener el componente Animator
    }

    void Update()
    {
        // Detectar toques con Raycast (para dispositivos táctiles o clics en PC)
        if (Input.GetMouseButtonDown(0) && !hasFallen)  // Detectar el clic (o toque)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // Proyectamos un rayo desde el ratón (o toque)

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)  // Si el rayo tocó el avión
                {
                    touchCount++;

                    // Si el avión ha sido tocado tres veces, cae al suelo
                    if (touchCount == 3)
                    {
                        FallToGround();  // Llamamos a la función para hacer que el avión caiga
                    }
                    else
                    {
                        ActivateShakeAnimation();  // Activar la animación de tambaleo
                        IncreaseSpeed();  // Aumentar la velocidad
                    }
                }
            }
        }

        if (isInAir && !hasFallen)
        {
            // Verificamos la distancia al siguiente punto de la ruta actual
            float distance = Vector3.Distance(pathFollow[way].path_objs[currentWayPointID].position, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, pathFollow[way].path_objs[currentWayPointID].position, Time.deltaTime * speed);

            // Calcular la dirección hacia el siguiente punto
            Vector3 targetDirection = pathFollow[way].path_objs[currentWayPointID].position - transform.position;

            // Limitar la dirección solo al plano XZ (ignorando Y)
            targetDirection.y = 0;  // Esto garantiza que solo gire en el plano horizontal (eje Y)

            // Si la dirección es válida, gira hacia el siguiente punto
            if (targetDirection != Vector3.zero)
            {
                // Calculamos el ángulo para rotar sobre el eje Y
                float angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg; // Rotación en el plano XZ
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * rotSpeed); // Rotación suave
            }

            // Si hemos llegado al siguiente punto, ir al siguiente
            if (distance <= reachDistance)
            {
                currentWayPointID++;
            }

            // Si hemos llegado al final de la ruta actual, ir a la siguiente
            if (currentWayPointID >= pathFollow[way].path_objs.Count)
            {
                currentWayPointID = 0;
                if (way == 1)
                {
                    way = 0;
                    speed = 0.2f;
                }
            }
        }
    }

    void ActivateShakeAnimation()
    {
        // Activar la animación de tambaleo en el Animator
        animator.SetBool("IsShaking", true);

        // Opcional: Desactivar la animación después de que haya terminado
        // Puedes agregar un pequeño retraso o usar un evento para que la animación se desactive automáticamente después de reproducirse
        StartCoroutine(DeactivateShakeAnimation());
    }

    IEnumerator DeactivateShakeAnimation()
    {
        // Esperar a que termine la animación de tambaleo
        yield return new WaitForSeconds(1f);  // Suponiendo que la animación dura 1 segundo

        // Desactivar el parámetro "IsShaking" después de que termine la animación
        animator.SetBool("IsShaking", false);
    }

    void IncreaseSpeed()
    {
        // Aumentar la velocidad con cada toque
        speed += 0.2f;
    }

    void FallToGround()
    {
        // Detener el vuelo y hacer que el avión caiga al suelo
        isInAir = false;
        hasFallen = true;

        // Puedes animar o mover el avión hacia el suelo de manera rápida
        // En este caso, movemos el avión directamente al "suelo" (pista).
        Vector3 groundPosition = new Vector3(transform.position.x, 0, transform.position.z);  // Suponiendo que el suelo está en y = 0
        transform.position = Vector3.Lerp(transform.position, groundPosition, Time.deltaTime * 2);  // Movimiento suave hasta el suelo
    }
}




//*/*/*/*/*/*/Código en el que solamente sirve para girar en Y en dirección de los puntos//*/**/*/*/*
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class MoveWay : MonoBehaviour
// {
//     public WayCreator[] pathFollow;  // Aquí se asignarán las rutas
//     public int currentWayPointID;
//     public float rotSpeed;
//     public float speed;
//     public float reachDistance = 0.1f;

//     public int currentPathIndex = 0; // Índice de la ruta actual
//     public int way = 0;

//     private int touchCount = 0; // Contador de toques
//     private bool isInAir = true; // Para saber si el avión está volando
//     private bool hasFallen = false; // Para saber si ya cayó al suelo

//     Vector3 last_position;
//     Vector3 current_position;

//     void Start()
//     {
//         last_position = transform.position;
//         currentWayPointID = 0;  // Inicializamos el primer punto de la ruta
//     }

//     void Update()
//     {
//         if (Input.touchCount > 0 && !hasFallen)  // Detectar el toque solo si el avión no ha caído
//         {
//             touchCount++;

//             // Si el avión ha sido tocado tres veces, cae al suelo
//             if (touchCount == 3)
//             {
//                 FallToGround();  // Llamamos a la función para hacer que el avión caiga
//             }
//             else
//             {
//                 IncreaseSpeed();  // Aumentar la velocidad
//             }
//         }

//         if (isInAir && !hasFallen)
//         {
//             // Verificamos la distancia al siguiente punto de la ruta actual
//             float distance = Vector3.Distance(pathFollow[way].path_objs[currentWayPointID].position, transform.position);
//             transform.position = Vector3.MoveTowards(transform.position, pathFollow[way].path_objs[currentWayPointID].position, Time.deltaTime * speed);

//             // Calcular la dirección hacia el siguiente punto
//             Vector3 targetDirection = pathFollow[way].path_objs[currentWayPointID].position - transform.position;

//             // Limitar la dirección solo al plano XZ (ignorando Y)
//             targetDirection.y = 0;  // Esto garantiza que solo gire en el plano horizontal (eje Y)

//             // Si la dirección es válida, gira hacia el siguiente punto
//             if (targetDirection != Vector3.zero)
//             {
//                 // Calculamos el ángulo para rotar sobre el eje Y
//                 float angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg; // Rotación en el plano XZ
//                 transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * rotSpeed); // Rotación suave
//             }

//             // Si hemos llegado al siguiente punto, ir al siguiente
//             if (distance <= reachDistance)
//             {
//                 currentWayPointID++;
//             }

//             // Si hemos llegado al final de la ruta actual, ir a la siguiente
//             if (currentWayPointID >= pathFollow[way].path_objs.Count)
//             {
//                 currentWayPointID = 0;

//                 // Cambiar a la siguiente ruta, si es necesario
//                 currentPathIndex++;

//                 // Si hemos terminado todas las rutas, reiniciar
//                 if (currentPathIndex >= pathFollow.Length)
//                 {
//                     currentPathIndex = 0;
//                 }
//             }
//         }
//     }

//     void IncreaseSpeed()
//     {
//         // Aumentar la velocidad con cada toque
//         speed += 0.2f;
//     }

//     void FallToGround()
//     {
//         // Detener el vuelo y hacer que el avión caiga al suelo
//         isInAir = false;
//         hasFallen = true;

//         // Puedes animar o mover el avión hacia el suelo de manera rápida
//         // En este caso, movemos el avión directamente al "suelo" (pista).
//         Vector3 groundPosition = new Vector3(transform.position.x, 0, transform.position.z);  // Suponiendo que el suelo está en y = 0
//         transform.position = Vector3.Lerp(transform.position, groundPosition, Time.deltaTime * 2);  // Movimiento suave hasta el suelo
//     }
// }

