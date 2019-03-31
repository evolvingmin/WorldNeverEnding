using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChallengeKit.Pattern;
using System;

namespace ChallengeKit
{
    public class UIComponent : MonoBehaviour
    {
        public virtual void HandleSwipe(float startX, float startY, float endX, float endY, float velocityX, float velocityY) { }
        public virtual void InvalidateUI(params object[] inputs) { }

        public virtual void BeginDrag(float screenX, float screenY) { }
        public virtual void DragTo(float screenX, float screenY) { }
        public virtual void EndDrag(float velocityXScreen, float velocityYScreen) { }

        public virtual void PointerDown(float positionX, float positionY, float longTabDuration){}
        public virtual void Tab(float positionX, float positionY){}

        public virtual void PointerUp(float positionX, float positionY){}
    }
}


