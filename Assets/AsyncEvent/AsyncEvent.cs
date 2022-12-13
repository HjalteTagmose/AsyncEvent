using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncEvent
{
	[Serializable]
	public class AsyncEvent
	{
		[SerializeField] private AsyncEventType type;
		[SerializeField] private AsyncMethodCall[] calls;

		public AsyncEventType Type 
		{
			get => type;
			set => type = value;
		}

        public async Task Invoke()
		{
			switch (Type)
			{
				case AsyncEventType.WhenAll:   await InvokeWhenAll();	break;
				case AsyncEventType.Sequence:  await InvokeSequenced(); break;
				case AsyncEventType.Synchronous:     InvokeSync();		break;
			}
		}

		public async Task Invoke(AsyncEventType type)
		{
			switch (type)
			{
				case AsyncEventType.WhenAll:   await InvokeWhenAll();	break;
				case AsyncEventType.Sequence:  await InvokeSequenced(); break;
				case AsyncEventType.Synchronous:     InvokeSync();		break;
			}
		}

		private async Task InvokeWhenAll()
		{
			List<Task> tasks = new List<Task>();
			
			foreach (var call in calls)
				tasks.Add(call.Invoke());

			await Task.WhenAll(tasks);
        }

        private async Task InvokeSequenced()
        {
            foreach (var call in calls)
                await call.Invoke();
        }

        private void InvokeSync()
		{
			foreach (var call in calls)
				_ = call.Invoke();
		}
	}
}