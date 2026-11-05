
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject HomePanel;
    public GameObject SettingPanel;
    public GameObject InfoPanel;

    [Header("Logo")]
    public RectTransform Logo;

    [Header("Buttons")]
    public RectTransform PlayB;
    public RectTransform SettingB;
    public RectTransform InfoB;

    private Coroutine buttonRoutine;

    public void Setting()
    {
        HomePanel.SetActive(false);
        SettingPanel.SetActive(true);
        InfoPanel.SetActive(false);

        Logo.transform.DOKill();

        Logo.transform.localScale = Vector3.one * 0.6f;

        Logo.transform.DOScale(Vector3.one, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Info()
    {
        HomePanel.SetActive(false);
        SettingPanel.SetActive(false);
        InfoPanel.SetActive(true);
    }

    public void Back()
    {
        SettingPanel.SetActive(false);
        InfoPanel.SetActive(false);
        HomePanel.SetActive(true);

        if (buttonRoutine != null)
            StopCoroutine(buttonRoutine);

        buttonRoutine = StartCoroutine(MenuButton());
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }



    public void VibrationT()
    {
        Debug.Log("Vibration Manager");
    }

    private IEnumerator MenuButton()
    {
        RectTransform[] buttons = { PlayB, SettingB, InfoB };
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
    }

    private void OnDisable()
    {
        if (buttonRoutine != null)
        {
            StopCoroutine(buttonRoutine);
            buttonRoutine = null;
        }
    }
}
