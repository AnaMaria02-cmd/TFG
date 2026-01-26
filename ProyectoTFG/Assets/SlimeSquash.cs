using UnityEngine;

public class SlimeSquash : MonoBehaviour
{
    public float squashAmount = 0.3f;
    public float speed = 5f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float velocity = GetComponent<Rigidbody>().linearVelocity.magnitude;

        float squash = Mathf.Clamp(velocity * squashAmount, 0, squashAmount);

        transform.localScale = new Vector3(
            originalScale.x + squash,
            originalScale.y - squash,
            originalScale.z + squash
        );
    }
}
