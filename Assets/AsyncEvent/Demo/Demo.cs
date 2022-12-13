using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AsyncEvent.Demo
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private AsyncEvent demoEvent;
        [SerializeField] private Button restartButton;
        [SerializeField] private Text tipText;
        [SerializeField] private Text runText;

        private Color runningColor = new Color(185f / 255f, 114f / 255f, 125f / 255f);
        private Color finishColor  = new Color(134f / 255f, 176f / 255f, 157f / 255f);

        private async void Start()
        {
            SetCallType((int)demoEvent.Type);
            await RunDemo();
        }

        private async Task RunDemo()
        {
            // Running
            runText.color = Color.white;
            runText.text = "Running";
            restartButton.interactable = false;
            Camera.main.backgroundColor = runningColor;

            // Invoke the event!
            await demoEvent?.Invoke(AsyncEventType.Sequence);

            // Finished
            runText.color = Color.green;
            runText.text = "Finished!";
            restartButton.interactable = true;
            Camera.main.backgroundColor = finishColor;
        }

        public void SetCallType(int i)
        {
            demoEvent.Type = (AsyncEventType)i;

            switch (demoEvent.Type)
            {
                case AsyncEventType.WhenAll:     tipText.text = "Tasks will run simultaneously. The event finishes when all tasks are finished."; break;
                case AsyncEventType.Sequence:    tipText.text = "Tasks will run one after another. The event finishes when last task is finished."; break;
                case AsyncEventType.Synchronous: tipText.text = "Tasks will run simultaneously. The event finished immediately!"; break;
                default: break;
            }
        }

        public void Restart()
        {
            var tasks = GetComponentsInChildren<DemoTask>();
            foreach (var task in tasks)
                task.Reset();

            _ = RunDemo();
        }
    }
}
