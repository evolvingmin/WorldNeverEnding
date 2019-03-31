using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChallengeKit.GamePlay;

namespace ChallengeKit.Pattern
{

    public class UISystem : SystemMono
    {
        private Dictionary<string, UIComponent> uiBindings;

        private void Awake()
        {
            uiBindings = new Dictionary<string, UIComponent>();
            base.Init(new UIParser());
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public UIComponent GetUIComponent(string name)
        {
            if (uiBindings.ContainsKey(name))
            {
                return uiBindings[name];
            }
            else
            {
                UIComponent target = transform.Find(name).GetComponent<UIComponent>();

                if(target != null)
                {
                    uiBindings.Add(name, target);
                }
                else
                {
                    Debug.LogWarning("Finding UIComponent Failed, name is " + name);
                }

                return target;
            }
        }
    }


    public class UIParser : IParser
    {
        UISystem uiSystem;

        public Define.Result Init(SystemMono parentSystem)
        {
            uiSystem = (UISystem)parentSystem;

            return Define.Result.OK;
        }

        public bool ParseCommand(string Command, params object[] Objs)
        {
            //MessageSystem.Instance.BroadcastSystems(null, "SetActive", "CandleDataDisplayer", bOpen);
            switch (Command)
            {
                case "SetActive":
                    uiSystem.GetUIComponent((string)Objs[0]).gameObject.SetActive((bool)Objs[1]);
                    return true;
                case "InvalidateUI":
                    uiSystem.GetUIComponent((string)Objs[0]).InvalidateUI(Objs);
                    return true;
                /*
                case "HandleSwipe":
                    uiSystem.GetUIComponent((string)Objs[0]).HandleSwipe((float)Objs[1], (float)Objs[2]);
                    return true;
                case "BeginDrag":
                    uiSystem.GetUIComponent((string)Objs[0]).BeginDrag((float)Objs[1], (float)Objs[2]);
                    return true;
                case "DragTo":
                    uiSystem.GetUIComponent((string)Objs[0]).DragTo((float)Objs[1], (float)Objs[2]);
                    return true;
                case "EndDrag":
                    uiSystem.GetUIComponent((string)Objs[0]).EndDrag((float)Objs[1], (float)Objs[2]);
                    return true;
                */
                default:
                    return false;
            }

        }
    }
}


