using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Exit()
    {
        AudioService.Instance.PlayEffect("Button");
        Invoke(nameof(QuitGame), 0.1f);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
