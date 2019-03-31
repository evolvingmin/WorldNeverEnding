using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeKit.GamePlay.EndlessWorldSystem
{

    public class Chunk : MonoBehaviour
    {
        private int unitSize = 10; // 이유닛 사이즈는 청크가 정하는게 아니라 월드가 제시 해 주어야 한다.
        private Tuple<int, int, int> key;
        public Tuple<int, int, int> Key {  get { return key; } }

        private Action<Chunk> collectSelf;

        public bool IsAlive {  get { return isAlive; } }
        private bool isAlive = false;

        private float aliveTime = 0.0f;

        private float cumulatedTime = 0.0f;

        public Define.Result Init(Tuple<int, int, int> key, int unitChunkSize, float chunkAliveTime, Action<Chunk> collectChunk)
        {
            this.key = key;
            this.collectSelf = collectChunk;
            this.aliveTime = chunkAliveTime;
            unitSize = unitChunkSize;
            transform.position = new Vector3(key.Item1, key.Item2, key.Item3);
            SetAlive(true);
            Deserialize();

            return Define.Result.OK;
        }

        public override string ToString()
        {
            return "Chunk(" + key + ")";
        }

        public void SetAlive(bool isAlive)
        {
            this.isAlive = isAlive;
            if(isAlive)
            {
                cumulatedTime = 0.0f;
            }

        }

        // 청크를 저장하고 세이브 할때, 청크 자체가 저장 대상은 아니다. 청크에서도 저장할 데이터와 아닌것을 구분해서 
        // 그것만 저장하고, 다시 로딩할때는 그 데이터의 상태를 기반으로 청크 내부를 맞춰주어야 한다.
        public void Serialize()
        {
            
        }

        public void Deserialize()
        {

        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isAlive == false)
                return;

            if(cumulatedTime > aliveTime)
            {
                collectSelf(this);
                isAlive = false;
            }

            cumulatedTime += Time.deltaTime;
        }

        void OnDrawGizmos()
        {
            if (isAlive == false)
                return;

            //Gizmos.color = new Color(1, 0, 0, 0.1f);
            //Gizmos.DrawCube(transform.position, new Vector3(UnitSize, UnitSize, UnitSize));

            // onto xz plane

            // left top
            float halfLength = unitSize / 2.0f;

            Vector3 leftTop = new Vector3(transform.position.x - halfLength, transform.position.y, transform.position.z + halfLength);

            Vector3 rightTop = new Vector3(transform.position.x + halfLength, transform.position.y, transform.position.z + halfLength);

            Vector3 leftBottom = new Vector3(transform.position.x - halfLength, transform.position.y, transform.position.z - halfLength);

            Vector3 rightBottom = new Vector3(transform.position.x + halfLength, transform.position.y, transform.position.z - halfLength);

            Gizmos.color = Color.black;

            Gizmos.DrawLine(leftTop, rightTop);

            Gizmos.DrawLine(rightTop, rightBottom);

            Gizmos.DrawLine(rightBottom, leftBottom);

            Gizmos.DrawLine(leftBottom, leftTop);
        }


    }
}
