namespace OpenCvSharp.Demo
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using UnityEngine.UI;
	using OpenCvSharp;

    using System.Collections;
    using System.IO;
    using UnityEngine.Android;

    public class FaceDetectorScene : WebCamera
	{
		public TextAsset faces;
		public TextAsset eyes;
		public TextAsset shapes;

        public RawImage landmarkImage;

		private FaceProcessorLive<WebCamTexture> processor;

        private bool onCapture = false;

        /// <summary>
        /// Default initializer for MonoBehavior sub-classes
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

			/*byte[] shapeDat = shapes.bytes;
			if (shapeDat.Length == 0)
			{
				string errorMessage =
					"In order to have Face Landmarks working you must download special pre-trained shape predictor " +
					"available for free via DLib library website and replace a placeholder file located at " +
					"\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
					"Without shape predictor demo will only detect face rects.";

#if UNITY_EDITOR
				// query user to download the proper shape predictor
				if (UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
					Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
#else
             UnityEngine.Debug.Log(errorMessage);
#endif
			}*/

			processor = new FaceProcessorLive<WebCamTexture>();
			processor.Initialize(faces.text, eyes.text, shapes.bytes);

			// data stabilizer - affects face rects, face landmarks etc.
			processor.DataStabilizer.Enabled = true;        // enable stabilizer
			processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
			processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

			// performance data - some tricks to make it work faster
			processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
			processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
		}

		/// <summary>
		/// Per-frame video capture processor
		/// </summary>
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			// detect everything we're interested in
			processor.ProcessTexture(input, TextureParameters);

			// mark detected objects
			processor.MarkDetected();

			// processor.Image now holds data we'd like to visualize
			output = Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

            landmarkImage.GetComponent<RawImage>().texture = Unity.MatToTexture(processor.LandMarkImage,output);

            return true;
		}

        public void PressBtnCapture()
        {
            if (onCapture == false)
            {
                StartCoroutine("CRSaveScreenshot");
            }
        }

        IEnumerator CRSaveScreenshot()
        {
            onCapture = true;

            yield return new WaitForEndOfFrame();

            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) == false)
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);

                yield return new WaitForSeconds(0.2f);
                yield return new WaitUntil(() => Application.isFocused == true);

                if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) == false)
                {
                    yield break;
                }
            }

            string fileLocation = "mnt/sdcard/MediaProject/";
            string filename = Application.productName + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            string finalLOC = fileLocation + filename;

            if (!Directory.Exists(fileLocation))
            {
                Directory.CreateDirectory(fileLocation);
            }

            byte[] imageByte; //스크린샷을 Byte로 저장.Texture2D use 

            RectTransform transform = landmarkImage.GetComponent<RectTransform>();

            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            UnityEngine.Rect rect = new UnityEngine.Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
            rect.x -= (transform.pivot.x * size.x);
            rect.y -= ((1.0f - transform.pivot.y) * size.y);

            Debug.Log(rect);

            Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, true);
            tex.ReadPixels(rect, 0, 0, true);
            tex.Apply();

            imageByte = tex.EncodeToPNG();
            DestroyImmediate(tex);

            File.WriteAllBytes(finalLOC, imageByte);


            AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass classUri = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject objIntent = new AndroidJavaObject("android.content.Intent", new object[2] { "android.intent.action.MEDIA_SCANNER_SCAN_FILE", classUri.CallStatic<AndroidJavaObject>("parse", "file://" + finalLOC) });
            objActivity.Call("sendBroadcast", objIntent);

            onCapture = false;
        }
    }
}