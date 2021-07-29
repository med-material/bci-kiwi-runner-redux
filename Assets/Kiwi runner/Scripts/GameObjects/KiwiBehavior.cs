using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KiwiBehavior : MonoBehaviour
{
    float baseAnimSpeed;

    Rigidbody2D rb;
    Animator anim;
    readonly float jumpForce = 400f;

    AudioSource[] soundEffects;

    public GameObject thoughtBubbleBabies;
    public GameObject thoughtBubbleHeart;
    public GameObject exclamationMark;
    float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        soundEffects = GetComponents<AudioSource>();
        baseAnimSpeed = anim.speed;
    }

    public void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce);
        anim.Play("kiwiJump");
        PlaySound(2);
    }

    public void Fly()
    {
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
    }

    public void Descend()
    {
        rb.gravityScale = 1;
    }

    public void Run()
    {
        anim.Play("kiwiRun");
    }

    public void Idle()
    {
        anim.Play("kiwiIdle");
    }

    public void PlaySound(int i)
    {
        soundEffects[i].Play();
    }

    public void Reset()
    {
        anim.speed = baseAnimSpeed;
    }

    public void SlowDown()
    {
        anim.speed = baseAnimSpeed * 0.5f;
    }

    public void ThinkOfBabies()
    {
        thoughtBubbleBabies.SetActive(true);
    }

    void Happy()
    {
        thoughtBubbleHeart.SetActive(true);
        exclamationMark.SetActive(false);
        Idle();
        PlaySound(1);
    }

    public void Exclaim()
    {
        exclamationMark.SetActive(true);
        speed = 1.75f;
        PlaySound(0);
    }

    public void ResetThoughts()
    {
        thoughtBubbleBabies.SetActive(false);
        exclamationMark.SetActive(false);
    }

    public void SeeBabies(Vector2 babies)
    {
        Invoke("Exclaim", 1f);
        StartCoroutine(RunAtBabies(babies));
    }

    IEnumerator RunAtBabies(Vector2 babies)
    {
        float t = 0;
        Vector2 startPos = transform.position;
        float timeToMove = 4;
        speed = 1;

        while(t < timeToMove)
        {
            t += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(startPos, new Vector2(babies.x - 1, startPos.y), t / timeToMove);
            yield return null;
        }

        Happy();

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "GameController")
        {
            Physics2D.IgnoreCollision(other, GetComponent<Collider2D>()); //no collision for player
        }
    }
}
