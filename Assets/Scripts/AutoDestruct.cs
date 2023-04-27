using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AutoDestruct : MonoBehaviour
{
    [SerializeField] private float destructTime = 2.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destructTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
