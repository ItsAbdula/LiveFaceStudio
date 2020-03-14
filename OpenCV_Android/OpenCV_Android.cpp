#include "OpenCV_Android.h"
#include <opencv2/opencv.hpp>

extern "C"
{
	float OpenCV_Android::Foopluginmethod()
	{
		cv::Mat img(10, 10, CV_8UC1); // use some OpenCV objects
		return img.rows * 1.0f;     // should return 10.0f
	}
}