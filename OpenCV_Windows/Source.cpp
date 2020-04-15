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