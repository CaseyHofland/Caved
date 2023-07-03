using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Playables;

public class MemoryTrigger : MonoBehaviour
{
    private bool _triggerd = false;

    [SerializeField] Animator _scaleTip;
    public EmMovement _demazanaAnimation;

    [SerializeField] private float _reactionTime;

    InventorySystem _inventoryManager;
    countingMemories _seenMemoriesCheck;

    [SerializeField] InventoryItemSO _memory;
    [SerializeField] UnityEvent _event;
    bool _pickedUp;
    [SerializeField] private GameObject _worldSpacePickup;

    [Header("Cinemachine")]
    public CinemachineCamera _memoryCamera;
    public CinemachineCamera _emotionReactionCam;

    public ParticleSystem _memorylights;
    public bool _lookClicked;
    
    EmInput _playerInputMemory;
    
    [SerializeField] private float _thinkingTime;
    private bool _isInRange;
    public GameObject _choices;

    public DecalProjector _projector;


    private void Awake()
    {
        _inventoryManager = FindObjectOfType<InventorySystem>();
        _worldSpacePickup.SetActive(false);
        _choices.SetActive(false);
        _seenMemoriesCheck = FindObjectOfType<countingMemories>();
        _demazanaAnimation = _demazanaAnimation.GetComponent<EmMovement>();

        _memoryCamera.enabled = false;
        _emotionReactionCam.enabled = false;

        _playerInputMemory = new EmInput();

    }

    private void Start()
    {
        if(!_inventoryManager)
            _inventoryManager = FindObjectOfType<InventorySystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(_worldSpacePickup != null && !_pickedUp)
            {
                _worldSpacePickup.SetActive(true);
            }
            _triggerd = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (_worldSpacePickup != null && !_pickedUp)
            {
                _worldSpacePickup.SetActive(false);
            }
            _triggerd = false;
        }
    }

    private void Update()
    {
        if (_pickedUp)
        {
            _worldSpacePickup.SetActive(false);
        }
            
    }

    public void Remember()
    {
        if (!_pickedUp && _memory != null && _triggerd)
        {
            if (!_inventoryManager)
                _inventoryManager = FindObjectOfType<InventorySystem>();
            _inventoryManager.AddItemToSavedMemories(_memory.Id);
            //_seenMemoriesCheck._count++;

            //keeping track of mental state
            _inventoryManager.PositiveMemoriesScore += _memory.PositiveScore;
            _inventoryManager.NegativeMemoriesScore += _memory.NegativeScore;

            _pickedUp = true;
            _inventoryManager._rememberedMemories++;

            StartCoroutine(RememberImpact());

            //yield return new WaitForSeconds(_reactionTime);
            
        }
    }

    private IEnumerator RememberImpact()
    {
        //turn on emotion camera
        _emotionReactionCam.enabled = true;
        _choices.SetActive(false);

        //play remember animation
        if (_memory.PositiveScore > _memory.NegativeScore)
            _demazanaAnimation.rememberHappyAnimation();
        else
            _demazanaAnimation.rememberSadAnimation();
            //play happy animation

        yield return new WaitForSeconds(4);
            
        _emotionReactionCam.enabled = false;
        _memoryCamera.enabled = false;

        _scaleTip.Play("ANI_BtnScaleLeft");

        yield return new WaitForSeconds(2);

        if (_event != null)
            {
                _event.Invoke();
            }
    }

    public void Forget()
    {
        _projector.enabled = false;
        //_seenMemoriesCheck._count++;
        _memorylights.Stop();
        StartCoroutine(ForgetImpact());
    }

    private IEnumerator ForgetImpact()
    {
        //turn on emotion camera
        _emotionReactionCam.enabled = true;
        _choices.SetActive(false);

        //play remember animation
        _demazanaAnimation.forgetAnimation();
        //play happy animation

        yield return new WaitForSeconds(6);

        _emotionReactionCam.enabled = false;
        _memoryCamera.enabled = false;

        _scaleTip.Play("ANI_BtnScaleRight");

        yield return new WaitForSeconds(2);

        if (_event != null)
        {
            _event.Invoke();
        }
    }

    public void LookClicked()
    {
        Debug.Log(" Click");
        if (_triggerd && !_pickedUp)
            StartCoroutine(RememberChoice());
    }

    private IEnumerator RememberChoice()
    {
        if (_triggerd && !_pickedUp)
        {
            _lookClicked = true;
            _memoryCamera.enabled = true;

            yield return new WaitForSeconds(_thinkingTime);

            _choices.SetActive(true);
            _lookClicked = false;
        }
        else
        {
            yield break;
        }
    }



}
