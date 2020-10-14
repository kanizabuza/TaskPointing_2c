using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Systems{
    public class Director : MonoBehaviour{
        [SerializeField] private GameObject target;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text lengthText;
        [SerializeField] private Text targetSizeText;
        [SerializeField] private int totalLoopTime;
        [SerializeField] private Slider targetSlider;
        [SerializeField] private Text finTex;
        [SerializeField] private InputField name;
        
        private GameObject targetPrefab;
        private Vector2 preTargetPos;
        private float length = 0;
        private Vector2 clickPoint;
        private Vector2 firstClickPoint;
        private bool isFirst = true;
        private float totalScore = 0;
        private bool isCollect = false;
        private static float countTime;
        private int nowLoopCount;
        private float targetSize;

        private enum State{
            Start,
            Play,
            Result
        }
        private State state = State.Start;
        
        void Start() {
            nowLoopCount = 0;
            clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            firstClickPoint = clickPoint;

            scoreText.text = "Count:" + totalLoopTime;
            timeText.text = "Time:0";
            lengthText.text = "Length:0";
            finTex.text = "";
        }

        void Update() {
            if (Input.GetKey(KeyCode.Escape)) {
                Quit();
            }
            
            if (state == State.Start) {
                targetSize = targetSlider.value;
                targetSizeText.text = "targetSize:" + targetSize.ToString();
            }
            
            if (totalLoopTime == nowLoopCount) {
                state = State.Result;
                finTex.text = "Finish";
                return;
            }
            
            if (Input.GetMouseButtonDown(0) && state == State.Play) {
                clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D point = Physics2D.OverlapPoint(clickPoint);
                if (point) {
                    GameObject clickedObject = point.transform.gameObject;
                    isCollect = true;
                    SetLength();
                    SetScore();
                    ChangePos();
                } else {
                    isCollect = false;
                    SetLength();
                    SetScore();
                    ChangePos();
                }
                nowLoopCount++;
                scoreText.text = "Count:" + (totalLoopTime - nowLoopCount);
                WritePointingData();
            }
            if(state == State.Play) UpdateTime();
        }

        private void ChangePos() {
            countTime = 0;
            preTargetPos = targetPrefab.transform.position;
            targetPrefab.transform.position = new Vector2(Random.Range(-7.0f, 7.0f), Random.Range(-4.0f, 4.0f));
        }


        private void SetLength() {
            if (!isFirst) {
                length = Vector2.Distance(clickPoint, preTargetPos);
                lengthText.text = length.ToString();
            } else {
                length = Vector2.Distance(firstClickPoint, preTargetPos);
                lengthText.text = length.ToString();
                isFirst = false;
            }
        }

        private void SetScore() {
            if (!isCollect) {
                return;
            } else {
                totalScore += length;
                scoreText.text = "Score:" + totalScore;
            }
        }
        
        private void UpdateTime() {
            countTime += Time.deltaTime;
            timeText.text = countTime.ToString("F2");
        }

        public void ChangeState() {
            if (state == State.Start) {
                state = State.Play;
                target.transform.localScale = new Vector2(targetSize*0.4f,targetSize*0.4f);
                targetPrefab = Instantiate(target);
                targetPrefab.transform.position = new Vector2(Random.Range(-7.0f, 7.0f), Random.Range(-4.0f, 4.0f));
                preTargetPos = targetPrefab.transform.position;
            }else if (state == State.Play) {
                state = State.Result;
                finTex.text = "Finish";
            } else {
                state = State.Start;
            }
        }

        private void Quit() {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_STANDALONE
            UnityEngine.Application.Quit();
            #endif
        }

        private void WritePointingData() {
            using (StreamWriter streamWriter = new StreamWriter("C:/Users/hika0/Documents/pointingTask01.csv",true, Encoding.GetEncoding("Shift_JIS"))) {
                streamWriter.WriteLine(name.text + "," + isCollect + ", "+lengthText.text + ", "+timeText.text + ",");
                streamWriter.Close();
            }
        }
    }
}