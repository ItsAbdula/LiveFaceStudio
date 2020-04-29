namespace OpenCvSharp.Demo
{
    using System;
    using System.Collections.Generic;
    using OpenCvSharp;

    static partial class ArrayUtilities
    {
        public static T[] RangeSubset<T>(this T[] array, int startIndex, int length)
        {
            T[] subset = new T[length];
            Array.Copy(array, startIndex, subset, 0, length);
            return subset;
        }

        public static T[] SubsetFromTo<T>(this T[] array, int fromIndex, int toIndex)
        {
            return array.RangeSubset<T>(fromIndex, toIndex - fromIndex + 1);
        }

        public static T[] Subset<T>(this T[] array, params int[] indices)
        {
            T[] subset = new T[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                subset[i] = array[indices[i]];
            }
            return subset;
        }
    }

    internal class FaceProcessorPerformanceParams
    {
        public int Downscale { get; set; }

        public int SkipRate { get; set; }

        public FaceProcessorPerformanceParams()
        {
            Downscale = 0;
            SkipRate = 0;
        }
    }

    internal class FaceProcessor<T>
        where T : UnityEngine.Texture
    {
        protected CascadeClassifier cascadeFaces = null;
        protected CascadeClassifier cascadeEyes = null;
        protected ShapePredictor shapeFaces = null;

        protected Mat processingImage = null;
        protected Double appliedFactor = 1.0;
        protected bool cutFalsePositivesWithEyesSearch = false;

        public FaceProcessorPerformanceParams Performance { get; private set; }

        public DataStabilizerParams DataStabilizer { get; private set; }

        public Mat Image { get; private set; }

        public List<DetectedFace> Faces { get; private set; }

        public FaceProcessor()
        {
            Faces = new List<DetectedFace>();
            DataStabilizer = new DataStabilizerParams();
            Performance = new FaceProcessorPerformanceParams();
        }

        public virtual void Initialize(string facesCascadeData, string eyesCascadeData, byte[] shapeData = null)
        {
            if (null == facesCascadeData || facesCascadeData.Length == 0)
                throw new Exception("FaceProcessor.Initialize: No face detector cascade passed, with parameter is required");

            FileStorage storageFaces = new FileStorage(facesCascadeData, FileStorage.Mode.Read | FileStorage.Mode.Memory);
            cascadeFaces = new CascadeClassifier();
            if (!cascadeFaces.Read(storageFaces.GetFirstTopLevelNode()))
                throw new System.Exception("FaceProcessor.Initialize: Failed to load faces cascade classifier");

            if (null != eyesCascadeData)
            {
                FileStorage storageEyes = new FileStorage(eyesCascadeData, FileStorage.Mode.Read | FileStorage.Mode.Memory);
                cascadeEyes = new CascadeClassifier();
                if (!cascadeEyes.Read(storageEyes.GetFirstTopLevelNode()))
                    throw new System.Exception("FaceProcessor.Initialize: Failed to load eyes cascade classifier");
            }

            if (null != shapeData && shapeData.Length > 0)
            {
                shapeFaces = new ShapePredictor();
                shapeFaces.LoadData(shapeData);
            }
        }

        protected virtual Mat MatFromTexture(T texture, Unity.TextureConversionParams texParams)
        {
            if (texture is UnityEngine.Texture2D)
                return Unity.TextureToMat(texture as UnityEngine.Texture2D, texParams);
            else if (texture is UnityEngine.WebCamTexture)
                return Unity.TextureToMat(texture as UnityEngine.WebCamTexture, texParams);
            else
                throw new Exception("FaceProcessor: incorrect input texture type, must be Texture2D or WebCamTexture");
        }

        protected virtual void ImportTexture(T texture, Unity.TextureConversionParams texParams)
        {
            if (null != processingImage)
                processingImage.Dispose();
            if (null != Image)
                Image.Dispose();

            Image = MatFromTexture(texture, texParams);
            if (Performance.Downscale > 0 && (Performance.Downscale < Image.Width || Performance.Downscale < Image.Height))
            {
                int w = Image.Width;
                int h = Image.Height;

                if (w >= h)
                {
                    appliedFactor = (double)Performance.Downscale / (double)w;
                    w = Performance.Downscale;
                    h = (int)(h * appliedFactor + 0.5);
                }
                else
                {
                    appliedFactor = (double)Performance.Downscale / (double)h;
                    h = Performance.Downscale;
                    w = (int)(w * appliedFactor + 0.5);
                }

                processingImage = new Mat();
                Cv2.Resize(Image, processingImage, new Size(w, h));
            }
            else
            {
                appliedFactor = 1.0;
                processingImage = Image;
            }
        }

        public virtual void ProcessTexture(T texture, Unity.TextureConversionParams texParams, bool detect = true)
        {
            ImportTexture(texture, texParams);

            if (detect)
            {
                double invF = 1.0 / appliedFactor;
                DataStabilizer.ThresholdFactor = invF;

                Mat gray = new Mat();
                Cv2.CvtColor(processingImage, gray, ColorConversionCodes.BGR2GRAY);

                Cv2.EqualizeHist(gray, gray);

                Rect[] rawFaces = cascadeFaces.DetectMultiScale(gray, 1.2, 6);
                if (Faces.Count != rawFaces.Length)
                    Faces.Clear();

                int facesCount = 0;
                for (int i = 0; i < rawFaces.Length; ++i)
                {
                    Rect faceRect = rawFaces[i];
                    Rect faceRectScaled = faceRect * invF;
                    using (Mat grayFace = new Mat(gray, faceRect))
                    {
                        if (cutFalsePositivesWithEyesSearch && null != cascadeEyes)
                        {
                            Rect[] eyes = cascadeEyes.DetectMultiScale(grayFace);
                            if (eyes.Length == 0 || eyes.Length > 2)
                                continue;
                        }

                        DetectedFace face = null;
                        if (Faces.Count < i + 1)
                        {
                            face = new DetectedFace(DataStabilizer, faceRectScaled);
                            Faces.Add(face);
                        }
                        else
                        {
                            face = Faces[i];
                            face.SetRegion(faceRectScaled);
                        }

                        facesCount++;
                        if (null != shapeFaces)
                        {
                            Point[] marks = shapeFaces.DetectLandmarks(gray, faceRect);

                            // 68-point predictor
                            if (marks.Length == 68)
                            {
                                List<Point> converted = new List<Point>();
                                foreach (Point pt in marks)
                                    converted.Add(pt * invF);

                                face.SetLandmarks(converted.ToArray());
                            }
                        }
                    }
                }
            }
        }

        public void MarkDetected(bool drawSubItems = true)
        {
            foreach (DetectedFace face in Faces)
            {
                Cv2.Rectangle((InputOutputArray)Image, face.Region, Scalar.FromRgb(255, 0, 0), 3);

                Mat LandMarkImage = new Mat(Image.Size(), Image.Type());

                if (drawSubItems)
                {
                    List<string> closedItems = new List<string>(new string[] { "Nose", "Eye", "Lip" });
                    foreach (DetectedObject sub in face.Elements)
                    {
                        if (sub.Marks != null)
                        {
                            Scalar color = Scalar.FromRgb(255, 255, 255);
                            if (sub.Name == "Nose") color = Scalar.FromRgb(255, 0, 0);
                            else if (sub.Name == "Eye") color = Scalar.FromRgb(0, 255, 0);
                            else if (sub.Name == "Lip") color = Scalar.FromRgb(0, 0, 255);
                            Cv2.Polylines(Image, new IEnumerable<Point>[] { sub.Marks }, /*closedItems.Contains(sub.Name)*/true, color, 3);
                        }
                    }
                }

                {
                    if (face.Marks == null || face.Marks.Length == 0) return;

                    FaceDetectorScene.setHeadRotation(GetFaceRotation(face.Marks));
                }
            }
        }

        private double[] GetFaceRotation(Point[] marks)
        {
            var imagePoints = Get2dImagePoints(marks);
            var modelPoints = Get3dModelPoints();

            var cameraMat = GetCameraMatrix(Image.Height, new Point2d(Image.Height / 2, Image.Width / 2));
            var distanceCoeffs = new double[4];
            var rotationVector = new double[4];
            var translationVector = new double[4];

            Cv2.SolvePnP(modelPoints, imagePoints, cameraMat, distanceCoeffs, out rotationVector, out translationVector);

            var noseEndPoint3d = new List<Point3f> { new Point3f(0, 0, 1000.0f) };
            var noseEndPoint2d = new Point2f[6];
            var jacobian = new double[1, 1];

            Cv2.ProjectPoints(noseEndPoint3d, rotationVector, translationVector, cameraMat, distanceCoeffs, out noseEndPoint2d, out jacobian);

            var oaMatrix = new Mat(3, 3, MatType.CV_64F);
            var oa = OutputArray.Create(oaMatrix);
            var ia = InputArray.Create(rotationVector);

            Cv2.Rodrigues(ia, oa);

            var projMatrix = new Mat(3, 4, MatType.CV_64F);
            {
                projMatrix.Set(0, 0, oa.GetMat().At<double>(0, 0));
                projMatrix.Set(1, 0, oa.GetMat().At<double>(1, 0));
                projMatrix.Set(2, 0, oa.GetMat().At<double>(2, 0));

                projMatrix.Set(0, 1, oa.GetMat().At<double>(0, 1));
                projMatrix.Set(1, 1, oa.GetMat().At<double>(1, 1));
                projMatrix.Set(2, 1, oa.GetMat().At<double>(2, 1));

                projMatrix.Set(0, 2, oa.GetMat().At<double>(0, 2));
                projMatrix.Set(1, 2, oa.GetMat().At<double>(1, 2));
                projMatrix.Set(2, 2, oa.GetMat().At<double>(2, 2));

                projMatrix.Set(0, 3, translationVector[0]);
                projMatrix.Set(1, 3, translationVector[1]);
                projMatrix.Set(2, 3, translationVector[2]);
            }

            var cameraMatrix = new Mat(3, 3, MatType.CV_64F);
            var rotMatrix = new Mat(3, 3, MatType.CV_64F);
            var translateVector = new Mat(4, 1, MatType.CV_64F);
            var eulerAngles = new Mat(3, 1, MatType.CV_64F);

            Cv2.DecomposeProjectionMatrix(projMatrix, cameraMatrix, rotMatrix, translateVector, null, null, null, eulerAngles);

            var pitch = eulerAngles.At<double>(0) * UnityEngine.Mathf.Deg2Rad;
            var yaw = eulerAngles.At<double>(1) * UnityEngine.Mathf.Deg2Rad;
            var roll = eulerAngles.At<double>(2) * UnityEngine.Mathf.Deg2Rad;

            pitch = Math.Asin(Math.Sin(pitch)) * UnityEngine.Mathf.Rad2Deg;
            roll = -Math.Asin(Math.Sin(roll)) * UnityEngine.Mathf.Rad2Deg;
            yaw = Math.Asin(Math.Sin(yaw)) * UnityEngine.Mathf.Rad2Deg;

            return new double[3] { pitch, yaw, roll };
        }

        private List<Point2f> Get2dImagePoints(Point[] points)
        {
            List<Point2f> imagePoints = new List<Point2f>
            {
                new Point2f(points[30].X, points[30].Y),    // Nose tip
                new Point2f(points[8].X, points[8].Y),      // Chin
                new Point2f(points[36].X, points[36].Y),    // Left eye left corner
                new Point2f(points[45].X, points[45].Y),    // Right eye right corner
                new Point2f(points[48].X, points[48].Y),    // Left Mouth corner
                new Point2f(points[54].X, points[54].Y)    // Right mouth corner
            };

            return imagePoints;
        }

        private List<Point3f> Get3dModelPoints()
        {
            List<Point3f> modelPoints = new List<Point3f>
            {
                new Point3f(0.0f, 0.0f, 0.0f), //The first must be (0,0,0) while using POSIT
                new Point3f(0.0f, -330.0f, -65.0f),
                new Point3f(-225.0f, 170.0f, -135.0f), //new Point3f(165.0f, 170.0f, -135.0f),
                new Point3f(225.0f, 170.0f, -135.0f), //new Point3f(165.0f, 170.0f, -135.0f),
                new Point3f(-150.0f, -150.0f, -125.0f),
                new Point3f(150.0f, -150.0f, -125.0f)
            };

            return modelPoints;
        }

        private double[,] GetCameraMatrix(double focalLength, Point2d center)
        {
            double[,] cameraMatrix = new double[3, 3];
            cameraMatrix[0, 0] = focalLength;
            cameraMatrix[0, 1] = 0;
            cameraMatrix[0, 2] = center.X;

            cameraMatrix[1, 0] = 0;
            cameraMatrix[1, 1] = focalLength;
            cameraMatrix[1, 2] = center.Y;

            cameraMatrix[2, 0] = 0;
            cameraMatrix[2, 1] = 0;
            cameraMatrix[2, 2] = 1;

            return cameraMatrix;
        }

        public List<DetectedFace> GetDetectedFaces()
        {
            return Faces;
        }
    }

    internal class FaceProcessorLive<T> : FaceProcessor<T>
        where T : UnityEngine.Texture
    {
        private int frameCounter = 0;

        public FaceProcessorLive()
            : base()
        { }

        public override void ProcessTexture(T texture, Unity.TextureConversionParams texParams, bool detect = true)
        {
            bool acceptedFrame = (0 == Performance.SkipRate || 0 == frameCounter++ % Performance.SkipRate);
            base.ProcessTexture(texture, texParams, detect && acceptedFrame);
        }
    }
}