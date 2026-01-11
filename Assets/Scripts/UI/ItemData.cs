public class ItemData
{
    public int itemId;
    public int spriteId;     
    public string name;
    public string description;

    public ItemData(int itemId, int spriteId, string name, string description)
    {
        this.itemId = itemId;
        this.spriteId = spriteId;
        this.name = name;
        this.description = description;
    }
}
