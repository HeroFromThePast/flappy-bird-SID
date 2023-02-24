using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBird : MonoBehaviour
{
    [SerializeField] HttpAuthHandler manager;
    public GameManager gameManager;
    public float velocity = 1;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            rb.velocity = Vector3.up * velocity;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = Vector3.up * velocity;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {   
        gameManager.GameOver();
    }
}
