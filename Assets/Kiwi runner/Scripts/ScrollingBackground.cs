using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Range(0, 1)]
    public float speedMult;

    float offset;
    bool scrolling;

    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        if(GameController.gameRunning && !scrolling)
        {
            StartScrolling();
        }
    }

    public void StartScrolling()
    {
        StartCoroutine(Scroll());
    }

    IEnumerator Scroll()
    {
        float t = 0;

        while (GameController.gameRunning)
        {
            t += Time.deltaTime / GameController.speed;
            offset = t * speedMult;
            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offset, 0);

            yield return null;
        }
    }
}
