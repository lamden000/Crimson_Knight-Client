using UnityEngine;

public class SkillTester : MonoBehaviour
{
    private SkillData testSkill;
    public SkillDatabase skillDatabase;
    public GameObject skillPrefab;
    public Transform playerTest;
    public SkillName skillName;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = 0;

            // 1) Raycast tìm monster
            RaycastHit2D hit = Physics2D.Raycast(mouse, Vector2.zero);

            if (hit.collider != null)
            {
                Transform target = hit.collider.transform;

                // 2) Check tên object
                if (target.name.ToLower().Contains("enemy"))
                {
                    // 4) Spawn skill và bắn xuống monster
                    var obj = Instantiate(skillPrefab);
                    testSkill=skillDatabase.GetSkillByName(skillName);

                    // startPos sẽ bị override trong ProjectileFromSky nên bạn truyền đại vector nào cũng được
                    obj.GetComponent<Skill>().Init(
                        testSkill,
                        playerTest.position,      // startPos không dùng
                        mouse,         // endPos chính là đầu monster
                        target
                    );

                    return;
                }
            }
            else
            {
                // 5) (optional) Không click monster → bắn xuống vị trí chuột
                // Nếu bạn muốn bắn vào đất:
                var fallback = Instantiate(skillPrefab);
                fallback.GetComponent<Skill>().Init(testSkill, playerTest.position, mouse);
            }
        }
    }
}
