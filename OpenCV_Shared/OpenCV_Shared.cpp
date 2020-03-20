#include "OpenCV_Shared.h"

extern "C"
{
	DLLEXPORT void STDCALL FlipImage(Color32 **rawImage, int width, int height)
	{
		cv::Mat image(height, width, CV_8UC4, *rawImage);

		cv::flip(image, image, -1);
	}
}

extern "C"
{
	DLLEXPORT float STDCALL OpenCV_Shared::Foopluginmethod()
	{
		cv::Mat img(20, 20, CV_8UC1); // use some OpenCV objects
		return img.rows * 1.0f;     // should return 10.0f
	}
}