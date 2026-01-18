using Assets.Scripts.Networking.Dtos;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Networking
{
    public static class RegisterService
    {
        public static async Task Register(RegisterRequest data)
        {
            string jsonBody = JsonUtility.ToJson(data);

            using (UnityWebRequest request = new UnityWebRequest(NetworkingUtil.GetRegisterApiUrl(), "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                string responseJson = request.downloadHandler.text;
                try
                {
                    RegisterRespone res = JsonUtility.FromJson<RegisterRespone>(responseJson);
                    UIManager.Instance.ShowOK(res.Message);
                }
                catch (System.Exception ex)
                {
                    UIManager.Instance.ShowOK("Có lỗi xảy ra");
                }

            }
        }
    }
}
