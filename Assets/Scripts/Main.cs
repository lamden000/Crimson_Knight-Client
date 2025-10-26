using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Main : MonoBehaviour
{
    private readonly int REFERENCE_HEIGHT = 540;
    private readonly int PPU = 1;

    public CameraFollow cameraFollow;

    void Awake()
    {

        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ConfigurePixelPerfectCameraForMobile();
        }
        else if (Application.isEditor ||
                 Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.OSXPlayer ||
                 Application.platform == RuntimePlatform.LinuxPlayer)
        {
            int w = Mathf.RoundToInt(REFERENCE_HEIGHT * (16f / 9f)); // 960
            int h = REFERENCE_HEIGHT; // 540

            Screen.SetResolution(w, h, false);
            ConfigurePixelPerfectCameraForPC(w, h);
        }


        if (cameraFollow != null)
        {
            float calculatedOrthoSize = (float)REFERENCE_HEIGHT / (PPU * 2f);
            cameraFollow.InitializeBounds(calculatedOrthoSize);
        }
        else
        {
            Debug.LogError("Chưa gán CameraFollow cho Main.cs!");
        }
    }

    private void ConfigurePixelPerfectCameraForMobile()
    {
        PixelPerfectCamera ppc = GetOrAddPixelPerfectCamera();
        if (ppc != null)
        {
            ppc.assetsPPU = PPU;
            ppc.refResolutionX = 0;
            ppc.refResolutionY = REFERENCE_HEIGHT;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.None;
        }
    }
    private void ConfigurePixelPerfectCameraForPC(int targetWidth, int targetHeight)
    {
        PixelPerfectCamera ppc = GetOrAddPixelPerfectCamera();
        if (ppc != null)
        {
            ppc.assetsPPU = PPU;
            ppc.refResolutionX = targetWidth;
            ppc.refResolutionY = targetHeight;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.None;
        }
    }
    private PixelPerfectCamera GetOrAddPixelPerfectCamera()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Camera.main not found!");
            return null;
        }
        PixelPerfectCamera ppc = Camera.main.GetComponent<PixelPerfectCamera>();
        if (ppc == null)
        {
            ppc = Camera.main.gameObject.AddComponent<PixelPerfectCamera>();
        }
        return ppc;
    }
}