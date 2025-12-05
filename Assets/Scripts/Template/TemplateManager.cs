using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TemplateManager
{
    public static List<MonsterTemplate> MonsterTemplates = new List<MonsterTemplate>()
    {
        new MonsterTemplate(){Id = 0, Name = "Slime",    ImageId = 1000},
        new MonsterTemplate(){Id = 1, Name = "Snail",    ImageId = 1001 },
        new MonsterTemplate(){Id = 2, Name = "Scorpion", ImageId = 1103 },
        new MonsterTemplate(){Id = 3, Name = "Bunny",    ImageId = 1173},
        new MonsterTemplate(){Id = 4, Name = "Frog",     ImageId = 1215},
    };
}
