using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AsyncEvent.Demo
{
    public class DemoTask : MonoBehaviour
    {
        [SerializeField]
        private float time;
        private Slider progressBar;
        private Image fill;

        void Awake()
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
                progressBar.value = pct;
                await Task.Yield();
            }

            progressBar.value = 1;
            fill.color = Color.green;
        }

        public void Reset()
        {
            progressBar.value = 0;
            fill.color = Color.red;
        }
    }
}