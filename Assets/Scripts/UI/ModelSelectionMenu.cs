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
