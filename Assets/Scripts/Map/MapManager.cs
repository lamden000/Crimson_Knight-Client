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
            _gridmapLoader.LoadMapByName("map3.json","Default");
        }
        public static void LoadMapById(int id)
        {
            _gridmapLoader.LoadMapByName($"map{id}.json", "Default");
        }
    }
}
