/* This class for manage the UI element in Main menu*/
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    
    [SerializeField] GameObject CreditUI;
    [SerializeField] GameObject settingUI;

    public void ShowSetting()
    {
        settingUI.SetActive(true);
    }

    public void HideSetting()
    {
        settingUI.SetActive(false);
    }

    public void ShowCredit()
    {
        CreditUI.SetActive(true);
    }

    public void HideCredit()
    {
        CreditUI.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
