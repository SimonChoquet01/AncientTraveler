using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using TMPro;

public class DialogueBehaviour : MonoBehaviour, IDamagable
{

    //--- Serializable Classes that will be used in editor ---
    [Serializable]
    private struct Dialogue
    {
        public Sprite Icon;
        [TextArea]
        public String Text;
        [Tooltip("The speed at which the text will be written in characters per second.")] public int speed;
        public UnityEvent DialogueEvent;
    }

    //--- Editor Parameters ---
    [SerializeField] [Tooltip("Does the dialogue dissappear after completion?")] private bool _disappearing = true;
    [SerializeField] [Tooltip("The sound the typing will make.")] private AudioClip _typeSound;
    [SerializeField] [Tooltip("The sound that changing to the next dialogue will make.")] private AudioClip _flipSound;
    [SerializeField] [Tooltip("The sequence of dialogues that the player will go through.")] private Dialogue[] _dialogues;

    //--- Private Variables ---
    private int _currentDialogue = 0;
    private bool _canContinue = true;
    private bool _appeared = false;

    //--- Components ---
    private Image _icon;
    private TextMeshProUGUI[] _textFields;
    private AudioSource _source;

    //--- Unity Methods ---
    void Start()
    {
        _icon = GetComponentInChildren<Image>();
        _textFields = GetComponentsInChildren<TextMeshProUGUI>();
        _source = GetComponent<AudioSource>();
        if (_dialogues.Length != 0)
        {
            _canContinue = false;
            _icon.sprite = _dialogues[0].Icon;
            foreach(TextMeshProUGUI tmp in _textFields)
            {
                tmp.enabled = false;
            }
        }
        else
        {
            _appeared = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_appeared && collision.gameObject == GameManager.Instance.Player.gameObject)
        {
            if (_dialogues.Length != 0)
            {
                _icon.enabled = true; //Just incase it was hidden in editor
                _appeared = true;
                foreach (TextMeshProUGUI tmp in _textFields)
                {
                    tmp.enabled = true;
                }
                StartCoroutine(textRoutine(_dialogues[0]));
            }
        }

    }

    //--- Inherited Methods ---
    public void TakeDamage(int damage)
    {
        if (!_canContinue) return;
        if (_dialogues.Length != 0 && _currentDialogue != _dialogues.Length - 1)
        {
            if (_dialogues[_currentDialogue].DialogueEvent != null) _dialogues[_currentDialogue].DialogueEvent.Invoke();
            _currentDialogue++;
            _icon.sprite = _dialogues[_currentDialogue].Icon;
            if (_source != null && _flipSound != null)
            {
                _source.pitch = 1;
                _source.PlayOneShot(_flipSound);
            }
            StartCoroutine(textRoutine(_dialogues[_currentDialogue]));
        }
        else
        {
            if (_disappearing) Destroy(this.gameObject);
        }
    }

    //--- Coroutines ---
    private IEnumerator textRoutine(Dialogue dialogue)
    {
        _canContinue = false;
        for (int i = 0; i <= dialogue.Text.Length; i++)
        {
            for (int j = 0; j < _textFields.Length; j++)
            {
                _textFields[j].text = dialogue.Text.Substring(0, i);
            }
            if (_source != null && _typeSound != null)
            {
                _source.pitch = UnityEngine.Random.Range(0.75f, 1.25f);
                _source.PlayOneShot(_typeSound);
            }
            yield return new WaitForSeconds(1.0f / (float)dialogue.speed);
        }
        _canContinue = true;
    }
}
