using UnityEngine;
using UnityEngine.UI;

public class ScrollToTop : MonoBehaviour
{
    [SerializeField] ScrollRect scrollRect;

    private void OnEnable()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRect.horizontalNormalizedPosition = 0f;
    }
}
