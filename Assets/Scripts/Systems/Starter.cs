using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

namespace Systems{
    public class Starter : MonoBehaviour{
        private Shooter shooter;
        private bool isClicked = false;
        [SerializeField] private Text countDown;
        [SerializeField] private float countTime;
        //[SerializeField] private InputField name;
        [SerializeField] GameObject inObj;
        private void Start() {
            shooter = GameObject.Find("Cursol").GetComponent<Shooter>();
        }

        private void Update() {
            if(isClicked) TimeCount();
        }

        public void OnClick() {
            isClicked = true;
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void TimeCount() {
            if (countTime <= 0) {
                countDown.enabled = false;
                shooter.ChangeState();
                Destroy(this.gameObject);

                inObj.SetActive(false);
            }
                                
            countTime -= 1.0f * Time.deltaTime;
            countDown.text = countTime.ToString("F0");
        }
        
    }
}
