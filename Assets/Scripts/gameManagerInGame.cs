
using UnityEngine;
using UnityEngine.SceneManagement;

public class gameManagerInGame : MonoBehaviour
{
    public void returnToMainMenu()
    {
        SceneManager.LoadScene("menu");
    }
}
