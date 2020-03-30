#include "opencv2/objdetect.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"
#include "opencv2/videoio.hpp"
#include "opencv2/face/facemark.hpp"

#include <iostream>

using namespace std;
using namespace cv;
using namespace face;

void detectAndDraw(Mat& img, CascadeClassifier& cascade, CascadeClassifier& nestedCascade, double scale);

void detectFaceLandmark(string fileName, string image, string cascade_name);

int main(int argc, const char** argv)
{
    Mat image;
    string inputName = "test.jpg";
    bool tryflip = false;
    CascadeClassifier cascade, nestedCascade;
    cascade.load("data/haarcascades/haarcascade_frontalface_alt.xml");
    nestedCascade.load("data/haarcascades/haarcascade_eye_tree_eyeglasses.xml");

    image = imread(samples::findFileOrKeep(inputName), IMREAD_COLOR);

    if (image.empty() == false)
    {
        if (tryflip) flip(image, image, 1);

        detectAndDraw(image, cascade, nestedCascade, 1.0);
        waitKey(0);
    }

    detectFaceLandmark("data/face/face_landmark_model.dat", "george.jpg", "data/haarcascades/haarcascade_frontalface_alt.xml");

    return 0;
}

void detectFaceLandmark(string modelDataName, string image, string cascade_name)
{
    CascadeClassifier face_cascade;

    face_cascade.load(cascade_name);
    Mat img = imread(image);
    Ptr<Facemark> facemark = createFacemarkKazemi();
    facemark->loadModel(modelDataName);

    //

    vector<Rect> faces;
    resize(img, img, Size(460, 460), 0, 0, INTER_LINEAR_EXACT);
    Mat gray;
    if (img.channels() == 1)
    {
        gray = img.clone();
    }
    else
    {
        cvtColor(img, gray, COLOR_BGR2GRAY);
    }
    equalizeHist(gray, gray);
    face_cascade.detectMultiScale(gray, faces, 1.1, 3, 0, Size(30, 30));

    //

    vector< vector<Point2f> > shapes;
    if (facemark->fit(img, faces, shapes))
    {
        for (size_t i = 0; i < faces.size(); i++)
        {
            cv::rectangle(img, faces[i], Scalar(255, 0, 0));
        }
        for (unsigned long i = 0; i < faces.size(); i++)
        {
            for (unsigned long k = 0; k < shapes[i].size(); k++)
            {
                cv::circle(img, shapes[i][k], 5, cv::Scalar(0, 0, 255), FILLED);
            }
        }

        namedWindow("Detected_shape");
        imshow("Detected_shape", img);

        waitKey(0);
    }
}

void detectAndDraw(Mat& img, CascadeClassifier& cascade, CascadeClassifier& nestedCascade, double scale)
{
    const auto Color = Scalar(255, 0, 0);

    vector<Rect> faces;

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
        vector<Rect> nestedObjects;
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
            rectangle(img, Point(cvRound(face.x*scale), cvRound(face.y*scale)),
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