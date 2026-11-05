using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModelSelectionMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject selectionMenu;

    [Header("Buttons")]
    public GameObject expandButton;
    public GameObject collapseButton;
    public RectTransform exitbutton, building1,buildign2,building3,building4,building5;

    [Header("Point")]
    [Tooltip("The menu grows out of this point when expanding and shrinks back into it when collapsing.")]
    public RectTransform startPoint;

    [Header("Animation")]
    public float duration = 0.3f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    [Header("Scene Name")]
    public string sceneName;

    private Vector3 menuScale;
    private Vector3 expandScale;
    private Vector3 menuRestPosition;

    private Coroutine buttonRoutine;

    void Awake()
    {
        menuScale = selectionMenu.transform.localScale;
        expandScale = expandButton.transform.localScale;
        menuRestPosition = selectionMenu.transform.position;
    }

    Vector3 StartPos()
    {
        return startPoint != null ? startPoint.position : menuRestPosition;
    }

    public void Collapsed()
    {
        selectionMenu.transform.DOKill();
        expandButton.transform.DOKill();

        // Menu moves into the point while shrinking, then turns off
        Sequence close = DOTween.Sequence().SetUpdate(true);
        close.Join(selectionMenu.transform.DOMove(StartPos(), duration).SetEase(closeEase));
        close.Join(selectionMenu.transform.DOScale(Vector3.zero, duration).SetEase(closeEase));
        close.OnComplete(() => selectionMenu.SetActive(false));

        // Expand button pops in
        expandButton.SetActive(true);
        expandButton.transform.localScale = Vector3.zero;
        expandButton.transform
            .DOScale(expandScale, duration)
            .SetEase(openEase)
            .SetUpdate(true);
    }

    public void Selections()
    {
        selectionMenu.transform.DOKill();
        expandButton.transform.DOKill();

        // Expand button shrinks, then turns off
        expandButton.transform
            .DOScale(Vector3.zero, duration * 0.5f)
            .SetEase(closeEase)
            .SetUpdate(true)
            .OnComplete(() => expandButton.SetActive(false));

        // Menu grows out of the point back to its resting position
        selectionMenu.SetActive(true);
        selectionMenu.transform.position = StartPos();
        selectionMenu.transform.localScale = Vector3.zero;

        Sequence open = DOTween.Sequence().SetUpdate(true);
        open.Join(selectionMenu.transform.DOMove(menuRestPosition, duration).SetEase(openEase));
        open.Join(selectionMenu.transform.DOScale(menuScale, duration).SetEase(openEase));
    }

    public void Exit()
    {
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator MenuButton()
    {
        RectTransform[] buttons = { exitbutton, building1, buildign2, building3, building4, building5 };
        Vector3[] originalScales = new Vector3[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].DOKill();
            originalScales[i] = buttons[i].localScale;
            buttons[i].localScale = Vector3.zero;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].DOScale(originalScales[i], 0.65f)
                .SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.15f);
        }

        buttonRoutine = null;

         void OnDisable()
        {
            if (buttonRoutine != null)
            {
                StopCoroutine(buttonRoutine);
                buttonRoutine = null;
            }
        }


    }

    /*
     * backup og script
    public void Collapsed()
    {
        selectionMenu.SetActive(false);
        expandButton.SetActive(true);
    }

    public void Selections()
    {
        expandButton.SetActive(false);
        selectionMenu.SetActive(true);
    }
}
     */
}