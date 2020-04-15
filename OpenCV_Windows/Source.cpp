#include <dlib/image_processing/frontal_face_detector.h>
#include <dlib/image_processing/render_face_detections.h>
#include <dlib/image_processing.h>
#include <dlib/opencv.h>
#include <dlib/gui_widgets.h>
#include <dlib/image_io.h>
#include <dlib/opencv.h>

#include <opencv2/opencv.hpp>
#include <opencv2/core.hpp>
#include <opencv2/core/types_c.h>
#include <opencv2/imgproc.hpp>
#include <opencv2/face/facemark.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/videoio.hpp>
#include <opencv2/objdetect.hpp>

#include <iostream>

#define FACE_DOWNSAMPLE_RATIO 4

using namespace std;
using namespace cv;
using namespace dlib;

void drawPolyline(Mat &img, const dlib::full_object_detection& d, const int start, const int end, bool isclosed);
void drawFace(Mat &img, const dlib::full_object_detection& d);

std::vector<cv::Point3d> get3dModelPoints();
std::vector<cv::Point2d> get2dImagePoints(full_object_detection &d);
cv::Mat getCameraMatrix(float focal_length, cv::Point2d center);

int main()
{
    VideoCapture vcap;
    Mat src;
    Mat dst;

#if IP_CAMERA
    const std::string videoStreamAddress = "http://192.168.47.5:25328/video";

    if (vcap.open(videoStreamAddress) == false)
    {
        std::cout << "Error opening video stream or file" << std::endl;
        return -1;
    }
#endif

    frontal_face_detector detector = get_frontal_face_detector();
    shape_predictor shapePredictor;
    deserialize("shape_predictor_68_face_landmarks.dat") >> shapePredictor;

    array2d<bgr_pixel> dlibImage;
    cv_image<bgr_pixel> cvMat2dlib;

    vcap >> src;
    Mat imgResized, imgDisplay;
    resize(src, imgResized, Size(), 1.0 / FACE_DOWNSAMPLE_RATIO, 1.0 / FACE_DOWNSAMPLE_RATIO);
    resize(src, imgDisplay, Size(), 0.5, 0.5);

    int counter = 0;
    const int divisor = 5;
    std::vector<dlib::rectangle> faces;

    // Camera
    while (true)
    {
        if (waitKey(1) >= 0) break;

        vcap >> src;
        resize(src, imgResized, Size(), 1.0 / FACE_DOWNSAMPLE_RATIO, 1.0 / FACE_DOWNSAMPLE_RATIO);
        cv_image<bgr_pixel> cimg_small(imgResized);
        cv_image<bgr_pixel> cimg(src);

        counter += 1;
        if (counter % divisor == 0)
        {
            counter = 0;

            faces = detector(cimg_small);
        }

        for (unsigned long i = 0; i < faces.size(); ++i)
        {
            dlib::rectangle rect(
                (long)(faces[i].left() * FACE_DOWNSAMPLE_RATIO),
                (long)(faces[i].top() * FACE_DOWNSAMPLE_RATIO),
                (long)(faces[i].right() * FACE_DOWNSAMPLE_RATIO),
                (long)(faces[i].bottom() * FACE_DOWNSAMPLE_RATIO)
            );
            full_object_detection shape = shapePredictor(cimg, rect);
            std::vector<cv::Point2d> imagePoints = get2dImagePoints(shape);
            std::vector<cv::Point3d> modelPoints = get3dModelPoints();

            double focal_length = src.cols;
            cv::Mat cameraMat = getCameraMatrix(focal_length, cv::Point2d(src.cols / 2, src.rows / 2));

            cv::Mat rotationVector;
            cv::Mat translationVector;
            cv::Mat distanceCoeffs = cv::Mat::zeros(4, 1, cv::DataType<double>::type);

            cv::solvePnP(modelPoints, imagePoints, cameraMat, distanceCoeffs, rotationVector, translationVector);

            std::vector<cv::Point3d> noseEndPoint3d;
            noseEndPoint3d.push_back(cv::Point3d(0, 0, 1000.0));

            std::vector<cv::Point2d> noseEndPoint2d;
            cv::projectPoints(noseEndPoint3d, rotationVector, translationVector, cameraMat, distanceCoeffs, noseEndPoint2d);

            cv::line(src, imagePoints[0], noseEndPoint2d[0], cv::Scalar(0, 0, 255), 2);
            drawFace(src, shape);
        }

        resize(src, imgDisplay, Size(), 0.5, 0.5);
        imshow("fast facial landmark detector", imgDisplay);
    }
}

void drawPolyline(Mat &img, const dlib::full_object_detection& d, const int start, const int end, bool isclosed = false)
{
    std::vector <Point> points;
    for (int i = start; i <= end; ++i)
    {
        points.push_back(Point(d.part(i).x(), d.part(i).y()));
    }
    cv::polylines(img, points, isclosed, Scalar(255, 0, 0), 2, 16);
}

void drawFace(Mat &img, const dlib::full_object_detection& d)
{
    drawPolyline(img, d, 0, 16);           // jaw line
    drawPolyline(img, d, 17, 21);          // left eyebrow
    drawPolyline(img, d, 22, 26);          // right eyebrow
    drawPolyline(img, d, 27, 30);          // nose bridge
    drawPolyline(img, d, 30, 35, true);    // lower nose
    drawPolyline(img, d, 36, 41, true);    // left eye
    drawPolyline(img, d, 42, 47, true);    // right eye
    drawPolyline(img, d, 48, 59, true);    // outer lip
    drawPolyline(img, d, 60, 67, true);    // inner lip
}

std::vector<cv::Point3d> get3dModelPoints()
{
    std::vector<cv::Point3d> modelPoints;

    modelPoints.push_back(cv::Point3d(0.0f, 0.0f, 0.0f)); //The first must be (0,0,0) while using POSIT
    modelPoints.push_back(cv::Point3d(0.0f, -330.0f, -65.0f));
    modelPoints.push_back(cv::Point3d(-225.0f, 170.0f, -135.0f));
    modelPoints.push_back(cv::Point3d(225.0f, 170.0f, -135.0f));
    modelPoints.push_back(cv::Point3d(-150.0f, -150.0f, -125.0f));
    modelPoints.push_back(cv::Point3d(150.0f, -150.0f, -125.0f));

    return modelPoints;
}

std::vector<cv::Point2d> get2dImagePoints(full_object_detection &d)
{
    std::vector<cv::Point2d> image_points;
    image_points.push_back(cv::Point2d(d.part(30).x(), d.part(30).y()));    // Nose tip
    image_points.push_back(cv::Point2d(d.part(8).x(), d.part(8).y()));      // Chin
    image_points.push_back(cv::Point2d(d.part(36).x(), d.part(36).y()));    // Left eye left corner
    image_points.push_back(cv::Point2d(d.part(45).x(), d.part(45).y()));    // Right eye right corner
    image_points.push_back(cv::Point2d(d.part(48).x(), d.part(48).y()));    // Left Mouth corner
    image_points.push_back(cv::Point2d(d.part(54).x(), d.part(54).y()));    // Right mouth corner
    return image_points;
}

cv::Mat getCameraMatrix(float focal_length, cv::Point2d center)
{
    cv::Mat camera_matrix = (cv::Mat_<double>(3, 3) << focal_length, 0, center.x, 0, focal_length, center.y, 0, 0, 1);
    return camera_matrix;
}