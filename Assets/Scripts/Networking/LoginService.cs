using Assets.Scripts.Networking.Dtos;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Networking
{
    public static class LoginService
    {
        public static async Task SendLoginRequest(string username, string password)
        {
            UIManager.Instance.DisableLoginScreen();
            UIManager.Instance.EnableLoadScreen();


            LoginRequest req = new LoginRequest { Username = username, Password = password };
            req.Version = "e8eb7ee5-3abc-4da5-8869-15cd10cf3b23";
            string jsonBody = JsonUtility.ToJson(req);

            using (UnityWebRequest request = new UnityWebRequest(NetworkingUtil.GetLoginApiUrl(), "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                string responseJson = request.downloadHandler.text;
                try
                {
                    LoginResponse res = JsonUtility.FromJson<LoginResponse>(responseJson);
                    if (res.HttpStatusCode == 200)
                    {
                        Session.Connect(res.Message);
                    }
                    else
                    {
                        UIManager.Instance.DisableLoadScreen();
                        UIManager.Instance.EnableLoginScreen();
                        UIManager.Instance.ShowOK(res.Message);
                    }
                }
                catch (System.Exception ex)
                {
                    UIManager.Instance.DisableLoadScreen();
                    UIManager.Instance.EnableLoginScreen();
                    UIManager.Instance.ShowOK(ex.Message);
                }

            }
        }
    }
}
