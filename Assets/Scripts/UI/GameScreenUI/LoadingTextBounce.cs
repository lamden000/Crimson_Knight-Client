using TMPro;
using UnityEngine;

public class LoadingTextWave : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float amplitude = 10f;   
    [SerializeField] private float frequency = 4f;    
    [SerializeField] private float charOffset = 0.2f; 

    private TMP_TextInfo textInfo;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        text.ForceMeshUpdate();
        textInfo = text.textInfo;
    }

    private void Update()
    {
        text.ForceMeshUpdate();
        textInfo = text.textInfo;

        float time = Time.time * frequency;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            float offsetY = Mathf.Sin(time - i * charOffset) * amplitude;
            Vector3 offset = new Vector3(0, offsetY, 0);

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
