
using TMPro;
using UnityEngine;

public class FloatingAmount : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;
    [SerializeField] private float riseDistance = 0.5f;
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private Color gainColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color lossColor = new Color(1f, 0.5f, 0.5f);

    private float elapsedTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Show(int amount)
    {
        label.text = amount.ToString("+0;-0");
        label.color = amount >= 0 ? gainColor : lossColor;
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position += Vector3.up * (riseDistance / lifetime) * Time.deltaTime;
        elapsedTime += Time.deltaTime;
        float percent = elapsedTime / lifetime;
        label.alpha = 1f - percent;

    }

    private void Awake()
    {
        MeshRenderer mr = label.GetComponent<MeshRenderer>();
        mr.sortingLayerName = "Shop";     // match your customers
        mr.sortingOrder = 100;
    }
}
