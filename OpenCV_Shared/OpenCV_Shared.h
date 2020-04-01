#pragma once
#include "opencv2/opencv.hpp"
#include "opencv2/core.hpp"
#include "opencv2/objdetect.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"
#include "opencv2/videoio.hpp"
#include <iostream>

using namespace std;
using namespace cv;

#if _WINDLL
#define DLLEXPORT __declspec(dllexport)
#define CALLCONV __stdcall
#else
#define DLLEXPORT
#define CALLCONV
#endif

struct Color32
{
    uchar red;
    uchar green;
    uchar blue;
    uchar alpha;
};

struct Circle
{
    Circle(int x, int y, int radius) : X(x), Y(y), Radius(radius) {}
    int X, Y, Radius;
};

extern "C"
{
    DLLEXPORT void CALLCONV LinkLogger(void(CALLCONV* logFunctPtr)(const char *));

    DLLEXPORT void CALLCONV FlipImage(unsigned char *rawImage, int width, int height);
    DLLEXPORT void CALLCONV DetectFace(Circle *faces, const char *cascadeXml, const char *nestedcascadeXml, unsigned char *rawImage, int width, int height);

    DLLEXPORT float CALLCONV Foopluginmethod();
}