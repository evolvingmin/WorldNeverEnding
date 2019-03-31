using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ChallengeKit.GamePlay
{
    
    public interface IDialogDisplayable
    {
        // This function will be called before the displaying letters. So setting displayble objects.
        void SetDisplayDialogByData(DialogSystem.Data DialogData);

        // This function will be called multiple times until hand over all the letters to make complete sentence.
        // ex ) sentencesDisplay.text += letter;
        void AppendLetterToDisplayText(char Letter);

        // During the text displaying, if user call skip events,
        // This function will be called and hand over the complete sentence at once. 
        void SetSentenceToDisplayText(string Sentence);

        // Clear all your display objects for continuing next dialog step.
        void OnClear();

        // This will be called when play all of the current displayScripts.
        void OnFinished();

        void DisplaySelectList(List<string> selections);
    }

    public class DialogSystem : Pattern.SystemMono
    {
        public class Data
        {
            public string sentences;
            public float speed;
            public string type;
        }

        [SerializeField]
        private string dialogRootPath = "Root/";

        [SerializeField]
        private bool isLoop = false;

        private IDialogDisplayable dialogDisplaybleObject;

        List<string> selections;
        private List<Data> dialogDatas;

        public int DialogLenth { get { return dialogDatas.Count; } }

        private int index = 0;
        private bool isSkip = false;
        private bool isFinished = false;
        private bool isSetDialogDisplayble = false;

        private Coroutine typingCoroutine;

        public Data GetDialogDataAt(int Index)
        {
            return dialogDatas[Index];
        }

        private void Reset()
        {
            index = 0;
            isSkip = false;
            isFinished = false;
            selections.Clear();
        }

        public void ParseCSVData(string rootpath, string parseTypeName, string tableName)
        {
            Reset();

            CsvTableHandler.ResourcePath = rootpath;

            CsvTableHandler.Table CSVTable = CsvTableHandler.Get(tableName, CsvTableHandler.StreamMode.Resource);

            Type ParseType = Type.GetType(parseTypeName);

            if(ParseType == null)
            {
                Debug.Log("Parsed Type is not Correct,Requested TypeName is :Namespace." + parseTypeName);
                Debug.Break();
            }

            dialogDatas = new List<Data>(CSVTable.Length);

            for(int i =0; i< CSVTable.Length; i++)
            {
                dialogDatas.Add((Data)CSVTable.GetAt(i).CovertToParsedRow(ParseType));
            }
        }

        public void ParseCSVData<T>(string tableName) where T : Data
        {
            Reset();

            CsvTableHandler.ResourcePath = dialogRootPath; 

            CsvTableHandler.Table CSVTable = CsvTableHandler.Get(tableName, CsvTableHandler.StreamMode.Resource);

            dialogDatas = new List<Data>(CSVTable.Length);

            foreach (var item in CSVTable.ConvertoGenericList<T>())
            {
                dialogDatas.Add(item);
            }

        }

        public Define.Result Init(Pattern.IParser parser, IDialogDisplayable dialogDisplaybleObject)
        {
            Define.Result result;

            result = base.Init(parser);

            if (result != Define.Result.OK)
                return result;

            this.dialogDisplaybleObject = dialogDisplaybleObject;

            if (dialogDisplaybleObject == null)
                return Define.Result.NOT_INITIALIZED;

            isSetDialogDisplayble = true;
            selections = new List<string>();

            return Define.Result.OK;
        }

        public void StartDialog()
        {
            dialogDisplaybleObject.OnClear();
            typingCoroutine = StartCoroutine(Typing());
        }

        public IEnumerator Typing()
        {
            if (isSetDialogDisplayble == false)
            {
                Debug.LogWarning("DialogDisplayable was not set");
                yield return null;
            }
                
            Data CurrentData = GetDialogDataAt(index);

            if(CurrentData.type == "Select")
            {
                selections.Clear();
                bool bAppend = false;
                string questions = "";
                foreach (char letter in CurrentData.sentences.ToCharArray())
                {
                    if(letter == '[')
                    {
                        bAppend = true;
                    }
                    else if (letter == ']')
                    {
                        bAppend = false;
                        selections.Add(questions);
                        questions = "";
                    }
                    else
                    {
                        if (bAppend)
                        {
                            questions += letter;
                        }
                    }
                }
                CurrentData.sentences = selections[0];
                selections.RemoveAt(0);
            }

            dialogDisplaybleObject.SetDisplayDialogByData(CurrentData);

            foreach (char letter in CurrentData.sentences.ToCharArray())
            {
                dialogDisplaybleObject.AppendLetterToDisplayText(letter);
                yield return new WaitForSeconds(CurrentData.speed);
            }

            if(CurrentData.type == "Select")
            {
                dialogDisplaybleObject.DisplaySelectList(selections);
            }

            isSkip = true;
        }

        public void ContinueDialog()
        {
            if (isSetDialogDisplayble == false)
            {
                Debug.LogWarning("DialogDisplayable was not set");
                return;
            }

            if(isLoop == false && isFinished)
            {
                return;
            }

            if (isSkip)
            {
                isFinished = index >= DialogLenth - 1;
                isSkip = false;
                
                if(isFinished)
                {
                    dialogDisplaybleObject.OnFinished();

                    if (isLoop)
                    {
                        isFinished = false;
                        index = 0;
                        StartDialog();
                    }
                }
                else
                {
                    index++;
                    StartDialog();
                }
            }
            else
            {
                // force ready to skip.
                isSkip = true;
                StopCoroutine(typingCoroutine);
                dialogDisplaybleObject.SetSentenceToDisplayText(GetDialogDataAt(index).sentences);
                if(GetDialogDataAt(index).type == "Select")
                {
                    dialogDisplaybleObject.DisplaySelectList(selections);
                }

            }
        }
    }
}
