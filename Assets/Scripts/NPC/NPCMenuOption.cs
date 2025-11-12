[System.Serializable]
public class NPCMenuOption
{
    public string label; // Tên hiển thị: "Mua hàng", "Dịch chuyển", "Nói chuyện tiếp"
    public string actionKey; // Định danh để code xử lý (vd: "open_shop", "teleport", "continue_dialogue")
}
