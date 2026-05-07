using UnityEngine;

public class ClickRaycaster : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask clickableLayers = ~0;
    [SerializeField] private float rayDistance = 100f;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!PointerInput.TryGetPointerDown(out Vector2 pointerPosition))
        {
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.GameActive)
        {
            return;
        }

        TryClickTarget(pointerPosition);
    }

    private void TryClickTarget(Vector2 pointerPosition)
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(pointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableLayers, QueryTriggerInteraction.Collide))
        {
            return;
        }

        ClickTarget target = hit.collider.GetComponentInParent<ClickTarget>();

        if (target != null)
        {
            target.DestroyTarget();
        }
    }
}
