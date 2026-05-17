using System;
using UnityEngine;

public class OutlineGradient : MonoBehaviour
{
    [SerializeField] private SpriteRenderer outline;

    private GameObject player;
    private bool isPlayerNear;
    private float maxDistance;

    private void Start()
    {
        outline.color = new Color(outline.color.r, outline.color.g, outline.color.b, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            isPlayerNear = true;
            maxDistance = Vector3.Distance(transform.position, player.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            isPlayerNear = false;

            outline.color = new Color(outline.color.r, outline.color.g, outline.color.b, 0);
        }
    }

    private void Update()
    {
        if (!isPlayerNear) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        float clampedDistance = 1f - Mathf.Clamp01(distance / maxDistance);

        float opacity = clampedDistance * (153f / 255f);
        outline.color = new Color(outline.color.r, outline.color.g, outline.color.b, opacity);

        Debug.Log(opacity);
    }
}