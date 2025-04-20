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

    public ParticleSystem explosionParticles;  // Explosión al caer
    public ParticleSystem collisionParticles;  // Partículas del avión al chocar

    private Material planeMaterial; // Material del avión para modificar el shader

    // Referencia al Animator de la hélice
    public Animator heliceAnimator;

    void Start()
    {
        last_position = transform.position;
        currentWayPointID = 0;  // Inicializamos el primer punto de la ruta
        animator = GetComponent<Animator>();  // Obtener el componente Animator

        // Asegurarse de que las partículas de explosión estén desactivadas al inicio
        explosionParticles.Stop();
        collisionParticles.Stop();  // Desactivar las partículas de colisión al inicio

        // Instanciamos el material para modificarlo individualmente
        planeMaterial = GetComponent<Renderer>().material;
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

        // Activar la animación de caída
        animator.SetBool("IsFalling", true);

        // Activar el sistema de partículas de explosión
        explosionParticles.Play();

        // Desactivar las partículas de colisión (si estaban activadas previamente)
        collisionParticles.Stop();

        // Detener la animación de la hélice (la animación se detiene al caer)
        StopHeliceAnimation();

        // Iniciar el efecto de disolución
        StartCoroutine(DissolveEffect());
        // Aquí puedes hacer que la animación de caída controle el movimiento hacia el suelo
        // No moveremos el avión manualmente, todo lo manejará la animación
    }

    // Detener la animación de la hélice cuando el avión cae
    void StopHeliceAnimation()
    {
        if (heliceAnimator != null)
        {
            // Detener la animación de la hélice al caer
            heliceAnimator.enabled = false;  // Desactivar el Animator de la hélice
            // O también podrías poner un estado Idle:
            // heliceAnimator.Play("Idle"); // Si tienes un estado llamado "Idle" para la hélice
        }
    }

    // Corutina para modificar los valores de disolución
    IEnumerator DissolveEffect()
    {
        float dissolveTime = 5f;  // Tiempo que tardará el disolverse 
        float targetDissolve = 0.8f;  // Valor máximo de disolución
        float targetEdgeDissolve = 0.5f;  // Valor inicial de Edge Dissolve al tocar el suelo
        float currentDissolve = 0f;
        float currentEdgeDissolve = 10f;  // Inicializamos EdgeDisolve en 10

        // Gradualmente modificamos el valor de Disolve y EdgeDisolve
        while (currentDissolve < targetDissolve && currentEdgeDissolve > 0)
        {
            currentDissolve += Time.deltaTime / dissolveTime * targetDissolve;  // Aumentar gradualmente el valor de dissolve
            currentEdgeDissolve -= Time.deltaTime / dissolveTime * (targetEdgeDissolve - 0);  // Disminuir gradualmente el valor de EdgeDissolve

            // Asignar los nuevos valores a los parámetros del shader
            planeMaterial.SetFloat("_Disolve", currentDissolve);
            planeMaterial.SetFloat("_EdgeDisolve", currentEdgeDissolve);

            yield return null;  // Espera el siguiente frame
        }

        // Asegurarse de que se lleguen a los valores finales
        planeMaterial.SetFloat("_Disolve", targetDissolve);
        planeMaterial.SetFloat("_EdgeDisolve", 0f); // Asegurarnos de que EdgeDisolve termine en 0
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
}


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

//     private Animator animator;  // Variable para controlar el Animator

//     Vector3 last_position;
//     Vector3 current_position;

//     public ParticleSystem explosionParticles;  // Explosión al caer
//     public ParticleSystem collisionParticles;  // Partículas del avión al chocar

//     private Material planeMaterial; // Material del avión para modificar el shader

//     void Start()
//     {
//         last_position = transform.position;
//         currentWayPointID = 0;  // Inicializamos el primer punto de la ruta
//         animator = GetComponent<Animator>();  // Obtener el componente Animator

//         // Asegurarse de que las partículas de explosión estén desactivadas al inicio
//         explosionParticles.Stop();
//         collisionParticles.Stop();  // Desactivar las partículas de colisión al inicio

//         // Instanciamos el material para modificarlo individualmente
//         planeMaterial = GetComponent<Renderer>().material;
//     }

//     void Update()
//     {
//         // Detectar toques con Raycast (para dispositivos táctiles o clics en PC)
//         if (Input.GetMouseButtonDown(0) && !hasFallen)  // Detectar el clic (o toque)
//         {
//             RaycastHit hit;
//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // Proyectamos un rayo desde el ratón (o toque)

//             if (Physics.Raycast(ray, out hit))
//             {
//                 if (hit.transform == transform)  // Si el rayo tocó el avión
//                 {
//                     touchCount++;

//                     // Si el avión ha sido tocado tres veces, cae al suelo
//                     if (touchCount == 3)
//                     {
//                         FallToGround();  // Llamamos a la función para hacer que el avión caiga
//                     }
//                     else
//                     {
//                         ActivateShakeAnimation();  // Activar la animación de tambaleo
//                         IncreaseSpeed();  // Aumentar la velocidad
//                     }
//                 }
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
//                 if (way == 1)
//                 {
//                     way = 0;
//                     speed = 0.2f;
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

//         // Activar la animación de caída
//         animator.SetBool("IsFalling", true);

//         // Activar el sistema de partículas de explosión
//         explosionParticles.Play();

//         // Desactivar las partículas de colisión (si estaban activadas previamente)
//         collisionParticles.Stop();

//         // Iniciar el efecto de disolución
//         StartCoroutine(DissolveEffect());
//         // Aquí puedes hacer que la animación de caída controle el movimiento hacia el suelo
//         // No moveremos el avión manualmente, todo lo manejará la animación
//     }

//     // Corutina para modificar los valores de disolución
//     IEnumerator DissolveEffect()
//     {
//         float dissolveTime = 6f;  // Tiempo que tardará el disolverse 
//         float targetDissolve = 0.8f;  // Valor máximo de disolución
//         float targetEdgeDissolve = 0.21f;  // Valor máximo de Edge Dissolve
//         float currentDissolve = 0f;
//         float currentEdgeDissolve = 0f;

//         // Gradualmente modificamos el valor de Disolve y EdgeDisolve
//         while (currentDissolve < targetDissolve && currentEdgeDissolve < targetEdgeDissolve)
//         {
//             currentDissolve += Time.deltaTime / dissolveTime * targetDissolve;  // Aumentar gradualmente el valor de dissolve
//             currentEdgeDissolve += Time.deltaTime / dissolveTime * targetEdgeDissolve;  // Aumentar gradualmente el valor de EdgeDissolve

//             // Asignar los nuevos valores a los parámetros del shader
//             planeMaterial.SetFloat("_Disolve", currentDissolve);
//             planeMaterial.SetFloat("_EdgeDisolve", currentEdgeDissolve);

//             yield return null;  // Espera el siguiente frame
//         }

//         // Asegurarse de que se lleguen a los valores finales
//         planeMaterial.SetFloat("_Disolve", targetDissolve);
//         planeMaterial.SetFloat("_EdgeDisolve", targetEdgeDissolve);
//     }

//     void ActivateShakeAnimation()
//     {
//         // Activar la animación de tambaleo en el Animator
//         animator.SetBool("IsShaking", true);

//         // Opcional: Desactivar la animación después de que haya terminado
//         // Puedes agregar un pequeño retraso o usar un evento para que la animación se desactive automáticamente después de reproducirse
//         StartCoroutine(DeactivateShakeAnimation());
//     }

//     IEnumerator DeactivateShakeAnimation()
//     {
//         // Esperar a que termine la animación de tambaleo
//         yield return new WaitForSeconds(1f);  // Suponiendo que la animación dura 1 segundo

//         // Desactivar el parámetro "IsShaking" después de que termine la animación
//         animator.SetBool("IsShaking", false);
//     }
// }





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

//     private Animator animator;  // Variable para controlar el Animator

//     Vector3 last_position;
//     Vector3 current_position;

//     void Start()
//     {
//         last_position = transform.position;
//         currentWayPointID = 0;  // Inicializamos el primer punto de la ruta
//         animator = GetComponent<Animator>();  // Obtener el componente Animator
//     }

//     void Update()
//     {
//         // Detectar toques con Raycast (para dispositivos táctiles o clics en PC)
//         if (Input.GetMouseButtonDown(0) && !hasFallen)  // Detectar el clic (o toque)
//         {
//             RaycastHit hit;
//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // Proyectamos un rayo desde el ratón (o toque)

//             if (Physics.Raycast(ray, out hit))
//             {
//                 if (hit.transform == transform)  // Si el rayo tocó el avión
//                 {
//                     touchCount++;

//                     // Si el avión ha sido tocado tres veces, cae al suelo
//                     if (touchCount == 3)
//                     {
//                         FallToGround();  // Llamamos a la función para hacer que el avión caiga
//                     }
//                     else
//                     {
//                         ActivateShakeAnimation();  // Activar la animación de tambaleo
//                         IncreaseSpeed();  // Aumentar la velocidad
//                     }
//                 }
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
//                 if (way == 1)
//                 {
//                     way = 0;
//                     speed = 0.2f;
//                 }
//             }
//         }
//     }

//     void ActivateShakeAnimation()
//     {
//         // Activar la animación de tambaleo en el Animator
//         animator.SetBool("IsShaking", true);

//         // Opcional: Desactivar la animación después de que haya terminado
//         // Puedes agregar un pequeño retraso o usar un evento para que la animación se desactive automáticamente después de reproducirse
//         StartCoroutine(DeactivateShakeAnimation());
//     }

//     IEnumerator DeactivateShakeAnimation()
//     {
//         // Esperar a que termine la animación de tambaleo
//         yield return new WaitForSeconds(1f);  // Suponiendo que la animación dura 1 segundo

//         // Desactivar el parámetro "IsShaking" después de que termine la animación
//         animator.SetBool("IsShaking", false);
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




