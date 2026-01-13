using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class ResourceManager
    {
        public static void Load()
        {
            LoadItemSprites();
            LoadSkillIcons();
        }
        public static Dictionary<int, Sprite> ItemEquipmentIconSprites = new Dictionary<int, Sprite>();
        public static Dictionary<int, Sprite> ItemConsumableIconSprites = new Dictionary<int, Sprite>();
        public static Dictionary<int, Sprite> ItemMaterialsIconSprites = new Dictionary<int, Sprite>();

        public static Dictionary<int,Sprite> SkillIcons = new Dictionary<int, Sprite>();

        private static void LoadItemSprites()
        {
            LoadSprites("Items/Equipments", ItemEquipmentIconSprites);
            LoadSprites("Items/Consumables", ItemConsumableIconSprites);
            LoadSprites("Items/Materials", ItemMaterialsIconSprites);

            Debug.Log($"[ItemSpriteLoader] Equip={ItemEquipmentIconSprites.Count}, " +
                      $"Consumable={ItemConsumableIconSprites.Count}, " +
                      $"Material={ItemMaterialsIconSprites.Count}");
        }

        private static void LoadSkillIcons()
        {
            LoadSprites("Skills/Icons", SkillIcons);
        }

        private static void LoadSprites(string path, Dictionary<int, Sprite> dict)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);

            foreach (var sp in sprites)
            {
                string texName = sp.texture.name;

                if (int.TryParse(texName, out int iconId))
                {
                    if (!dict.ContainsKey(iconId))
                        dict[iconId] = sp;
                }
            }
        }


    }
}
