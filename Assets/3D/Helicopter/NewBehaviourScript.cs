using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public   Transform model;
    public Vector3 v ;
    bool IsGRounded = true;
    void Start()
    {
            v = new Vector3(model.position.x, model.position.y + 5f, model.position.z);
        
}


    void Update()
    {
        IsGRounded = model.transform.position == v;
        if (!IsGRounded)
        {

        }

        if(model.transform.position != v)
            model.Translate(Vector3.up * Time.deltaTime );
         
    }
}
