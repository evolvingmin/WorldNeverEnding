using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChallengeKit.GamePlay.BoardSystem
{
    public enum BlockState
    {
        Default,
        Over,
        Selected,
        None
    }

    public interface IBlockInteractable
    {
        void UpdateState(BlockState newState);
    }

    public class Block : MonoBehaviour
    {
        private BoardManager boardManager;

        private Vector2 index;

        private Vector3 location;
        private Image image;

        public Rect Rect { get; private set; }
        private BlockState state = BlockState.None;

        private IBlockInteractable placedInteractable;
        public BlockState State
        {
            get
            {
                return state;
            }

            set
            {
                UpdateState(value);
            }
        }

        private bool assigned = false;
        public bool Assigned
        {
            get
            {
                return assigned;
            }
        }

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public Define.Result Initialize(BoardManager boardManager, Vector2 _index, Vector3 _location, Vector3 _scale)
        {
            this.boardManager = boardManager;
            image = GetComponent<Image>();
            index = _index;
            transform.Reset();
            location = _location;

            float x = location.x - _scale.x / 2.0f;
            float y = location.y + _scale.y / 2.0f;

            Rect = new Rect(x, y, rectTransform.sizeDelta.x* _scale.x, rectTransform.sizeDelta.y * _scale.y);
            
            transform.position = location;
            transform.localScale = _scale;

            State = BlockState.Default;

            return Define.Result.OK;
        }

        public void AttachInteratable(IBlockInteractable blockInteractable)
        {
            placedInteractable = blockInteractable;
            assigned = true;
            boardManager.ReportAssigned();
        }

        public void DettachInteratable()
        {
            placedInteractable = null;
            assigned = false;
            boardManager.ReportAssigned(false);
            ResetState();
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}] ({2},{3})", name, state, index.x, index.y);
        }

        public void OnPointerEnter()
        {
            if (state == BlockState.Selected)
                return;

            UpdateState(BlockState.Over);
        }

        public void OnPointerExit()
        {
            if (state == BlockState.Selected)
                return;

            UpdateState(BlockState.Default);
        }

        public void OnPointerClick()
        {
            boardManager.SetSelected(this);
        }

        public void ResetState()
        {
            UpdateState(BlockState.Default);
        }

        private void UpdateState(BlockState newState)
        {
            if (state == newState)
                return;

            state = newState;
            Color nextColor = boardManager.StateColors[(int)state];
            image.color = nextColor;

            if (placedInteractable == null)
                return;

            if(boardManager.UseLog)
            {
                Debug.Log("Update State :" + ToString());
            }

            placedInteractable.UpdateState(state);
        }
    }

}
