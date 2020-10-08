using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehavior : MonoBehaviour
{
    Transform trampoline;
    Transform cue;
    Transform obstacle;

    // Start is called before the first frame update
    void Start()
    {
        trampoline = GetComponentInChildren<Transform>().Find("Trampoline");
        cue = GetComponentInChildren<Transform>().Find("Cue");
        obstacle = GetComponentInChildren<Transform>().Find("SlowZone");

        if(name != "Babies")
            Resize();

        StartCoroutine(Move());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Move()
    {
        float t = 0;
        Vector2 startPos = transform.localPosition;
        Vector2 endPos = new Vector2(-transform.localPosition.x, transform.localPosition.y);

        while (transform.localPosition.x > endPos.x && GameController.gameRunning)
        {
            t += Time.deltaTime / (GameController.speed * ((startPos.x - endPos.x) / GameController.groundSize));
            transform.position = Vector3.Lerp(new Vector2(startPos.x, transform.position.y), 
                new Vector2(endPos.x, transform.position.y), t);

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
        float newTrampolineWidth = ((GameController.groundSize / GameController.speed)
            / trampolineColliderSize) * GameController.inputWindow;

        trampoline.transform.localScale = new Vector3(newTrampolineWidth, trampoline.transform.localScale.y);

        // Moving the trampoline object after resizing
        float obstacleWidth = obstacle.GetComponent<Renderer>().bounds.size.x;
        float trampolineWidth = trampoline.GetComponent<Renderer>().bounds.size.x;
        float newTaskPos = obstacle.transform.localPosition.x - obstacleWidth / 2 - trampolineWidth / 2;

        trampoline.transform.localPosition = new Vector2(newTaskPos, trampoline.transform.localPosition.y);

        //Moving the cue/signpost after resizing task, according to prep phase length
        float newCuePos = trampoline.transform.localPosition.x - trampolineColliderSize / 2 - 
            (GameController.groundSize / GameController.speed) * GameController.prepPhase;

        cue.transform.localPosition = new Vector2(newCuePos, cue.transform.localPosition.y);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "GameController" || other.tag == "Player")
        {
            Physics2D.IgnoreCollision(other, GetComponent<Collider2D>()); //no collision for player
        }
    }
}
