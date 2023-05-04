using UnityEditor;

namespace AsyncEvents.Editor
{
	public static class AsyncEventEditorTools
	{
		public static void RepaintInspector(SerializedObject BaseObject)
		{
			foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
				if (item.serializedObject == BaseObject)
				{ item.Repaint(); return; }
		}
	}
}