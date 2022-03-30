using System.Collections;

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBehaviour : MonoBehaviour
{
    //--- Editor Parameters ---
    [Header("Configuration")]
    [SerializeField] [Tooltip("The bottom left corner of the level.")] private Vector2Int _boundsStart = new Vector2Int(-10, -10);
    [SerializeField] [Tooltip("The top right corner of the level.")] private Vector2Int _boundsEnd = new Vector2Int(10, 10);
    [SerializeField] [Tooltip("The target the camera is going to move to")] private Transform _target;

    //--- Private Variables ---
    private Camera _camera;
    private Coroutine _shaking;
    private Vector3 _offset;

    //--- Custom Methods ---
    public void ShakeCam()
    {
        if (_shaking != null)
            StopCoroutine(_shaking);
        StartCoroutine(ShakeRoutine());
    }
    public void ShakeCam(int amplitude = 5, float duration = 0.5f, int intervals = 20)
    {
        if (_shaking != null)
            StopCoroutine(_shaking);
        StartCoroutine(ShakeRoutine(amplitude,duration,intervals));
    }

    //--- Coroutines ---
    private IEnumerator ShakeRoutine(int amplitude = 5, float duration = 1.0f, int intervals = 20)
    {
        for (int i = 0; i < intervals; i++)
        {
            int pixels = ((intervals - i) * amplitude) / intervals;
            _offset = (Random.insideUnitCircle * pixels) / 16.0f;
            yield return new WaitForSeconds(duration / intervals);
        }
        _offset = Vector3.zero;
    }

    //--- Unity Methods ---
    void Start()
    {
        _camera = GetComponent<Camera>();
        if (_target == null) _target = GameManager.Instance.Player.transform;
    }
    void Update()
    {
        //Move the camera and apply offset
        this.transform.position = new Vector3(_target.position.x,_target.position.y,-10) + _offset;
        //Clamp the camera to world boundaries
        float minX = _boundsStart.x + _camera.orthographicSize * _camera.aspect;
        float maxX = _boundsEnd.x - _camera.orthographicSize * _camera.aspect;
        float minY = _boundsStart.y + _camera.orthographicSize;
        float maxY = _boundsEnd.y - _camera.orthographicSize;
        this.transform.position = new Vector3(Mathf.Clamp(transform.position.x,minX,maxX), Mathf.Clamp(transform.position.y, minY, maxY),-10);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Vector3 size = new Vector3(_boundsEnd.x - _boundsStart.x, _boundsEnd.y - _boundsStart.y, 0);
        Gizmos.DrawWireCube(new Vector3(_boundsStart.x, _boundsStart.y, 0) + size / 2, size);
    }
    
}
