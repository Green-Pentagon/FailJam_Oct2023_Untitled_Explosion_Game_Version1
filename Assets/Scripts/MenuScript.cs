using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    // Start is called before the first frame update
    public void BeginGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}
