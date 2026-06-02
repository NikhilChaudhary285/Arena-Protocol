using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject hostButton;
    public GameObject clientButton;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        HideButtons();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        HideButtons();
    }

    private void HideButtons()
    {
        if (hostButton) hostButton.SetActive(false);
        if (clientButton) clientButton.SetActive(false);
    }
}