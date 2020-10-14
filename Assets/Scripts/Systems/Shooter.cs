using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UniRx;
using Random = UnityEngine.Random;

namespace Systems{
    public class Shooter : MonoBehaviour{
        [SerializeField] private GameObject shootPoint;
        [SerializeField] private float turnSpeed = 0f;
        [SerializeField] private Text hitText;
        [SerializeField] private int totalLoopTime;
        [SerializeField] private Text finText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text countText;
        [SerializeField] private InputField name;
        [SerializeField] private bool isClickMode = false;
        [SerializeField] private GameObject target;
        [SerializeField] private bool viewTrail = false;
        
        private GameObject targetPrefab;
        private bool isCollect;
        private LineRenderer line;
        private float turnInputValue = 0f;
        private static float countTime;
        private int nowLoopCount;
        private int posNum;
        private Vector2 clickPoint;
        
        private float totalLength = 0;
        private Vector2 firstPos;
        private Vector2 afterPos;
        private LineRenderer orbitLine;
        private Vector2 prePos = new Vector2(-6,3);
        
        private enum State{
            Start,
            Play,
            Result
        }
        private State state = State.Start;

        [SerializeField] private List<Vector2> posList;

        private void Start() {
            nowLoopCount = 0;
            posNum = posList.Count;
            hitText = GameObject.Find("IsHit").GetComponent<Text>();
            finText = GameObject.Find("Finish").GetComponent<Text>();
            timeText = GameObject.Find("Time").GetComponent<Text>();
            countText = GameObject.Find("Count").GetComponent<Text>();
            line = GetComponent<LineRenderer>();

            orbitLine = gameObject.transform.GetChild(0).GetComponent<LineRenderer>();
            //距離計算
            afterPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(viewTrail) orbitLine.SetPosition(0, afterPos);
            Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe(_ => {
                firstPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if(viewTrail) orbitLine.SetPosition(1, firstPos);
                
                totalLength += Vector2.Distance(firstPos, afterPos);
                if(viewTrail) orbitLine.SetPosition(0,afterPos);
                afterPos = firstPos;
            }).AddTo(this);
        }

        private void Update() {
            if (state != State.Play) {
                return;
            }

            if (totalLoopTime == nowLoopCount) {
                state = State.Result;
                finText.text = "Finish";
                Cursor.visible = true;
                return;
            }
            
            if (state == State.Play && !isClickMode) {
                turnInputValue = Input.GetAxis("Mouse X");
                Turn();
                if (Input.GetMouseButtonDown(0)) {
                    Shoot();
                    
                }
            } else if(state == State.Play && isClickMode && Input.GetMouseButtonDown(0)){
                clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D point = Physics2D.OverlapPoint(clickPoint);
                if (point) {
                    GameObject clickedObject = point.transform.gameObject;
                    isCollect = true;
                    
                    hitText.text = totalLength.ToString();
                } else {
                    isCollect = false;
                    
                    hitText.text = "not hit";
                    return;
                }
                
                ChangePos();
                nowLoopCount++;
                countText.text = "Count:" + (totalLoopTime - nowLoopCount);
                WritePointingData();
                totalLength = 0;
            }
            if(state == State.Play) UpdateTime();
        }

        public void ChangeState() {
            if (state == State.Start) {
                state = State.Play;
                countText.text = "Count:" + totalLoopTime;
                targetPrefab = Instantiate(target);
                targetPrefab.transform.position = posList[0];
                totalLength = 0;
                if (!isClickMode) {
                    Cursor.visible = false;
                } else {
                    GetComponent<SpriteRenderer>().enabled = false;
                    GameObject.Find("ShootPoint").GetComponent<SpriteRenderer>().enabled = false;
                }
            }else if (state == State.Play) {
                state = State.Result;
                finText.text = "Finish";
            } else {
                state = State.Start;
            }
        }
        
        private void UpdateTime() {
            countTime += Time.deltaTime;
            timeText.text = countTime.ToString("F2");
        }
        
        private void Turn() {
            float turn = turnInputValue * -10f;
            var diff = new Vector3(turn,0,0);
            transform.RotateAround(shootPoint.transform.position, Vector3.forward, turn);
            
            Ray2D ray = new Ray2D(transform.position, transform.up);
            RaycastHit2D hit = Physics2D.BoxCast(ray.origin, Vector2.one * 0.2f, 0f, ray.direction);
            
            Debug.DrawRay(ray.origin, ray.direction * 10f);
            line.SetPosition(0, transform.position);
            
            if (hit.collider != null) {
                line.SetPosition(1, hit.point);
            } else {
                line.SetPosition(1,transform.position);
            }
        }

        private void Shoot() {
            Ray2D ray = new Ray2D(transform.position, transform.up);
            RaycastHit2D hit = Physics2D.BoxCast(ray.origin, Vector2.one * 0.2f, 0f, ray.direction);

            if (hit.collider != null) {
                //hitText.text = "hit";
                hitText.text = totalLength.ToString();
                isCollect = true;
            } else {
                hitText.text = "not hit";
                isCollect = false;
                return;
            }

            ChangePos();
            nowLoopCount++;
            countText.text = "Count:" + (totalLoopTime - nowLoopCount);
            
            WritePointingData();
            totalLength = 0;
        }
        
        private void ChangePos() {
            /*
            countTime = 0;
            if (posNum <= 0) {
                posNum = posList.Count;
            }
            posNum--;
            targetPrefab.transform.position = posList[posNum];
            */
            
            var pos = posList[Random.Range(0,4)];
            while (prePos == pos) {
                pos = posList[Random.Range(0, 4)];
            }
            targetPrefab.transform.position = pos;
            prePos = pos;
        }
        
        private void WritePointingData() {
            using (StreamWriter streamWriter = new StreamWriter("C:/Users/hika0/Documents/task2.csv",true, Encoding.GetEncoding("Shift_JIS"))) {
                streamWriter.WriteLine(name.text + ","+ isClickMode + "," +timeText.text + ","+totalLength);
                streamWriter.Close();
            }
        }
    }
}
