using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// The drag gesture on a card. Owns no rules - it asks PlayController whether
/// a drop is legal and does what it is told.
///
/// Lives on the card root (the alpha-0 hit area), not on CardContent, because
/// the root is what the hand layout positions and what must reparent to
/// DragLayer during a drag.
[RequireComponent(typeof(CanvasGroup))]     // auto adds a canvas group and requires one to exist
public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private float returnDuration = 0.35f;          // how long it takes for the card to return to the hand

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private CardVisual visual;
    private Canvas rootCanvas;
    private Transform dragLayer;

    // Where the card came from. Storing the INDEX rather than the position,
    // because the hand rearranges the moment this card is reparented out -
    // a cached position would be stale before the return lerp starts.
    private Transform homeParent;
    private int homeSiblingIndex;

    public bool IsDragging { get; private set; }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        visual = GetComponent<CardVisual>();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        Transform found = rootCanvas.transform.Find("DragLayer");
        dragLayer = found != null ? found : rootCanvas.transform;
    }
        // a required method for IBeginDragHandler, e is the event data which is standard format even though it's not used
    public void OnBeginDrag(PointerEventData e)
    {
        IsDragging = true;

        homeParent = transform.parent;              // the transform of the parent that this is nested under
        homeSiblingIndex = transform.GetSiblingIndex();     // the position among the parent's children, 0 for first, 1 for second, ect.

        // Reparent so the card renders above the hand and the panels.
        // worldPositionStays: true keeps it under the cursor during the swap.
        transform.SetParent(dragLayer, true);       // assigns dragLayer as the new parent, true keeps the card visually where it was before being dragged
        transform.SetAsLastSibling();               // moves to the end of the list, so it renders on top in dragLayer

        // The card must not block its own drop test or the targets beneath it.
        canvasGroup.blocksRaycasts = false;

        PlayController.Instance.BeginDragHighlight(visual);     //starts the drag highlight in PlayController
    }

    // required by IDragHandler
    public void OnDrag(PointerEventData e)
    {
        // Screen Space - Camera canvases need the delta divided by the canvas
        // scale factor, or the card lags the pointer at anything but 1x.
        rect.anchoredPosition += e.delta / rootCanvas.scaleFactor;      // e.delta is how far the pointer moved since the last frame
                                                                        // dividing by the scaleFactor means a 10px move is a 5-unit position change
    }

    public void OnEndDrag(PointerEventData e)
    {
        IsDragging = false;
        canvasGroup.blocksRaycasts = true;

        // e.position is screen pixels - the space SpaceProbe validated.
        DropTarget target = PlayController.Instance.TargetUnderScreenPoint(e.position);

        if (PlayController.Instance.TryPlay(visual, target))
            return;   // played; PlayController had HandManager remove it

        ReturnHome();
    }

    /// Click-to-select. OnPointerClick does NOT fire after a drag, so these
    /// two never collide.
    public void OnPointerClick(PointerEventData e)
    {
        PlayController.Instance.SelectCard(visual);
    }

    private void ReturnHome()
    {
        Vector2 from = rect.anchoredPosition;

        transform.SetParent(homeParent, false);
        transform.SetSiblingIndex(homeSiblingIndex);

        // Reparenting fires OnTransformChildrenChanged, so FanLayout (or the
        // layout group) has already computed the correct slot by now. Lerp
        // from where the card was to where the layout put it.
        Vector2 to = rect.anchoredPosition;
        rect.anchoredPosition = from;

        StartCoroutine(LerpTo(to));
        PlayController.Instance.ClearSelection();   // clears the selected card in PlayController
    }

    private IEnumerator LerpTo(Vector2 target)
    {
        Vector2 from = rect.anchoredPosition;
        float t = 0f;

        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / returnDuration);
            rect.anchoredPosition = Vector2.Lerp(from, target, p);
            yield return null;
        }

        rect.anchoredPosition = target;
    }
}
