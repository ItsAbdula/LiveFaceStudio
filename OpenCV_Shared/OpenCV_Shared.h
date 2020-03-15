#pragma once

#if _WINDLL
#define DLLEXPORT __declspec(dllexport)
#define STDCALL __stdcall
#else
#define DLLEXPORT
#define STDCALL
#endif

extern "C"
{
	namespace OpenCV_Shared
	{
		DLLEXPORT float STDCALL Foopluginmethod();
	}
}