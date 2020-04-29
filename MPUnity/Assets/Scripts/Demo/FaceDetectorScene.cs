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

        public FaceLandmark faceImage;

        private FaceProcessorLive<WebCamTexture> processor;

        private bool onCapture = false;

        private static GameObject head = null;

        protected override void Awake()
        {
            base.Awake();
            base.forceFrontalCamera = true;

            var go = new GameObject("ResourceManager");
            go.AddComponent<ResourceManager>();

            processor = new FaceProcessorLive<WebCamTexture>();
            processor.Initialize(faces.text, eyes.text, shapes.bytes);

            // data stabilizer - affects face rects, face landmarks etc.
            processor.DataStabilizer.Enabled = true;
            processor.DataStabilizer.Threshold = 1.0;       // threshold value in pixels
            processor.DataStabilizer.SamplesCount = 1;      // how many samples do we need to compute stable data

            // performance data - some tricks to make it work faster
            processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
            processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)

            {
                head = ResourceManager.instantiatePrefab("Cube");
                head.transform.Translate(new Vector3(-1.5f, -3.0f, 0.0f));
            }
        }

        public static void setHeadRotation(double[] rotationVector)
        {
            if (head == null) return;

            var coefficient = 1.0f;
            head.transform.localEulerAngles = new Vector3(coefficient * (float)rotationVector[0], coefficient * (float)rotationVector[1], coefficient * (float)rotationVector[2]);
        }

        protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
        {
            processor.ProcessTexture(input, TextureParameters);

            processor.MarkDetected();

            output = Unity.MatToTexture(processor.Image, output);

            List<DetectedFace> Faces = processor.GetDetectedFaces();
            foreach (DetectedFace face in Faces)
            {
                faceImage.setSize(input.width, input.height);
                faceImage.setFaceRect(face.Region);

                List<string> closedItems = new List<string>(new string[] { "Nose", "Eye", "Lip" });
                int eyeCount = 0;
                int lipCount = 0;
                foreach (DetectedObject sub in face.Elements)
                {
                    if (sub.Marks != null)
                    {
                        int type = -1;
                        if (sub.Name == "Eye" || sub.Name == "Nose" || sub.Name == "Lip")
                        {
                            if (sub.Name == "Nose") type = 1;
                            else if (sub.Name == "Lip")
                            {
                                type = 2 + lipCount;
                                ++lipCount;
                            }
                            else if (sub.Name == "Eye")
                            {
                                type = 4 + eyeCount;
                                ++eyeCount;
                            }
                            IEnumerable<Point>[] ptr = new IEnumerable<Point>[] { sub.Marks };
                            faceImage.setLandmark(ptr, type);
                        }
                    }
                }
            }

            return true;
        }

        public void PressBtnCapture()
        {
            if (onCapture == false)
            {
                StartCoroutine("SaveScreenshot");
            }
        }

        private IEnumerator SaveScreenshot()
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

            RectTransform transform = faceImage.GetComponent<RectTransform>();

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
