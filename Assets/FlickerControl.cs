using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerControl : MonoBehaviour
{
    public bool isFlickering = false;
    public float timeDelay;
    public float _lowFlicker;
    public float _highFlicker;



    // Update is called once per frame
    void Update()
    {
        if(!isFlickering) StartCoroutine(FlickeringLight());
    }

    private IEnumerator FlickeringLight()
    {
        isFlickering= true;
        //this.gameObject.GetComponent<Light>().enabled = false;
        this.gameObject.GetComponent<Light>().intensity = _lowFlicker;
        timeDelay = Random.Range(0.01f, 0.5f);
        yield return new WaitForSeconds(timeDelay);

        //this.gameObject.GetComponent<Light>().enabled = true;
        this.gameObject.GetComponent<Light>().intensity = _highFlicker;
        timeDelay = Random.Range(0.01f, 0.2f);
        yield return new WaitForSeconds(timeDelay);
        isFlickering = false;
    }
}
