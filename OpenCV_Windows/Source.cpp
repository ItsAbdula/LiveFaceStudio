#include <dlib/image_processing/frontal_face_detector.h>
#include <dlib/image_processing/render_face_detections.h>
#include <dlib/image_processing.h>
#include <dlib/opencv.h>
#include <dlib/gui_widgets.h>
#include <dlib/image_io.h>

#include <opencv2/core.hpp>
#include <opencv2/core/types_c.h>
#include <opencv2/imgproc.hpp>
#include <opencv2/face/facemark.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/videoio.hpp>
#include <opencv2/objdetect.hpp>

#include <iostream>

#define WIDTH 640
#define HEIGHT 480

using namespace std;
using namespace cv;
using namespace dlib;

image_window win, win_faces;

void detectAndDraw(Mat& img, CascadeClassifier& cascade, CascadeClassifier& nestedCascade, double scale);

std::vector<full_object_detection> detectFaceLandmark(frontal_face_detector &detector, shape_predictor &sp, array2d<bgr_pixel> &img);
void drawFaceLandmarkOverlay(array2d<bgr_pixel> &dlibImage, std::vector<full_object_detection> &shapes);
void drawFaceOnlyWindow(array2d<bgr_pixel> &dlibImage, std::vector<full_object_detection> &shapes);

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

    // TODO: 웹캠에서 동작 확인
    // set Camera's Resolution
    {
        vcap.set(CAP_PROP_FRAME_WIDTH, WIDTH);
        vcap.set(CAP_PROP_FRAME_HEIGHT, HEIGHT);
    }

    frontal_face_detector detector = get_frontal_face_detector();
    shape_predictor sp;
    deserialize("shape_predictor_68_face_landmarks.dat") >> sp;

    array2d<bgr_pixel> dlibImage;
    cv_image<bgr_pixel> cvMat2dlib;

    int counter = 0;
    const int divisor = 5;

    // Camera
    while (true)
    {
        if (waitKey(1) >= 0) break;

        counter += 1;
        if (counter % divisor != 0) continue;

        counter = 0;

        if (vcap.read(src) == false)
        {
            std::cout << "No frame" << std::endl;
            waitKey();
        }

        // Set Input Image's Resolution
        {
            resize(src, dst, Size(WIDTH, HEIGHT));

            cvMat2dlib = cv_image<bgr_pixel>(dst);
            assign_image(dlibImage, cv_image<bgr_pixel>(dst));
        }

        auto shapes = detectFaceLandmark(detector, sp, dlibImage);

        drawFaceLandmarkOverlay(dlibImage, shapes);
        drawFaceOnlyWindow(dlibImage, shapes);
    }
}

std::vector<full_object_detection> detectFaceLandmark(frontal_face_detector &detector, shape_predictor &sp, array2d<bgr_pixel> &img)
{
    pyramid_up(img);

    std::vector<dlib::rectangle> dets = detector(img);
    cout << "Number of faces detected : " << dets.size() << endl;

    std::vector<full_object_detection> shapes;
    for (unsigned long i = 0; i < dets.size(); i++)
    {
        full_object_detection shape = sp(img, dets[i]);
        //cout << "Number of parts: " << shape.num_parts() << endl;
        //for (unsigned long j = 0; j < shape.num_parts(); j++)
        //{
        //    cout << "pixel position of " + to_string(j) + " part : " << shape.part(j) << endl;
        //}

        shapes.push_back(shape);
    }

    return shapes;
}

void drawFaceLandmarkOverlay(array2d<bgr_pixel> &dlibImage, std::vector<full_object_detection> &shapes)
{
    win.clear_overlay();
    win.set_image(dlibImage);
    win.add_overlay(render_face_detections(shapes));
}

void drawFaceOnlyWindow(array2d<bgr_pixel> &dlibImage, std::vector<full_object_detection> &shapes)
{
    dlib::array<array2d<rgb_pixel> > face_chips;
    extract_image_chips(dlibImage, get_face_chip_details(shapes), face_chips);
    win_faces.set_image(tile_images(face_chips));
}

void detectAndDraw(Mat& img, CascadeClassifier& cascade, CascadeClassifier& nestedCascade, double scale)
{
    const auto Color = Scalar(255, 0, 0);

    std::vector<Rect> faces;

    Mat gray;
    cvtColor(img, gray, COLOR_BGR2GRAY);

    Mat smallImg;
    double fx = 1 / scale;
    resize(gray, smallImg, Size(), fx, fx, INTER_LINEAR_EXACT);
    equalizeHist(smallImg, smallImg);

    cascade.detectMultiScale(smallImg, faces, 1.1, 2, CASCADE_SCALE_IMAGE, Size(30, 30));

    for (const auto &face : faces)
    {
        Mat smallImgROI;
        std::vector<Rect> nestedObjects;
        Point center;

        int radius;

        auto aspect_ratio = (double)face.width / face.height;
        if (0.75 < aspect_ratio && aspect_ratio < 1.3)
        {
            center.x = cvRound((face.x + face.width*0.5)*scale);
            center.y = cvRound((face.y + face.height*0.5)*scale);
            radius = cvRound((face.width + face.height)*0.25*scale);

            circle(img, center, radius, Color, 3, 8, 0);
        }
        else
        {
            cv::rectangle(img, Point(cvRound(face.x*scale), cvRound(face.y*scale)),
                Point(cvRound((face.x + face.width - 1)*scale), cvRound((face.y + face.height - 1)*scale)),
                Color, 3, 8, 0);
        }

        smallImgROI = smallImg(face);
        nestedCascade.detectMultiScale(smallImgROI, nestedObjects, 1.1, 2, CASCADE_SCALE_IMAGE, Size(30, 30));
        for (const auto &nr : nestedObjects)
        {
            center.x = cvRound((face.x + nr.x + nr.width*0.5)*scale);
            center.y = cvRound((face.y + nr.y + nr.height*0.5)*scale);
            radius = cvRound((nr.width + nr.height)*0.25*scale);

            circle(img, center, radius, Color, 3, 8, 0);
        }
    }

    imshow("result", img);
}