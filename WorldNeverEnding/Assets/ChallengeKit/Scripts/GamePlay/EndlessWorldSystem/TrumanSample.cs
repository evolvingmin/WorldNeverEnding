using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeKit.GamePlay.EndlessWorldSystem
{
    public class TrumanSample : MonoBehaviour, ITruman
    {
        [SerializeField]
        private EndlessWorldSystem endlessWorldSystem;

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

        // Update is called once per frame
        void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 inputDir = new Vector3(horizontal, 0, vertical);

            transform.position += inputDir * speed * Time.deltaTime;
        }
    }
}

