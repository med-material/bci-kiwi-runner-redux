using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EagleBehavior : MonoBehaviour
{
    public SpriteRenderer exclamation;
    public Transform babies;

    readonly float multiplier = 5;
    readonly float offSet = 2;

    Vector2 offScreenPos;
    
    // Start is called before the first frame update
    void Start()
    {
        offScreenPos = new Vector2(14, transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FlyOffScreen()
    {
        PlaySound();
        StartCoroutine(Fly(transform.position, offScreenPos, false, 3));
    }

    public void Attack()
    {
        PlaySound();
        StartCoroutine(Fly(transform.position, 
            new Vector2(babies.transform.position.x - offSet,
            babies.transform.position.y + offSet), true, 1.25f));
    }

    IEnumerator Fly(Vector2 startPos, Vector2 endPos, bool attack, float timeToMove)
    {
        float t = 0;

        while(t < timeToMove)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t / timeToMove);

            yield return null;
        }

        if (!attack)
            Destroy(gameObject);
        else
        {
            GetComponent<AudioSource>().volume = 0.1f;
            exclamation.enabled = true;
            Invoke("FlyOffScreen", 1f);
        }
    }

    void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }
}
