using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Range(0, 1)]
    public float speedMult;

    float offset;
    bool scrolling;

    void Update()
    {
        if(GameController.gameRunning && !scrolling)
        {
            StartScrolling();
        }
    }

    void StartScrolling()
    {
        StartCoroutine(Scroll());
        scrolling = true;
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
