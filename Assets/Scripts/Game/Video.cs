using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class FullscreenVideoPlayer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage _rawImage;

    [Header("Settings")]
    [SerializeField] private string _videoFileName = "intro.mp4";

    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();

        if (_rawImage == null)
        {
            Debug.LogError("RawImage is not assigned!");
            return;
        }
        
        _renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        _renderTexture.Create();

        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.targetTexture = _renderTexture;

        _rawImage.texture = _renderTexture;
        _rawImage.rectTransform.anchorMin = Vector2.zero;
        _rawImage.rectTransform.anchorMax = Vector2.one;
        _rawImage.rectTransform.offsetMin = Vector2.zero;
        _rawImage.rectTransform.offsetMax = Vector2.zero;

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, _videoFileName);
        _videoPlayer.url = videoPath;

        _videoPlayer.playOnAwake = true;
        _videoPlayer.isLooping = false;
    }

    private void Start()
    {
        _videoPlayer.Play();
    }

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }
}