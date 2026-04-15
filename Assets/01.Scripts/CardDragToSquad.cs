using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CardView), typeof(Collider))]
public class CardDragToSquad : MonoBehaviour
{
    [SerializeField] private SquadController squadController;
    [SerializeField] private Camera dragCamera;
    [SerializeField] private float dragPlaneY = 0f;
    [SerializeField] private LayerMask raycastMask = ~0;

    private CardView cardView;
    private Collider cardCollider;
    private Vector3 startPosition;
    private bool isDragging;
    private Plane dragPlane;

    private void Awake()
    {
        cardView = GetComponent<CardView>();
        cardCollider = GetComponent<Collider>();
        if (dragCamera == null) dragCamera = Camera.main;
        dragPlane = new Plane(Vector3.up, new Vector3(0f, dragPlaneY, 0f));
    }

    private void Update()
    {
        if (Mouse.current == null || dragCamera == null) return;

        Vector2 pointer = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryBeginDrag(pointer);

        if (isDragging && Mouse.current.leftButton.isPressed)
            Drag(pointer);

        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            EndDrag(pointer);
    }

    private void TryBeginDrag(Vector2 pointer)
    {
        if (!cardView.HasCard) return;

        Ray ray = dragCamera.ScreenPointToRay(pointer);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, raycastMask))
        {
            if (hit.collider == cardCollider || hit.collider.transform.IsChildOf(transform))
            {
                startPosition = transform.position;
                isDragging = true;
            }
        }
    }

    private void Drag(Vector2 pointer)
    {
        Ray ray = dragCamera.ScreenPointToRay(pointer);
        if (dragPlane.Raycast(ray, out float enter))
            transform.position = ray.GetPoint(enter);
    }

    private void EndDrag(Vector2 pointer)
    {
        isDragging = false;
        transform.position = startPosition;

        if (squadController == null) return;

        Ray ray = dragCamera.ScreenPointToRay(pointer);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, raycastMask)) return;

        SquadDropZone zone = hit.collider.GetComponent<SquadDropZone>();
        if (zone == null) return;

        squadController.TryPlaceFromHand(cardView, zone.SquadIndex);
    }
}
