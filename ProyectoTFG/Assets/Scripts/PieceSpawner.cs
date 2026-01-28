using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    public GameObject cylinderPrefab;
    public Transform spawnParent; // PlayerRoot o vacío para editor

    public void CreateCylinder()
    {
        GameObject piece = Instantiate(cylinderPrefab);
        piece.transform.SetParent(spawnParent);
        piece.transform.position = spawnParent.position + Vector3.right * 2; // spawn visible
    }
}
