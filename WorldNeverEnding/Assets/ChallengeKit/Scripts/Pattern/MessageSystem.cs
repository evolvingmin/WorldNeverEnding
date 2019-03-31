using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeKit.Pattern
{
    public class MessageSystem : Singleton<MessageSystem>
    {
        List<SystemMono> systemObjects;

        public void Listen(SystemMono systemObject)
        {
            if(systemObjects == null)
            {
                systemObjects = new List<SystemMono>();
            }

            if (systemObjects.Contains(systemObject) == true)
                return;

            systemObjects.Add(systemObject);
        }
        // 진짜 페킷을 가정한다면 이런 유연한 시스템은 만들어 질 수 없다. 아니면 레이어 하나 더 두던가.
        // 그냥 클래스 뭉터기 하나 던지고 받는게 더 나을거다.
        public void BroadcastSystems(SystemMono sender, string command, params object[] objs)
        {
            foreach (var system in systemObjects)
            {
                if (sender == system)
                    continue;

                if (system == null)
                    continue;

                system.ProcReceive(command, objs);
            }
            
        }
    }

    public class SystemMono : MonoBehaviour, IMessenger
    {
        private IParser parser;

        private void Awake()
        {
            MessageSystem.Instance.Listen(this);
        }

        public virtual Define.Result Init(IParser parser)
        {
            this.parser = parser;

            if (parser == null)
                return Define.Result.SYSTEM_PARSER_NULL;

            // awake를 오버로드 할 시, 명시적으로  Init 을 호출하면 마찬가지로 리슨 해줄 수 있도록.
            MessageSystem.Instance.Listen(this);

            return parser.Init(this);
        }

        public void ProcSend(string Command, params object[] Objs)
        {
            MessageSystem.Instance.BroadcastSystems(this, Command, Objs);
        }

        public void ProcReceive(string Command, params object[] Objs)
        {
            if (parser == null)
                return;

            parser.ParseCommand(Command, Objs);
        }
    }

    public interface IParser
    {
        Define.Result Init(SystemMono parentSystem);

        bool ParseCommand(string Command, params object[] Objs);
    }

}
