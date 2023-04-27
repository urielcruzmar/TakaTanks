using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bullet : MonoBehaviour
{
    // Explosion effect
    [SerializeField] private GameObject explosion;
    [SerializeField] private float speed = 600.0f;
    [SerializeField] private float lifeTime = 3.0f;
    public int damage = 50;
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Instantiate(explosion, contact.point, Quaternion.identity);
        Destroy(gameObject);
    }
}
