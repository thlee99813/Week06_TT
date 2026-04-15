using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CardView), typeof(Collider))]
public class CardDragToSquad : MonoBehaviour
{
    private enum DragSourceType
    {
        Hand,
        Temp
    }

    [Header("Source")]
    [SerializeField] private DragSourceType sourceType = DragSourceType.Hand;

    [Header("Controllers")]
    [SerializeField] private DeckController deckController;
    [SerializeField] private SquadController squadController;
    [SerializeField] private TempStorageController tempStorageController;

    [Header("Drag")]
    [SerializeField] private Camera dragCamera;
    [SerializeField] private float dragPlaneY = 0f;
    [SerializeField] private LayerMask raycastMask = ~0;

    private CardView cardView;
    private Collider cardCollider;
    private Vector3 startPosition;
    private bool isDragging;
    private Plane dragPlane;

    [SerializeField] private LayerMask dragStartMask;
    [SerializeField] private LayerMask dropCheckMask;


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
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, dragStartMask))
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

        bool droppedOnValidZone = false;
        bool moved = false;

        Ray ray = dragCamera.ScreenPointToRay(pointer);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, dropCheckMask))
        {
            SquadDropZone squadZone = hit.collider.GetComponentInParent<SquadDropZone>();
            if (squadZone != null)
            {
                droppedOnValidZone = true;
                moved = TryMoveToSquad(squadZone.SquadIndex);
            }
            else
            {
                TempStorageDropZone tempZone = hit.collider.GetComponentInParent<TempStorageDropZone>();
                if (tempZone != null)
                {
                    droppedOnValidZone = true;

                    if (sourceType == DragSourceType.Hand)
                    {
                        moved = TryMoveToTempStorage();
                    }
                    else
                    {
                        moved = true;
                    }
                }

            }
        }

        if (!droppedOnValidZone)
        {
            if (sourceType == DragSourceType.Temp)
            {
                TryDiscardSourceCard();
            }
            return;
        }

        if (!moved && sourceType == DragSourceType.Temp)
        {
            TryDiscardSourceCard();
        }
    }


    private bool TryMoveToSquad(int squadIndex)
    {
        if (squadController == null) return false;
        if (!cardView.HasCard) return false;
        if (!squadController.CanPlaceInSquad(squadIndex)) return false;

        if (!cardView.TryTakeCard(out Card card) || card == null) return false;

        if (!squadController.TryPlaceCard(squadIndex, card))
        {
            cardView.SetCard(card);
            return false;
        }

        FinalizeSourceAfterMove();
        return true;
    }

    private bool TryMoveToTempStorage()
    {
        if (sourceType != DragSourceType.Hand) return false;
        if (tempStorageController == null) return false;
        if (!tempStorageController.HasEmptySlot()) return false;

        if (!cardView.TryTakeCard(out Card card) || card == null) return false;

        if (!tempStorageController.TryStoreCard(card))
        {
            cardView.SetCard(card);
            return false;
        }

        FinalizeSourceAfterMove();
        return true;
    }

    private bool TryDiscardSourceCard()
    {
        if (deckController == null) return false;
        if (!cardView.TryTakeCard(out Card card) || card == null) return false;

        deckController.AddToDiscard(card);
        FinalizeSourceAfterMove();
        return true;
    }

    private void FinalizeSourceAfterMove()
    {
        if (sourceType == DragSourceType.Hand)
        {
            gameObject.SetActive(false);
        }
        else
        {
            cardView.ClearCard();
        }
    }
}
