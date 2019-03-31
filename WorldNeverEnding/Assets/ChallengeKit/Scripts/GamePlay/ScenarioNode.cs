using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeKit.GamePlay
{
    [CreateAssetMenu(menuName = "ChallengeKit/ScenarioNode", fileName = "ScenarioNode")]
    public class ScenarioNode : ScriptableObject
    {
        public string ScriptRoot;
        public string ScriptName;

        public string DialogType;

        private List<ScenarioNode> parentNodes;
        public List<ScenarioNode> ChildNodes;

    }
}
