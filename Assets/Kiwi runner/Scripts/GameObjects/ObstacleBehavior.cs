﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehavior : MonoBehaviour
{
    Transform trampoline;
    Transform cue;
    Transform obstacle;
    public GameObject air3;
    public GameObject air4;

    void Start()
    {
        trampoline = GetComponentInChildren<Transform>().Find("Trampoline");
        cue = GetComponentInChildren<Transform>().Find("Cue");
        obstacle = GetComponentInChildren<Transform>().Find("SlowZone");

        if (name != "Babies")
        {
            trampoline.GetComponent<SpriteRenderer>().sprite = GameController.color;
            air4.SetActive(false);
            Resize();
        }

        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        float t = 0;
        Vector2 startPos = transform.localPosition;
        Vector2 endPos = new Vector2(-transform.localPosition.x, transform.localPosition.y);

        // Obstacles move right -> left with the scrolling foreground based on the game's speed
        while (transform.localPosition.x > endPos.x && GameController.gameRunning)
        {
            t += Time.deltaTime / (GameController.speed * ((startPos.x - endPos.x) / GameController.groundSize));
            transform.position = Vector3.Lerp(new Vector2(startPos.x, transform.position.y), 
                new Vector2(endPos.x, transform.position.y), t);

            // When obstacles are off-screen, they are destroyed
            if (transform.position.x < endPos.x + 5)
            {
                Destroy(gameObject);
            }
            yield return null;
        }

    }

    void Resize()
    {
        // Resizing the trampoline object according to the input window
        float trampolineColliderSize = trampoline.GetComponent<BoxCollider2D>().size.x;
        float newTrampolineWidth = ((GameController.groundSize / GameController.baseSpeed)
            / trampolineColliderSize) * GameController.inputWindow;

        trampoline.localScale = new Vector3(newTrampolineWidth, trampoline.localScale.y);

        // If the trampoline is wide enough, add another aircurrent
        if(newTrampolineWidth > 1.4f)
        {
            air4.SetActive(true);
        } // If too narrow, remove one current
        else if(newTrampolineWidth < 0.8f)
        {
            air3.SetActive(false);
        }

        // Moving the trampoline object after resizing
        float obstacleWidth = obstacle.GetComponent<Renderer>().bounds.size.x;
        float trampolineWidth = trampoline.GetComponent<Renderer>().bounds.size.x;
        float newTaskPos = obstacle.localPosition.x - obstacleWidth / 2 - trampolineWidth / 2;

        trampoline.localPosition = new Vector2(newTaskPos, trampoline.transform.localPosition.y);


        // Moving the cue/signpost after resizing task, according to prep phase length
        float actualColliderSize = trampolineColliderSize / 10.22f * trampolineWidth;
        float newCuePos = newTaskPos - actualColliderSize / 2 - 
            (GameController.groundSize / GameController.baseSpeed) * GameController.prepPhase;

        cue.localPosition = new Vector2(newCuePos, cue.transform.localPosition.y);

        // Move entire obstacle object so it (seemingly) spawns just off screen
        transform.position = new Vector2(10 + Mathf.Abs(cue.localPosition.x), -1);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "GameController" || other.tag == "Player")
        {
            Physics2D.IgnoreCollision(other, GetComponent<Collider2D>()); // No collision for kiwi
        }
    }
}
