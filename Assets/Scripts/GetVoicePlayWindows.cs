using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using HTC.UnityPlugin.Vive;

public class GetVoicePlayWindows : MonoBehaviour
{
    public Camera virtualHMD;

    public GameObject browserWindowPrefab;

    private GameObject browserWindow;
    private Browser browserComponent;
    private Animator browserVisualingAnimator;
    
    public void Start()
    {
        browserWindow = null;
        browserComponent = null;
        browserVisualingAnimator = null;
    }

    public void InstantiateBrowserWindow(string URL = "http://www.google.com/")
    {
        if(browserWindow == null)
        {
            browserWindow = Instantiate(browserWindowPrefab, virtualHMD.transform.position, GetBrowserQuaternion());
            browserComponent = browserWindow.GetComponentInChildren<Browser>();
            browserComponent.Url = URL;

            browserVisualingAnimator = browserWindow.GetComponent<Animator>();
        }
        else
        {
            browserWindow.transform.position = virtualHMD.transform.position;
            browserComponent.LoadURL(URL, force: true);
        }
    }

    public void InitializeBrowserWindow()
    {
        if(browserWindow != null)
        {
            StartCoroutine("WaitAndInitialize");
        }
    }

    IEnumerator WaitAndInitialize()
    {
        if(browserVisualingAnimator != null)
        {
            browserVisualingAnimator.SetTrigger("Destroy");
            yield return new WaitForSeconds(1f);
            Destroy(browserWindow);
            browserWindow = null;
            browserVisualingAnimator = null;
        }
    }

    private Quaternion GetBrowserQuaternion()
    {
        float yRotation = virtualHMD.transform.eulerAngles.y;
        Vector3 browserRotation = new Vector3(0, yRotation, 0);

        return Quaternion.Euler(browserRotation);
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
