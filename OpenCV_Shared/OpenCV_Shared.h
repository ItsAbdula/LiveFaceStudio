#pragma once
#include <opencv2/opencv.hpp>
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
#define STDCALL __stdcall
#else
#define DLLEXPORT
#define STDCALL
#endif

struct Color32
{
	uchar red;
	uchar green;
	uchar blue;
	uchar alpha;
};

extern "C"
{
	DLLEXPORT void STDCALL DetectFace(Color32 **rawImage, int width, int height);
	namespace OpenCV_Shared
	{
		DLLEXPORT void STDCALL FlipImage(Color32 **rawImage, int width, int height);
		DLLEXPORT float STDCALL Foopluginmethod();
	}
}