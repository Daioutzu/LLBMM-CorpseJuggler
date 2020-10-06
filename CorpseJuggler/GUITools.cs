using UnityEngine;

namespace LLModdingTools
{
    public class GUITools
    {
        /// <summary>
        /// Keeps windows from being dragged off screen
        /// </summary>
        public static Rect ClampWindowToScreen(Rect rect)
        {
            rect.x = rect.x < 0 ? 0 : rect.x + rect.width > Screen.width ? Screen.width - rect.width : rect.x;
            rect.y = rect.y < 0 ? 0 : rect.y + rect.height > Screen.height ? Screen.height - rect.height : rect.y;
            return rect;
        }

        /// <summary>
        /// This should be used with ScaleGUIToViewPort if your windows are draggable
        /// </summary>
        public static Rect ClampWindowTo1080p(Rect rect)
        {
            rect.x = rect.x < 0 ? 0 : rect.x + rect.width > 1920 ? 1920 - rect.width : rect.x;
            rect.y = rect.y < 0 ? 0 : rect.y + rect.height > 1080 ? 1080 - rect.height : rect.y;
            return rect;
        }
        /// <summary>
        /// Makes GUI Elements fit within the viewport of LLB. Call this method at the start of OnGUI().
        /// </summary>
        public static void ScaleGUIToViewPort()
        {
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
            float inRatio = 1920 / 1080;
            float curRatio = currentResolution.x / currentResolution.y;
            float resolutionScale = curRatio < inRatio ? currentResolution.x / 1920 : currentResolution.y / 1080;
            GUI.matrix = Matrix4x4.TRS(KeepTo16by9(Vector3.zero), Quaternion.identity, new Vector3(resolutionScale, resolutionScale, 1));
        }
        static Vector3 KeepTo16by9(Vector3 pos)
        {
            float num = 1920f / 1080f;
            float num2 = Screen.width;
            float num3 = Screen.height;
            float num4 = num2 / num3;
            Vector3 zero = Vector3.zero;
            if (num4 < num)
            {
                zero.y = (num3 - (num2 / num)) / 2;
                zero.y *= pos.y < num2 * 0.5f ? 1 : -1;
            }
            else
            {
                zero.x = (num2 - (num3 * num)) / 2;
                zero.x *= pos.x < num2 * 0.5f ? 1 : -1;
            }
            return pos + zero;
        }
    }
}
