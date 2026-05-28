using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
        hostButton.SetActive(false);
        clientButton.SetActive(false);
    }
}