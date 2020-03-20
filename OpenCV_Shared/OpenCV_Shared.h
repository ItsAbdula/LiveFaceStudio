#pragma once
#include "opencv2/core.hpp"
#include <opencv2/opencv.hpp>

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
	namespace OpenCV_Shared
	{
		DLLEXPORT void STDCALL FlipImage(Color32 **rawImage, int width, int height);
		DLLEXPORT float STDCALL Foopluginmethod();
	}
}