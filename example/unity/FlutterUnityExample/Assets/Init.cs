using UnityEngine;

public class Init : MonoBehaviour
{
    GameObject body;
    // Start is called before the first frame update
    void Start()
    {
        body = GameObject.Find("other_objetcs_body");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCarColor(string colorString)
    {
        string[] part = colorString.Split(',');
        Color color = new Color(float.Parse(part[0]), float.Parse(part[1]), float.Parse(part[2]));
        body.GetComponent<Renderer>().material.color = color;
    }
}
