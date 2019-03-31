using ChallengeKit.GamePlay.EndlessWorldSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RealTruman : MonoBehaviour, ITruman
{
    [SerializeField]
    private EndlessWorldSystem endlessWorldSystem;

    [SerializeField]
    private Camera camera = null;

    [SerializeField]
    private float speed = 10.0f;

    public Vector3 ReportPostion()
    {
        return transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        endlessWorldSystem.Init(this);
    }
    
    public void GoThere(Lean.Touch.LeanFinger finger)
    {
        float dist = ( camera.transform.position - transform.position ).magnitude;

        var ray = finger.GetRay(camera);

        Vector3 touchWorldPos = finger.GetWorldPosition(dist);
        //Debug.Log("Go there, Actual pos is " + touchWorldPos);
        touchWorldPos.y = 2;

        transform.position += (touchWorldPos - transform.position).normalized * speed * Time.deltaTime;
    }


}


