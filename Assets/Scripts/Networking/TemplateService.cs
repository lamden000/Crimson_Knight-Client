using Assets.Scripts.Networking.Dtos;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace Assets.Scripts.Networking
{
    public static class TemplateService
    {
        public static async Task<bool> LoadTemplatesAysnc()
        {
            bool flag = false;
            using (UnityWebRequest request = UnityWebRequest.Get(NetworkingUtil.GetLoadTemplateApiUrl()))
            {
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text;

                    try
                    {
                        LoadTemplateRespone res = JsonConvert.DeserializeObject<LoadTemplateRespone>(responseJson);
                        TemplateManager.MonsterTemplates = res.MonsterTemplates;
                        TemplateManager.NpcTemplates = res.NpcTemplates;
                        TemplateManager.SkillTemplates = res.SkillTemplates;
                        flag = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Parse JSON error: " + ex.Message);
                    }
                }
                else
                {
                    Debug.LogError($"Error: {request.error}");
                }
            }
            return flag;
        }
    }
}
