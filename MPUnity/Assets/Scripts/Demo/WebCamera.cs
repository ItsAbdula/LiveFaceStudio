namespace OpenCvSharp.Demo
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;
	using OpenCvSharp;

#if UNITY_ANDROID

    using UnityEngine.Android;

#endif

    public abstract class WebCamera: MonoBehaviour
	{
		public GameObject Surface;

		private Nullable<WebCamDevice> webCamDevice = null;
		private WebCamTexture webCamTexture = null;
		private Texture2D renderedTexture = null;

		protected bool forceFrontalCamera = false;

		/// WebCam texture parameters to compensate rotations, flips etc.
		protected Unity.TextureConversionParams TextureParameters { get; private set; }

		public string DeviceName
		{
			get
			{
				return (webCamDevice != null) ? webCamDevice.Value.name : null;
			}
			set
			{
				if (value == DeviceName)
					return;

				if (null != webCamTexture && webCamTexture.isPlaying)
					webCamTexture.Stop();

				int cameraIndex = -1;
				for (int i = 0; i < WebCamTexture.devices.Length && -1 == cameraIndex; i++)
				{
					if (WebCamTexture.devices[i].name == value)
						cameraIndex = i;
				}

				if (-1 != cameraIndex)
				{
					webCamDevice = WebCamTexture.devices[cameraIndex];
					webCamTexture = new WebCamTexture(webCamDevice.Value.name);

					ReadTextureConversionParameters();

					webCamTexture.Play();
				}
				else
				{
					throw new ArgumentException(String.Format("{0}: provided DeviceName is not correct device identifier", this.GetType().Name));
				}
			}
		}

		private void ReadTextureConversionParameters()
		{
			Unity.TextureConversionParams parameters = new Unity.TextureConversionParams();

			parameters.FlipHorizontally = forceFrontalCamera || webCamDevice.Value.isFrontFacing;
					
			if (0 != webCamTexture.videoRotationAngle)
				parameters.RotationAngle = webCamTexture.videoRotationAngle;

			TextureParameters = parameters;
		}

		protected virtual void Awake()
		{
#if UNITY_ANDROID
            if (Permission.HasUserAuthorizedPermission(Permission.Camera) == false)
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
#endif

            if (WebCamTexture.devices.Length == 1)
            {
                DeviceName = WebCamTexture.devices[WebCamTexture.devices.Length - 1].name;
            }
            else if(WebCamTexture.devices.Length>1)
            {
                string frontCamName = "";

                foreach (var camDevice in WebCamTexture.devices)
                {
                    if (camDevice.isFrontFacing)
                    {
                        frontCamName = camDevice.name;
                        break;
                    }
                }
                DeviceName = frontCamName;
            }
		}

		void OnDestroy() 
		{
			if (webCamTexture != null)
			{
				if (webCamTexture.isPlaying)
				{
					webCamTexture.Stop();
				}
				webCamTexture = null;
			}

			if (webCamDevice != null) 
			{
				webCamDevice = null;
			}
		}

		private void Update ()
		{
			if (webCamTexture != null && webCamTexture.didUpdateThisFrame)
			{
				ReadTextureConversionParameters();

				if (ProcessTexture(webCamTexture, ref renderedTexture))
				{
					RenderFrame();
				}
			}
		}

		protected abstract bool ProcessTexture(WebCamTexture input, ref Texture2D output);

		private void RenderFrame()
		{
			if (renderedTexture != null)
			{
				Surface.GetComponent<RawImage>().texture = renderedTexture;
			}
		}
	}
}