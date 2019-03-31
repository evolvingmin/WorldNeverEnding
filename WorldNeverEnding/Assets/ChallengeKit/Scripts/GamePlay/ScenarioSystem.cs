using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChallengeKit;
using System;

namespace ChallengeKit.GamePlay
{

    public class SelectionInfo
    {
        public int Index;
    }

    public class ScenarioSystem : ChallengeKit.Pattern.SystemMono
    {
        // todo : 어느정도 게임이 진행되었다면 이런식으로 에디터에서 세팅하는 것이 아니라, 저장된 데이터에서 불러와야 한다.
        // 이거 관련으로 다음 주제로 설정해도 될 듯하다.
        [SerializeField]
        private ScenarioNode rootNode = null;

        private ScenarioNode currentNode;

        public void StartScenarioByRoot()
        {
            currentNode = rootNode;
            ProcSend("StartDialogByScenario", currentNode);
        }

        public void UpdateScenarioNode(int index)
        {
            currentNode = rootNode.ChildNodes.Count > index ? rootNode.ChildNodes[index] : null;
 
            if (currentNode == null)
                return;

            ProcSend("StartDialogByScenario", currentNode);
        }
    }
}



