using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public static class MapManager
    {
        public static short MapId;
        public static string MapName;

        private static GridmapLoader _gridmapLoader;
        public static void Initialize()
        {
            Debug.Log("MapManager initialized.");
            _gridmapLoader = UnityEngine.Object.FindAnyObjectByType<GridmapLoader>();
        }

        public static void LoadMapForLoginScreen()
        {
            _gridmapLoader.LoadMapByName("Map3.json", "Default");
        }
        public static void LoadMapById(int id, Action onLoadComplete = null)
        {
            MapId = (short)id;
            _gridmapLoader.LoadMapByName($"map{id}.json", "Default", () =>
            {
                // Tự động phát nhạc cho map khi load xong
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayMapMusic(id);
                }
                onLoadComplete?.Invoke();
            });
        }

        public static void LoadMapByName(string name, Action onLoadComplete = null)
        {
            _gridmapLoader.LoadMapByName($"{name}.json", "Default", () =>
            {
                // Nếu đã có MapId được set trước đó, phát nhạc cho map đó
                if (MapId > 0 && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayMapMusic(MapId);
                }
                onLoadComplete?.Invoke();
            });
        }
    }
}
