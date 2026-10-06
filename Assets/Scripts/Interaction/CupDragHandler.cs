using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


/// The drag and click gestures on a finished drink. Owns no rules - it asks
/// PlayController whether the serve is legal and does what it is told.
///
/// Lives on the cup's world-space canvas, the card's PARENT, so pointer
/// events on the card bubble to it. The card itself keeps the prefab's
/// CardDragHandler, which InitializeAsDrink disables - a drink is not played
/// from the hand and must not take that path.


public class CupDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler

{
    [Header("Wiring")]
    [Tooltip("The cup this canvas belongs to.")]
    [SerializeField] private CupSlot cup;


    [Tooltip("The SCREEN-SPACE root canvas - the one the hand lives under, not " +
         "this cup's world-space canvas. OnDrag divides by its scaleFactor.")]
    [SerializeField] private Canvas uiRootCanvas;

    [SerializeField] private float returnDuration = 0.35f;

    // Found once from uiRootCanvas. The card is reparented here during a drag so
    // it renders above the hand and the panels.
    private Transform dragLayer;

    // All about the card currently being carried, so none of it can be cached in
    // Awake - the card does not exist until a cup matches, and is destroyed and
    // rebuilt whenever the contents change.
    private CardVisual carried;
    private RectTransform rect;
    private CanvasGroup carriedGroup;
    private Transform home;
    private Vector3 homeScale;


    private void Awake()
    {
        Transform found = uiRootCanvas.transform.Find("DragLayer");
        dragLayer = found != null ? found : uiRootCanvas.transform;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        carried = cup.DrinkCard;
        if (carried == null)
            return;

        rect = carried.GetComponent<RectTransform>();
        carriedGroup = carried.GetComponent<CanvasGroup>();
        home = carried.transform.parent;
        homeScale = carried.transform.localScale;

        carried.transform.SetParent(dragLayer, true);
        carried.transform.SetAsLastSibling();
        carried.transform.localScale = Vector3.one;
        carriedGroup.blocksRaycasts = false;

        PlayController.Instance.BeginCupDragHighlight(cup);
    }
    public void OnDrag(PointerEventData e)
    {
        if (carried == null) return;

        // Same as CardDragHandler: the delta is screen pixels, so it needs the
        // canvas scale factor or the card lags the pointer at anything but 1x.
        rect.anchoredPosition += e.delta / uiRootCanvas.scaleFactor;

    }

    public void OnEndDrag(PointerEventData e)
    {
        if (carried == null) return;
        carriedGroup.blocksRaycasts = true;
        DropTarget target = PlayController.Instance.TargetUnderScreenPoint(e.position);
        if (PlayController.Instance.TryServe(cup, target as CustomerSlot))
        {
            carried = null;
            return;
        }
        else
            ReturnHome();


    }
    /// Click-select-click, the same alternative the hand offers. Drag is
    /// fiddly on a trackpad and hard with assistive input, and both paths end
    /// at TryServe so there are no duplicate rules.

    public void OnPointerClick(PointerEventData e)
    {
        if (cup.DrinkCard == null) return;
        PlayController.Instance.SelectCup(cup);
    }

    private void ReturnHome()
    {
        carried.transform.SetParent(home, false);
        carried.transform.localScale = homeScale;
        StartCoroutine(LerpTo(Vector2.zero));
        PlayController.Instance.ClearSelection();
        carried = null;
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
