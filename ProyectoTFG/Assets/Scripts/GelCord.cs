using UnityEngine;

/// <summary>
/// Cordón de gelatina que:
///  · Sale cuando el cursor pasa por encima de una pieza (DropAndDrag).
///  · La punta sigue la pieza; si el cursor sale o la pieza se pega al jugador, el cordón retrae.
///  · Mientras se arrastra una pieza, permanece retraído.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class GelCord : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Referencias")]
    [Tooltip("Punto del cuerpo del jugador desde el que sale el cordón.")]
    public Transform playerOrigin;

    [Header("Rango y Geometría")]
    public float maxRange = 5f;
    [Range(8, 64)]
    public int resolution = 24;

    [Header("Anchura / Estiramiento")]
    public float baseWidth    = 0.15f;
    public float minTipWidth  = 0.02f;

    [Header("Comportamiento Orgánico")]
    public float gravitySag       = 0.4f;
    public float wobbleAmplitude  = 0.10f;
    public float wobbleFrequency  = 3.5f;

    [Header("Retracción")]
    [Tooltip("Velocidad (u/s) con la que la punta vuelve al origen.")]
    public float retractSpeed = 10f;

    [Header("Detección de Piezas")]
    [Tooltip("LayerMask para el Raycast del cursor. Por defecto detecta todo.")]
    public LayerMask pieceLayerMask = ~0;
    [Tooltip("Distancia máxima del Raycast de detección.")]
    public float detectMaxDist = 100f;

    // ── Estado ────────────────────────────────────────────────────────────────
    private enum CordState { Hidden, Extending, Retracting }
    private CordState state = CordState.Hidden;

    // ── Privados ──────────────────────────────────────────────────────────────
    private LineRenderer lr;
    private Camera        cam;
    private Vector3       tipPosition;    // posición 3D actual de la punta
    private Transform     hoveredPiece;   // Transform de la pieza bajo el cursor

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount     = resolution;
        lr.useWorldSpace     = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows    = false;
        lr.enabled           = false;

        cam = Camera.main;
        if (playerOrigin == null) playerOrigin = transform;
        tipPosition = playerOrigin.position;
    }

    private void Update()
    {
        DetectPieceUnderCursor();
        UpdateState();
        if (lr.enabled) DrawCord();
    }

    // ── Detección ─────────────────────────────────────────────────────────────
    private void DetectPieceUnderCursor()
    {
        // Mientras se arrastra algo, no detectar piezas nuevas
        if (DropAndDrag.IsDraggingAnyPiece)
        {
            hoveredPiece = null;
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, detectMaxDist, pieceLayerMask))
        {
            DropAndDrag dd = hit.collider.GetComponentInParent<DropAndDrag>();
            hoveredPiece = (dd != null) ? dd.transform : null;
        }
        else
        {
            hoveredPiece = null;
        }
    }

    // ── Máquina de estados ────────────────────────────────────────────────────
    private void UpdateState()
    {
        switch (state)
        {
            // ── OCULTO: espera que el cursor pase sobre una pieza
            case CordState.Hidden:
                lr.enabled = false;
                if (hoveredPiece != null)
                {
                    tipPosition = playerOrigin.position; // sale desde el origen
                    lr.enabled  = true;
                    state       = CordState.Extending;
                }
                break;

            // ── EXTENDIDO: punta sigue la pieza detectada
            case CordState.Extending:
                if (hoveredPiece == null || DropAndDrag.IsDraggingAnyPiece)
                {
                    // Cursor salió de la pieza, o la pieza fue agarrada/pegada → retraer
                    state = CordState.Retracting;
                    break;
                }

                // La punta va hacia la pieza, limitada al maxRange
                Vector3 toTarget = hoveredPiece.position - playerOrigin.position;
                float   dist     = Mathf.Min(toTarget.magnitude, maxRange);
                tipPosition = playerOrigin.position + toTarget.normalized * dist;
                break;

            // ── RETRACTANDO: la punta vuelve al origen suavemente
            case CordState.Retracting:
                tipPosition = Vector3.MoveTowards(tipPosition, playerOrigin.position,
                                                  retractSpeed * Time.deltaTime);

                if (Vector3.Distance(tipPosition, playerOrigin.position) < 0.05f)
                {
                    lr.enabled = false;
                    state      = CordState.Hidden;

                    // Si justo al terminar de retraer el cursor ya está sobre otra pieza,
                    // empezamos a extender de nuevo sin esperar un frame extra
                    if (hoveredPiece != null && !DropAndDrag.IsDraggingAnyPiece)
                    {
                        tipPosition = playerOrigin.position;
                        lr.enabled  = true;
                        state       = CordState.Extending;
                    }
                }
                break;
        }
    }

    // ── Dibujo del cordón ─────────────────────────────────────────────────────
    private void DrawCord()
    {
        Vector3 origin = playerOrigin.position;
        Vector3 tipPos = tipPosition;

        float stretchRatio = (maxRange > 0f)
            ? Mathf.Clamp01(Vector3.Distance(origin, tipPos) / maxRange)
            : 0f;

        // Anchura dinámica
        lr.startWidth = baseWidth;
        lr.endWidth   = Mathf.Lerp(baseWidth, minTipWidth, stretchRatio);

        // Eje perpendicular relativo a la cámara
        Vector3 cordDir = (tipPos - origin).normalized;
        if (cordDir.sqrMagnitude < 0.001f) cordDir = cam.transform.forward;

        Vector3 perp = Vector3.Cross(cordDir, cam.transform.up).normalized;
        if (perp.sqrMagnitude < 0.001f) perp = cam.transform.right;

        for (int i = 0; i < resolution; i++)
        {
            float t   = (float)i / (resolution - 1);
            Vector3 p = Vector3.Lerp(origin, tipPos, t);

            // Caída gravitatoria parabólica
            p.y += -gravitySag * stretchRatio * Mathf.Sin(t * Mathf.PI);

            // Oscilación lateral senoidal
            float envelope = Mathf.Sin(t * Mathf.PI); // 0 en bordes, 1 en centro
            p += perp * (wobbleAmplitude * envelope
                        * Mathf.Sin(t * Mathf.PI * 2f + Time.time * wobbleFrequency));

            lr.SetPosition(i, p);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (playerOrigin == null) return;
        UnityEditor.Handles.color = new Color(0.2f, 1f, 0.5f, 0.3f);
        UnityEditor.Handles.DrawWireDisc(playerOrigin.position, Vector3.forward, maxRange);
    }
#endif
}
