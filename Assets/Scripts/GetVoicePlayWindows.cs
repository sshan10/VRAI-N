using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using HTC.UnityPlugin.Vive;

public class GetVoicePlayWindows : MonoBehaviour
{
    public Camera virtualHMD;

    public GameObject browserWindowPrefab;

    public string URL;

    private GameObject browserWindow;
    private Browser browserComponent;
    private Animator browserVisualingAnimator;

    Vector3 browserRotation;

    public void Start()
    {
        float yRotation = virtualHMD.transform.eulerAngles.y;
        browserRotation = new Vector3(0, yRotation, 0);
        browserWindow = null;
        browserComponent = null;
        browserVisualingAnimator = null;
    }

    public void InstantiateBrowserWindow()
    {
        if(browserWindow != null)
        {
            browserWindow = Instantiate(browserWindowPrefab, virtualHMD.transform.position, Quaternion.Euler(browserRotation));
            browserComponent = browserWindow.GetComponentInChildren<Browser>();
            browserComponent.Url = URL;
        }
    }

    public void InitializeBrowserWindow()
    {
        if(browserVisualingAnimator != null)
        {
            StartCoroutine("WaitAndInitialize");
        }
    }

    IEnumerator WaitAndInitialize()
    {
        browserVisualingAnimator.SetTrigger("Destroy");
        yield return new WaitForSeconds(1f);
        Destroy(browserWindow);
        browserWindow = null;
        URL = null;
    }

    void Update()
    {
        if (ViveInput.GetPressDownEx(HandRole.LeftHand, ControllerButton.Trigger))
        {
            InstantiateBrowserWindow();
        }
        if (ViveInput.GetPressDownEx(HandRole.LeftHand, ControllerButton.Grip))
        {
            InitializeBrowserWindow();
        }
    }
}
