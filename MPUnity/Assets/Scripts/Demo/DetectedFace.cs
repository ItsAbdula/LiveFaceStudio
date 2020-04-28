namespace OpenCvSharp.Demo
{
	using System;
	using System.Collections.Generic;
	using OpenCvSharp;

	class DetectedObject
    {
        PointsDataStabilizer marksStabilizer = null;

        public DetectedObject(DataStabilizerParams stabilizerParameters)
        {
            marksStabilizer = new PointsDataStabilizer(stabilizerParameters);
            marksStabilizer.PerPointProcessing = false;

            Marks = null;
            Elements = new DetectedObject[0];
        }

        public DetectedObject(DataStabilizerParams stabilizerParameters, String name, Rect region)
            : this(stabilizerParameters)
        {
            Name = name;
            Region = region;
        }

        public DetectedObject(DataStabilizerParams stabilizerParameters, String name, OpenCvSharp.Point[] marks)
            : this(stabilizerParameters)
        {
            Name = name;

            marksStabilizer.Sample = marks;
            Marks = marksStabilizer.Sample;

            Region = Rect.BoundingBoxForPoints(marks);
        }

        public String Name { get; protected set; }

        public Rect Region { get; protected set; }

        public OpenCvSharp.Point[] Marks { get; protected set; }

        public DetectedObject[] Elements { get; set; }

        public virtual bool SetMarks(Point[] marks)
        {
            marksStabilizer.Sample = marks;

            Marks = marksStabilizer.Sample;
            return marksStabilizer.LastApplied;
        }
    }

    class DetectedFace : DetectedObject
    {
        public enum FaceElements
        {
            Jaw = 0,

            LeftEyebrow,
            RightEyebrow,

            NoseBridge,
            Nose,

            LeftEye,
            RightEye,

            OuterLip,
            InnerLip
        }

        RectStabilizer faceStabilizer = null;

        public DetectedFace(DataStabilizerParams stabilizerParameters, Rect roi)
            : base(stabilizerParameters, "Face", roi)
        {
            faceStabilizer = new RectStabilizer(stabilizerParameters);
        }

        public void SetRegion(Rect roi)
        {
            faceStabilizer.Sample = roi;

            Region = faceStabilizer.Sample;
            //faceInfo = null;
        }

        public bool DefineSubObject(FaceElements element, string name, int fromMark, int toMark, bool updateMarks = true)
        {
            int index = (int)element;
            Point[] subset = Marks.SubsetFromTo(fromMark, toMark);
            DetectedObject obj = Elements[index];

            bool applied = false;
            if (null == obj)
            {
                applied = true;
                obj = new DetectedObject(faceStabilizer.Params, name, subset);
                Elements[index] = obj;
            }
            else
            {
                if (updateMarks || null == obj.Marks || 0 == obj.Marks.Length)
                    applied = obj.SetMarks(subset);
            }

            return applied;
        }

        public void SetLandmarks(Point[] points)
        {
            // set marks
            Marks = points;

            // apply subs
            if (null == Elements || Elements.Length < 9)
                Elements = new DetectedObject[9];
            int keysApplied = 0;

            // key elements
            if (null != Marks)
            {
                keysApplied += DefineSubObject(FaceElements.Nose, "Nose", 30, 35) ? 1 : 0;
                keysApplied += DefineSubObject(FaceElements.LeftEye, "Eye", 36, 41) ? 1 : 0;
                keysApplied += DefineSubObject(FaceElements.RightEye, "Eye", 42, 47) ? 1 : 0;

                // non-key but independent
                DefineSubObject(FaceElements.OuterLip, "Lip", 48, 59);
                DefineSubObject(FaceElements.InnerLip, "Lip", 60, 67);

                // dependent
                bool updateDependants = keysApplied > 0;
                DefineSubObject(FaceElements.LeftEyebrow, "Eyebrow", 17, 21, updateDependants);
                DefineSubObject(FaceElements.RightEyebrow, "Eyebrow", 22, 26, updateDependants);
                DefineSubObject(FaceElements.NoseBridge, "Nose bridge", 27, 30, updateDependants);
                DefineSubObject(FaceElements.Jaw, "Jaw", 0, 16, updateDependants);
            }

            // re-fetch marks from sub-objects as they have separate stabilizers
            List<Point> fetched = new List<Point>();
            foreach (DetectedObject obj in Elements)
                if (obj.Marks != null)
                    fetched.AddRange(obj.Marks);
            Marks = fetched.ToArray();
        }
    }
}
