using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChallengeKit.GamePlay.EndlessWorldSystem
{
    /// 2d/3d 관계 없이, 하나의 신에서 무한정으로 확장해 나갈 수 있는 월드를 구성하고 관리하는 것을 목적으로 작성된 시스템
    /// (설령 이 시스템을 가져다 쓰지 않는다 하더라도, 이 과정을 익숙하게 여겨서 확장성 높은 게임을 만들어도 부담느끼지 않기 위한
    /// 연습 과제나 셈플로서 생각 해도 좋다.)
    /// 
    /// 해당 시스템이 커버쳐야 할 시스템을 정리하자면
    /// 지정한 프리셋 청크를 로드 할 수 있어야 한다.
    /// 렌덤으로 프리셋을 로드할 수도 있고, 정확히 지정된 위치에 프리셋 청크를 로드 할 수 있어야 한다.
    /// 이런 시스템을 만드는데 초반부 작업에서는 모든 청크에 해당하는 월드를 만들 수 없으므로, 랜덤을 목표로 한다. 
    /// 정확히 지정된 프리셋의 경우, 그런 프리셋의 위치를 놓고 에디팅 단에서 제어할 수 있도록 에디터/ 비 에디터간 전환도 가능해야 한다.
    /// 
    /// 무한한 확장 가능한 시스템은 무엇이 필요한가?
    /// 청크의 정의 : 플레이어가 세상과 인터렉션하는데 유의미한 최소 크기. 엔드레스 월드 시스템이 가질 수 있는 월드의 정의는
    /// 이 청크들의 합이라고 봐도 좋다.
    /// 청크의 속성중 크리티컬 한 속성들
    /// 엑티브 / 논엑티브
    /// 현재 유저가 플레이 하는데 활성화 된 청크와 아닌 청크를 나누는 기준은, 게임마다 사실 다르며, 직관적인 나눔은 유저의 카메라 뷰에
    /// 잡힌 범위까지가 활성화 된 청크의 대상이 되며, 나머지는 아니게 된다.
    /// 그래서 카메라 뷰를 어느정도로 잡을 것인가는 크리티컬 한 문제가 된다. 
    /// 엔드레스라 할지라도 그 범위는 무한이 될 수 없으며, (메모리에 모두 올릴 수 없으며) 유저 메모리가 감당할 수 있을만큼만 케싱하고 나머지는 IO 연산이 필요하다. 
    /// 

    /// 자 다음 문제. 그럼 얼마만큼 청크를 불러 올 것인가?
    /// 메모리의 절대적 양을 가지고 잡는건 다소 무리가 있다.
    /// 청크 한개씩 는다고 해서 그게 정량적으로 얼마만큼 늘 것인지는 계량하기 어렵기 때문이다. 청크 하나가 어떻게 구성되었느냐가 사실
    /// 더 중요한 이슈다.
    /// 그게 얼마나 메모리 사이즈를 잡을 것인지는 프로젝트 마다 상이하므로, 한번에 월드로서 유지할 청크의 숫자는 이 시스템이 정하는게 아니라 이 시스템을 가져다 쓸 사람들이 계략적으로 파악해서 정하도록 만들자. 물론 그렇게 정할때 안전마진에 유의하길 바래야 겠지만
    /// 

    // EndlessWorld가 봉사하는 단 하나의 존재
    // ITruman 의 위치를 정기적으로 업데이트에서 호출하며, 피봇 인덱스가 달라질 경우, EndlessWorldSystem 가 해당 기준점으로 추가해야 할 월드를 배치해 놓는다.
    public interface ITruman
    {
        Vector3 ReportPostion();
    }

    [RequireComponent(typeof(ResourceManager))]
    public class EndlessWorldSystem : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> randomChunkPrefabs; 
        // 랜덤하지 않은 방식으로 월드를 만들거라면, 해당 좌표에 어떤 청크를 그릴건지에 대한
        // 정보를 받아야 한다. 이를 WorldMap 정보라고(가칭) 정하자. 지금은 우선 고려하지 않는다.

        [SerializeField]
        private Vector3 worldStartPivot = Vector3.zero;

        [SerializeField]
        private bool is2D = true;

        [SerializeField]
        private bool isRandom = true;

        private ResourceManager resourceManager;

        [Tooltip("동시에 유지할 청크의 범위. 2D 라면 Y 는 뭘 기입하더라도 1로 고정됨")]
        [SerializeField]
        private Vector3 activeChunkRange;

        [SerializeField]
        private int unitChunkSize = 10;

        [SerializeField]
        private float chunkAliveTime = 10.0f;

        private ITruman truman;

        [SerializeField]
        private float checkTruman_UnitTick = 1.0f;

        private float cumulatedTime = 0.0f;

        private string resourceCategory = "EndlessWorldSystem::Chunk";

        private SortedDictionary<Tuple<int, int, int>, Chunk> world;

        private List<Chunk> collectedChunk;

        private void Awake()
        {
            resourceManager = GetComponent<ResourceManager>();
            resourceManager.Initialize();
            world = new SortedDictionary<Tuple<int, int, int>, Chunk>();
            collectedChunk = new List<Chunk>();

            if (isRandom)
            {
                foreach (var item in randomChunkPrefabs)
                {
                    resourceManager.SetPrefab<GameObject>(resourceCategory, item.name, item, transform);
                }
            }
        }

        public void CollectChunk(Chunk chunk)
        {
            collectedChunk.Add(chunk);
        }

        private Define.Result ManageWorlds(Vector3 pivotPosition)
        {
            Define.Result result = Define.Result.NOT_INITIALIZED;

            int xRange = (int)activeChunkRange.x;
            int yRange = is2D ? 1 : Mathf.Max(1, (int)activeChunkRange.y);
            int zRange = (int)activeChunkRange.z;

            int unitPivotPosX = (int)( pivotPosition.x / unitChunkSize ) * unitChunkSize;
            int unitPivotPosY = (int)( pivotPosition.y / unitChunkSize ) * unitChunkSize;
            int unitPivotPosZ = (int)( pivotPosition.z / unitChunkSize ) * unitChunkSize;

            float startXPos = unitPivotPosX + ( ( 1 - xRange ) / 2 ) * unitChunkSize;
            float currentXPos = startXPos;

            float startYPos = unitPivotPosY + ( ( 0 - yRange ) / 2 ) * unitChunkSize;
            float currentYPos = startYPos;

            float startZPos = unitPivotPosZ + ( ( 1 - zRange ) / 2 ) * unitChunkSize;
            float currentZPos = startZPos;

            for (int xIndex = 0; xIndex < xRange; xIndex++)
            {
                currentYPos = startYPos;
                for (int yIndex = 0; yIndex < yRange; yIndex++)
                {
                    currentZPos = startZPos;
                    for (int zIndex = 0; zIndex < zRange; zIndex++)
                    {
                        Tuple<int,int,int> key = new Tuple<int, int, int>((int)currentXPos, (int)currentYPos, (int)currentZPos);

                        if(world.ContainsKey(key))
                        {
                            world[key].SetAlive(true);
                        }
                        else
                        {
                            GameObject chunkObject = resourceManager.GetObject<GameObject>(resourceCategory, randomChunkPrefabs[Random.Range(0, randomChunkPrefabs.Count)].name);

                            Chunk chunk = chunkObject.GetComponent<Chunk>();
                            // 기본적으로 청크의 월드 피봇은 청크의 중앙을 가정으로 하며, y는 중앙이 아닌 바닥 기준으로 본다.
                            result = chunk.Init(key, unitChunkSize, chunkAliveTime, CollectChunk);

                            if (result != Define.Result.OK)
                            {
                                return result;
                            }
                            world.Add(key, chunk);
                        }
                        currentZPos += unitChunkSize;
                    }
                    currentYPos += unitChunkSize;
                }
                currentXPos += unitChunkSize;
            }

            if(collectedChunk.Count >0)
            {
                foreach (var chunk in collectedChunk)
                {
                    if (chunk.IsAlive)
                        continue;

                    chunk.Serialize(); // IO 생각하면 이건 좀 나이브 하다.
                    world.Remove(chunk.Key);
                    resourceManager.CollectGameObject(resourceCategory, chunk.gameObject);
                    
                }
                collectedChunk.Clear();
            }

            return result;
        }

        public Define.Result Init(ITruman truman)
        {
            this.truman = truman;

            if (truman == null)
                return Define.Result.NOT_INITIALIZED;

            Define.Result result = ManageWorlds(worldStartPivot);

            return result;
        }

        // Start is called before the first frame update
        void Start()
        {
            cumulatedTime = 0.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (truman == null)
                return;

            if(cumulatedTime > checkTruman_UnitTick)
            {
                Vector3 trumanPosition = truman.ReportPostion();
                ManageWorlds(trumanPosition);

                cumulatedTime = 0.0f;
            }

            cumulatedTime += Time.deltaTime;
        }

        
    }

}
