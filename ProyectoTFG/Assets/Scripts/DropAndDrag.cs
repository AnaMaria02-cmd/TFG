using System.Collections.Generic;
using UnityEngine;

public class DropAndDrag : MonoBehaviour
{
    Vector3 mouseOffset;
    Vector3 worldPosition;

    public float snapDistance = 1f;
    public List<Transform> nodes = new List<Transform>();
    public Transform playerTransform;

    public static bool IsDraggingAnyPiece = false; // New static flag

    bool isAttached = false;
    bool isSelected = false;
    Transform attachedNode;

    private Plane dragPlane;
    private Camera cam;
    private Rigidbody rb;

    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        // Si empieza suelta, activar física normal
        if (rb != null && !isAttached)
            rb.isKinematic = false;

        // Las piezas no deben bloquear ni empujar al jugador
      
    }

   
    private void Update()
    {
        // Rotación solo si está seleccionada
        if (isSelected && Input.GetMouseButtonDown(1))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.Rotate(90f, 0f, 0f, Space.World);
            }
            else
            {
                transform.Rotate(0f, 90f, 0f, Space.World);
            }
        }
    }

    private void OnMouseDown()
    {
        isSelected = true;
        IsDraggingAnyPiece = true; // Set flag

        // Al coger la pieza: kinematic para que no choque mientras se arrastra
        if (rb != null) rb.isKinematic = true;

        // Plano que bloquea eje Z
        dragPlane = new Plane(-cam.transform.forward, transform.position);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            mouseOffset = transform.position - ray.GetPoint(distance);
        }

        // Si estaba ensamblada → desensamblar
        if (isAttached)
        {
            Socket socket = attachedNode.GetComponent<Socket>();
            if (socket != null)
                socket.isOccupied = false;

            // Quitar de la jerarquía y devolver física
            transform.SetParent(null, true);
            if (rb != null) rb.isKinematic = false;

            Socket[] childSockets = GetComponentsInChildren<Socket>(true);
            foreach (Socket s in childSockets)
            {
                // Comprobar con childCount > 0 (más fiable que isOccupied,
                // que puede estar desincronizado). Si tiene hijos, significa
                // que hay una pieza conectada → no desactivar para no apagar
                // esa pieza (ej. pieza B) junto con el socket.
                if (s.transform.childCount > 0)
                    continue;

                s.gameObject.SetActive(false);
            }

            // Desconectar el joints que hubiéramos conectado al padre
            Rigidbody prevParentRb = attachedNode != null ? attachedNode.GetComponentInParent<Rigidbody>() : null;
            if (prevParentRb != null)
            {
                foreach (CharacterJoint cj in GetComponentsInChildren<CharacterJoint>())
                {
                    if (cj.connectedBody == prevParentRb)
                    {
                        cj.connectedBody = null;
                    }
                }
            }

            attachedNode = null;
            isAttached = false;
        }
    }

    private void OnMouseDrag()
    {
        if (isAttached) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        bool isPlayerMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isPlayerMoving)
        {
            // Mientras el jugador se mueve, bloqueamos la posición de la pieza en el mundo.
            // Actualizamos el offset para que cuando se detenga el jugador, la pieza no dé un salto.
            if (dragPlane.Raycast(ray, out float d))
            {
                mouseOffset = transform.position - ray.GetPoint(d);
            }
            return;
        }

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            transform.position = point + mouseOffset;
            worldPosition = transform.position;
        }
    }

    private void OnMouseUp()
    {
        isSelected = false;
        IsDraggingAnyPiece = false; // Reset flag

        // Solo devolver física si NO encajó en socket (se gestiona abajo)

        if (isAttached) return;

        Socket closestSocket = null;
        Socket closestMySocket = null;
        float smallestDistance = snapDistance;

        Socket[] mySockets = GetComponentsInChildren<Socket>(true);

        // 🔹 Buscar el mejor socket usando SocketManager o Fallback
        if (SocketManager.Instance != null)
        {
            if (mySockets.Length > 0)
            {
                foreach (Socket mySocket in mySockets)
                {
                    if (mySocket.transform.childCount > 0) continue;

                    Socket wSocket = SocketManager.Instance.GetClosestSocket(mySocket.transform.position, smallestDistance, this.transform);
                    if (wSocket != null)
                    {
                        float d = Vector3.Distance(mySocket.transform.position, wSocket.transform.position);
                        if (d < smallestDistance)
                        {
                            smallestDistance = d;
                            closestSocket = wSocket;
                            closestMySocket = mySocket;
                        }
                    }
                }
            }
            else
            {
                closestSocket = SocketManager.Instance.GetClosestSocket(worldPosition, snapDistance, this.transform);
            }
        }
        else
        {
            // 🔹 Fallback por si no existe el manager
            foreach (Socket worldSocket in FindObjectsOfType<Socket>())
            {
                if (worldSocket == null || worldSocket.isOccupied || !worldSocket.gameObject.activeInHierarchy)
                    continue;
                
                if (worldSocket.transform.IsChildOf(this.transform))
                    continue;

                if (mySockets.Length > 0)
                {
                    foreach (Socket mySocket in mySockets)
                    {
                        if (mySocket.transform.childCount > 0) continue;

                        float d = Vector3.Distance(mySocket.transform.position, worldSocket.transform.position);
                        if (d < smallestDistance)
                        {
                            smallestDistance = d;
                            closestSocket = worldSocket;
                            closestMySocket = mySocket;
                        }
                    }
                }
                else
                {
                    float d = Vector3.Distance(worldSocket.transform.position, worldPosition);
                    if (d < smallestDistance)
                    {
                        smallestDistance = d;
                        closestSocket = worldSocket;
                    }
                }
            }
        }

        // 🔹 Si encontramos socket válido
        if (closestSocket != null)
        {
            // ✅ ORDEN CORRECTO: kinematic ANTES de SetParent
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Guardar velocidad del padre (ej: jugador) antes del SetParent
            // para que el snap no frene su movimiento
            Rigidbody parentRb = closestSocket.GetComponentInParent<Rigidbody>();
            Vector3 savedVelocity = Vector3.zero;
            Vector3 savedAngular  = Vector3.zero;
            if (parentRb != null && parentRb != rb)
            {
                savedVelocity = parentRb.linearVelocity;
                savedAngular  = parentRb.angularVelocity;
            }

            // 🔹 Aplicar Offset si conectamos por un socket propio
            if (closestMySocket != null)
            {
                Vector3 offset = closestMySocket.transform.position - transform.position;
                transform.position = closestSocket.transform.position - offset;
            }
            else
            {
                transform.position = closestSocket.transform.position;
            }

            // No sobreescribir la rotación: conservar la que el usuario aplicó
            transform.SetParent(closestSocket.transform, true);

            // Restaurar velocidad del padre tras el SetParent
           /* if (parentRb != null && parentRb != rb)
            {
                parentRb.linearVelocity  = savedVelocity;
                parentRb.angularVelocity = savedAngular;
            }
            */

            // 🔹 LOGICA PARA CUERDAS (CharacterJoint)
            Rigidbody targetRb = closestSocket.GetComponentInParent<Rigidbody>();
            if (targetRb != null)
            {
                CharacterJoint candidateJoint = null;

                if (closestMySocket != null)
                {
                    // Se conectó desde un extremo/socket específico
                    candidateJoint = closestMySocket.GetComponentInParent<CharacterJoint>();
                }
                else
                {
                    // Se conectó desde la base
                    candidateJoint = GetComponent<CharacterJoint>();
                    if (candidateJoint == null)
                    {
                        float minJointDist = float.MaxValue;
                        foreach (CharacterJoint cj in GetComponentsInChildren<CharacterJoint>())
                        {
                            float dist = Vector3.Distance(cj.transform.position, closestSocket.transform.position);
                            if (dist < minJointDist)
                            {
                                minJointDist = dist;
                                candidateJoint = cj;
                            }
                        }
                    }
                }

                // 🔹 CONECTAMOS EL SEGMENTO AL JUGADOR (Sea la Base o la Punta)
                if (candidateJoint != null)
                {
                    candidateJoint.autoConfigureConnectedAnchor = true;
                    candidateJoint.connectedBody = targetRb;
                    Debug.Log($"[CUERDA CONEXION] Segmento enganchado al jugador: '{candidateJoint.gameObject.name}' -> '{targetRb.gameObject.name}'");

                    // 🔹 REORGANIZAMOS TODA LA CADENA DE CONEXIONES PARA QUE CUELGUEN DE ESTE SEGMENTO
                    // Esto asegura que, sin importar por dónde agarres la cuerda, el resto cuelga físicamente.
                    foreach (CharacterJoint cj in GetComponentsInChildren<CharacterJoint>())
                    {
                        if (cj == candidateJoint) continue;

                        Rigidbody towardsRootRb = null;

                        // Si el segmento al que estamos enganchados (candidateJoint) está DENTRO de los hijos de 'cj'
                        if (candidateJoint.transform.IsChildOf(cj.transform))
                        {
                            // La cadena fluye Hacia ABAJO (del padre al hijo) para llegar al punto enganchado
                            foreach (Transform child in cj.transform)
                            {
                                if (candidateJoint.transform == child || candidateJoint.transform.IsChildOf(child))
                                {
                                    towardsRootRb = child.GetComponent<Rigidbody>();
                                    if (towardsRootRb == null) towardsRootRb = child.GetComponentInChildren<Rigidbody>();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // La cadena fluye Hacia ARRIBA (del hijo al padre) para llegar al punto enganchado
                            if (cj.transform.parent != null)
                            {
                                towardsRootRb = cj.transform.parent.GetComponentInParent<Rigidbody>();
                            }
                        }

                        if (towardsRootRb != null)
                        {
                            cj.autoConfigureConnectedAnchor = true;
                            cj.connectedBody = towardsRootRb;
                            Debug.Log($"[CUERDA RE-ROOT] '{cj.gameObject.name}' reconectado hacia '{towardsRootRb.gameObject.name}' para colgar del nuevo enganche");
                        }
                    }
                }
            }

            closestSocket.isOccupied = true;

            // Activar los sockets propios de esta pieza
            Socket[] childSockets = GetComponentsInChildren<Socket>(true);
            foreach (Socket s in childSockets)
            {
                if (s.transform != closestSocket.transform)
                {
                    s.gameObject.SetActive(true);
                    if (closestMySocket != null && s == closestMySocket)
                    {
                        s.isOccupied = true;
                    }
                    else
                    {
                        s.isOccupied = false;
                    }
                }
            }

            attachedNode = closestSocket.transform;
            isAttached = true;
        }
        else
        {
            // No encajó: devolver física normal
            if (rb != null) rb.isKinematic = false;
        }
    }
    // ── Reenvíos desde ChildClickForwarder (avisaCollider) ───────────────────
    public void OnChildMouseDown() => OnMouseDown();
    public void OnChildMouseDrag() => OnMouseDrag();
    public void OnChildMouseUp()   => OnMouseUp();

}
