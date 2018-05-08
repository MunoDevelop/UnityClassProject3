using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;


public class UIManager : MonoBehaviour {

	// Start Button
    [SerializeField]
    private GameObject go_start;

	// Title Panel
    [SerializeField]
    private RectTransform rtr_titlePanel;

	// HUD
    [SerializeField]
    private RectTransform rtr_hudPanel;

	
    [SerializeField]
    private Transform tr_scrollContent;


	//Icon Image Array
    [SerializeField]
    private Sprite[] icons;

    [SerializeField]
    private GameObject iconPrefab;

	// Material Array
    [SerializeField]
    private Material[] mats;

    [SerializeField]
    private Renderer testCubeRenderer;

    private Tween _punchCube;
	private Tween _rotateCube;

	// Shader Description Array
    [SerializeField]
    private string[] shaderDescs;

	// Material Description Array
	[SerializeField]
    private string[] materialDescs;

    [SerializeField]
    private CanvasGroup labelCanvas;

    [SerializeField]
    private Text shaderLabel;

    [SerializeField]
    private Text materialLabel;

    private Tween _fadeLabel;


    [SerializeField]
    private MeshFilter meshFilter;

	// Meshes for testRenderer
    [SerializeField]
    private Mesh[] meshes;
    private int _meshIndex;

	[SerializeField]
	private Button[] meshButtons;
	private Tween[] _meshBtnTweens;

	[SerializeField]
	private Color _defaultTextColor;
	[SerializeField]
	private Color _fadedColor;

	[SerializeField]
	private Slider scrubSlider;
	[SerializeField]
	private RectTransform arrow;
	private Tween _arrowTween;
	[SerializeField]
	private Button playButton;
	[SerializeField]
	private Button pauseButton;

	private Tween _playButtonTween;
	private Tween _pauseButtonTween;

	[SerializeField]
	private Button optionButton;
	private bool _option;

	[SerializeField]
	private Text aboutText;

	// Use this for initialization
	void Start () {

		go_start.GetComponent<Button>().onClick.AddListener(
			delegate {
				MovePanelOut(rtr_titlePanel, 0f, 2f, delegate () { rtr_titlePanel.gameObject.SetActive(false);});
				MovePanelIn(rtr_hudPanel, 0f, 1f, delegate() { ChangeMaterial(0, false); });
				FadePostEffect(2f);
			}
		);

		Tweens();
		FillScrollBar();
		SetupButtons();
		ChangeMaterial(Random.Range(0, mats.Length), true);
    }


	// Enable or Dusable Image Effect (Blur and Dust Particle)
	void FadePostEffect(float duration) {
		UnityStandardAssets.ImageEffects.TiltShift _filter = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.TiltShift>();
		Tween _fade = DOTween.To(() => _filter.blurArea, x => _filter.blurArea = x, 0f, duration)
			.OnComplete(delegate () { _filter.enabled = false; });
		ParticleSystem _ps = Camera.main.GetComponentInChildren<ParticleSystem>();
		_ps.Stop();
	}

	void EnablePostEffect(float duration) {
		UnityStandardAssets.ImageEffects.TiltShift _filter = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.TiltShift>();
		_filter.enabled = true;
		_filter.blurArea = 0f;
		Tween _fade = DOTween.To(() => _filter.blurArea, x => _filter.blurArea = x, 15f, duration);
		ParticleSystem _ps = Camera.main.GetComponentInChildren<ParticleSystem>();
		_ps.Play();
	}


	
	void Tweens() {
		_rotateCube = testCubeRenderer.transform.DOLocalRotate (Vector3.one * 360f, 10f, RotateMode.LocalAxisAdd).SetLoops (-1);

		_punchCube = testCubeRenderer.transform.DOPunchScale(Vector3.one * .5f, .5f, 10, 1f)
					.SetAutoKill(false).Pause();
		_fadeLabel = labelCanvas.DOFade(0f, 2f).SetDelay(4f).SetAutoKill(false).Pause();

		_meshBtnTweens = new Tween[meshButtons.Length];

		for (int i = 0; i < meshButtons.Length; i++) {
			_meshBtnTweens[i] = meshButtons[i].transform.DOPunchScale(Vector3.one * .25f, .5f, 10, 1f)
					.SetAutoKill(false).Pause();
		}

		_playButtonTween = playButton.transform.DOPunchScale(Vector3.one * .25f, .5f, 10, 1f)
			.SetAutoKill(false).Pause();
		_pauseButtonTween = pauseButton.transform.DOPunchScale(Vector3.one * .25f, .5f, 10, 1f)
			.SetAutoKill(false).Pause();

	}
	
	void SetupButtons() {
		for (int i = 0; i < meshButtons.Length; i++) {
			meshButtons[i].onClick.AddListener(delegate { ChangeMesh(); });
		}
		meshButtons[0].interactable = false;
		meshButtons[0].GetComponentInChildren<Text>().color = _fadedColor;

		playButton.onClick.AddListener (
			delegate {
			playButton.gameObject.SetActive (false);
			pauseButton.gameObject.SetActive (true);
			_pauseButtonTween.Restart();
			BackToAutoPlay ();
		});
		
		playButton.gameObject.SetActive (false);

		pauseButton.onClick.AddListener (
			delegate {
			pauseButton.gameObject.SetActive (false);
			playButton.gameObject.SetActive (true);
			_playButtonTween.Restart();
			EnableScrubbing ();
		});

		scrubSlider.maxValue = 10f;
		scrubSlider.onValueChanged.AddListener (delegate{_rotateCube.Goto (scrubSlider.value, false);});
		scrubSlider.gameObject.SetActive (false);

		optionButton.onClick.AddListener(delegate { OptionWindow(); });
	}


	void EnableScrubbing() {
		_rotateCube.Pause ();
		scrubSlider.value = 0f;
		scrubSlider.gameObject.SetActive (true);
		DOTween.To(() => scrubSlider.value, x => scrubSlider.value = x, 10f, 2f) .SetEase(Ease.OutBounce);
	}


	void BackToAutoPlay(){
		scrubSlider.gameObject.SetActive (false);
		_rotateCube.Restart ();
	}


	void OptionWindow() {
		if (_option) {
			//_rotateCube.Restart ();
			FadePostEffect(1f);
			MovePanelIn(rtr_hudPanel, 0f, 1f, delegate ()  { rtr_hudPanel.GetComponent<CanvasGroup>().interactable = true; });
			aboutText.gameObject.SetActive(false);
		} else {
			//_rotateCube.Pause ();
			EnablePostEffect(1f);
			MovePanelOut(rtr_hudPanel, 0f, 1f, delegate () { rtr_hudPanel.GetComponent<CanvasGroup>().interactable = false; });
			aboutText.gameObject.SetActive(true);

		}

		_option = !_option;
	}


	void ChangeMesh() {

		_meshIndex++;
		meshFilter.mesh = meshes[_meshIndex% meshes.Length];
		_punchCube.Restart();
		_meshBtnTweens[_meshIndex % meshes.Length].Restart();

		for (int i = 0; i < meshButtons.Length; i++) {
			meshButtons[i].interactable = true;
			meshButtons[i].GetComponentInChildren<Text>().color = _defaultTextColor;
		}
		meshButtons[_meshIndex % meshes.Length].interactable = false;
		meshButtons[_meshIndex % meshes.Length].GetComponentInChildren<Text>().color = _fadedColor;
	}


	void FillScrollBar() {
        for (int i = 0; i < icons.Length; i++) {
            GameObject go = Instantiate(iconPrefab) as GameObject;
			go.transform.SetParent(tr_scrollContent);
            go.transform.localScale = Vector3.one;
            go.GetComponent<Image>().sprite = icons[i];

            MaterialSample mSample = go.GetComponent<MaterialSample>();
            mSample.Index = i;
            Toggle to = go.GetComponent<Toggle>();
            to.onValueChanged.AddListener( delegate {ChangeMaterial(mSample.Index, false);});
            to.group = tr_scrollContent.GetComponent<ToggleGroup>();
            if (i == 0) { to.isOn = true; }
        }
    }

    void ChangeMaterial(int index, bool initial) {
         
        testCubeRenderer.material = mats[index];
        shaderLabel.text = shaderDescs[index];
        materialLabel.text = materialDescs[index];
		labelCanvas.alpha = 1f;

		if (!initial) { _punchCube.Restart(); _fadeLabel.Restart(); }
    }


    void MovePanelOut(RectTransform rtr, float x, float duration, TweenCallback startCallBack) {

		rtr.DOAnchorPosX(x, duration, false);
		rtr.GetComponent<CanvasGroup>().DOFade(0f, duration) .OnStart(startCallBack);
    }

    void MovePanelIn(RectTransform rtr, float x, float duration, TweenCallback endCallBack) {
		rtr.DOAnchorPosX(x, duration, false)
			.SetEase(Ease.OutElastic)
			.OnStart(delegate () { rtr.gameObject.SetActive(true); });
		rtr.GetComponent<CanvasGroup>().DOFade(1f, duration).OnComplete(endCallBack);
	}


}



/*    

    public enum Rezs {
        Full,
        ThreeQuater,
        Half,
        Quater
    };

    public Rezs screenRes = Rezs.ThreeQuater;

       // 화면 해상도
       switch (screenRes) {
           case Rezs.Full:
               Screen.SetResolution(Screen.width, Screen.width / 9 * 16, true);
               break;
           case Rezs.ThreeQuater:
               Screen.SetResolution(810, 1440, true);
               break;
           case Rezs.Half:
               Screen.SetResolution(540, 960, true);
               break;
           case Rezs.Quater:
               Screen.SetResolution(270, 480, true);
               break;
       };
       */
