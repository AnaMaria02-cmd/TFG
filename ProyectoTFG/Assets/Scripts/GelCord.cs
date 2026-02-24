using UnityEngine;

/// <summary>
/// Dibuja un cordón de "gelatina/plastilina" desde el jugador hasta el cursor.
/// Requiere un LineRenderer en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class GelCord : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Referencias")]
    [Tooltip("Punto del cuerpo del jugador desde el que sale el cordón.")]
    public Transform playerOrigin;

    [Header("Rango y Geometría")]
    [Tooltip("Radio máximo en unidades de mundo. La punta se queda clavada al sobrepasar este límite.")]
    public float maxRange = 5f;
    [Tooltip("Número de puntos del LineRenderer (≥20 para curva fluida).")]
    [Range(8, 64)]
    public int resolution = 24;

    [Header("Anchura / Estiramiento")]
    [Tooltip("Anchura en la base (origen).")]
    public float baseWidth = 0.15f;
    [Tooltip("Anchura mínima en la punta cuando el cordón está al máximo de estiramiento.")]
    public float minTipWidth = 0.02f;

    [Header("Comportamiento Orgánico")]
    [Tooltip("Cuánto cae el centro del cordón por 'gravedad' al máximo estiramiento.")]
    public float gravitySag = 0.4f;
    [Tooltip("Amplitud de la oscilación lateral senoidal.")]
    public float wobbleAmplitude = 0.10f;
    [Tooltip("Velocidad de la oscilación (radianes/s).")]
    public float wobbleFrequency = 3.5f;

    [Header("Visibilidad")]
    [Tooltip("Oculta el cordón cuando el cursor está muy cerca del origen.")]
    public float hideRadius = 0.1f;

    // ── Privados ──────────────────────────────────────────────────────────────
    private LineRenderer lr;
    private Camera        cam;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = resolution;
        lr.useWorldSpace  = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        cam = Camera.main;
        if (playerOrigin == null) playerOrigin = transform;
    }

    private void Update()
    {
        // ── 1. Posición del cursor en el plano del jugador ────────────────────
        // Plano siempre perpendicular a la cámara → funciona con cualquier rotación
        Plane dragPlane = new Plane(-cam.transform.forward, playerOrigin.position);
        Ray   ray        = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 cursorWorld = playerOrigin.position; // fallback

        if (dragPlane.Raycast(ray, out float hitDist))
            cursorWorld = ray.GetPoint(hitDist);

        // ── 2. Clamp al radio máximo ──────────────────────────────────────────
        Vector3 delta    = cursorWorld - playerOrigin.position;
        float rawDist    = delta.magnitude;

        // Ocultar si el cursor está pegado al origen
        lr.enabled = rawDist > hideRadius;
        if (!lr.enabled) return;

        float clampedDist  = Mathf.Min(rawDist, maxRange);
        float stretchRatio = (maxRange > 0f) ? clampedDist / maxRange : 0f; // 0–1

        Vector3 origin = playerOrigin.position;
        Vector3 tipPos = origin + delta.normalized * clampedDist;

        // ── 3. Anchura dinámica (se adelgaza al estirar) ──────────────────────
        float tipWidth = Mathf.Lerp(baseWidth, minTipWidth, stretchRatio);
        lr.startWidth = baseWidth;
        lr.endWidth   = tipWidth;

        // ── 4. Eje perpendicular relativo a la cámara (funciona en cualquier orientación) ─
        Vector3 cordDir = (tipPos - origin).normalized;
        // Cross del cordón con el 'arriba' de la cámara → perpendicular visible desde la cámara
        Vector3 perp = Vector3.Cross(cordDir, cam.transform.up).normalized;
        if (perp.sqrMagnitude < 0.001f)
            perp = cam.transform.right; // fallback si el cordón es paralelo al up de la cámara

        // ── 5. Construir los puntos de la curva ───────────────────────────────
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1); // 0 → 1

            // Posición base: interpolación lineal origen → punta
            Vector3 pos = Vector3.Lerp(origin, tipPos, t);

            // Caída gravitatoria: parábola máxima en el centro, escala con stretchRatio
            float sag = -gravitySag * stretchRatio * Mathf.Sin(t * Mathf.PI);
            pos.y += sag;

            // Oscilación lateral senoidal: más débil cerca de la punta (inmóvil)
            float envelope = Mathf.Sin(t * Mathf.PI); // 0 en bordes, 1 en centro
            float wobble   = wobbleAmplitude * envelope
                           * Mathf.Sin(t * Mathf.PI * 2f + Time.time * wobbleFrequency);
            pos += perp * wobble;

            lr.SetPosition(i, pos);
        }
    }

#if UNITY_EDITOR
    // Dibuja el radio máximo en el Editor para facilitar el ajuste
    private void OnDrawGizmosSelected()
    {
        if (playerOrigin == null) return;
        UnityEditor.Handles.color = new Color(0.2f, 1f, 0.5f, 0.3f);
        UnityEditor.Handles.DrawWireDisc(playerOrigin.position, Vector3.forward, maxRange);
    }
#endif
}
