using UnityEngine;

public class ClickRaycaster : MonoBehaviour
{
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
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.GameActive)
        {
            return;
        }

        TryClickTarget();
    }

    private void TryClickTarget()
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableLayers))
        {
            return;
        }

        if (hit.collider.TryGetComponent(out ClickTarget target))
        {
            target.DestroyTarget();
            return;
        }

        ClickTarget parentTarget = hit.collider.GetComponentInParent<ClickTarget>();

        if (parentTarget != null)
        {
            parentTarget.DestroyTarget();
        }
    }
}
