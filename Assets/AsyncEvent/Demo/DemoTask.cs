using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AsyncEvents.Demo
{
    public class DemoTask : MonoBehaviour
    {
        [SerializeField]
        private float time;
        private Slider progressBar;
        private Image fill;

        private void Awake()
        {
            progressBar = GetComponentInChildren<Slider>();
            fill = progressBar.fillRect.GetComponent<Image>();
            Reset();
        }

        public async Task MyTask()
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float pct = t / time;
                SetPercentage(pct);
                await Task.Yield();
            }
            SetPercentage(1);
        }

        public void Reset()
        {
            SetPercentage(0);
        }

        private void SetPercentage(float pct)
        {
            if (progressBar)
            {
                progressBar.value = pct;
                fill.color = pct == 1 ? Color.green : Color.red;
            }
        }
    }
}