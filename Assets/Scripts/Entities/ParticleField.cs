using System.Collections.Generic;
using UnityEngine;

public class ParticleField : MonoBehaviour
{
    //--- Structure for better handling of particle gameObject ---
    private struct Particle
    {

        public GameObject gameObject;
        public float life;
        public Vector2 movement;
        public Vector2 start;
        public Particle(GameObject gameObject, float life, Vector2 movement, Vector2 start)
        {
            this.gameObject = gameObject;
            this.life = life;
            this.movement = movement;
            this.start = start;
        }
    }

    //--- Privates Variables ---
    private Particle[] _particleArray;

    //--- Editor Parameters ---
    [Header("Field Options")]
    [SerializeField] [Tooltip("The size of the field from within which the particles will be contained.")] private float _fieldSize = 1;
    [SerializeField] [Tooltip("The direction and speed the particles will flow.")] private Vector2 _flow = Vector2.zero;
    [SerializeField] [Tooltip("The total amount of particles that happen on screen at one.")] private int _count = 10;
    [SerializeField] [Tooltip("The maximum life time of the particle before it is reset.")] private float _lifetime = 10.0f;

    [Header("Particle Settings")]
    [SerializeField] [Tooltip("A list of possible particle textures.")] private List<Sprite> _renderTextures = new List<Sprite>();
    [SerializeField] [Tooltip("How much the particle lifespan will vary. (+/-)")] private float _lifespanModifier = 1f;
    [SerializeField] [Tooltip("Speed variation (+/-) in pixels.")] private float _speedModifier = 1;

    //--- Unity Methods ---

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,_fieldSize/2);
    }

    private void Start()
    {
        generateParticles();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _particleArray.Length; i++)
        {
            if (_lifetime > 0 && ((_particleArray[i].life -= Time.fixedDeltaTime) <= 0))
                resetParticleByIndex(i);

            //Check if left radius
            if ((Vector2.Distance(_particleArray[i].start,_particleArray[i].gameObject.transform.localPosition) >= _fieldSize))
                resetParticleByIndex(i);
            _particleArray[i].gameObject.transform.localPosition = _particleArray[i].gameObject.transform.localPosition + (Vector3)_particleArray[i].movement*Time.fixedDeltaTime;
        }
    }

    //--- Custom Methods ---
    private void generateParticles()
    {
        _particleArray = new Particle[_count];
        for (int i = 0; i < _count; i++)
        {
            GameObject particle = new GameObject("particle_" + i);
            particle.AddComponent<SpriteRenderer>();
            if (_renderTextures.Count != 0)
                particle.GetComponent<SpriteRenderer>().sprite = _renderTextures[Random.Range(0,_renderTextures.Count)];
            particle.GetComponent<SpriteRenderer>().sortingOrder = -(int)this.transform.position.z;
            particle.transform.parent = transform;
            _particleArray[i] = new Particle(particle, _lifetime, _flow, transform.position);
            resetParticleByIndex(i,true);
        }
    }

    private void resetParticleByIndex(int index, bool fillSphere = false)
    {
        _particleArray[index].life = Mathf.Abs(_lifetime + Random.Range(-_lifespanModifier, _lifespanModifier));
        
        if (_flow == Vector2.zero)
        {
            _particleArray[index].gameObject.transform.localPosition = Random.insideUnitCircle*_fieldSize;
            _particleArray[index].movement = Random.insideUnitCircle;
        }
        else
        {
            float radius = _fieldSize/2;
            Vector2 offset = (-((Vector2)_flow).normalized);
            _particleArray[index].gameObject.transform.localPosition = offset * radius + Vector2.Perpendicular(offset) * Random.Range(-radius, radius);

            _particleArray[index].movement = _flow + _flow.normalized * Random.Range(-_speedModifier, _speedModifier);
        }
        _particleArray[index].start = _particleArray[index].gameObject.transform.position;
        if (fillSphere) _particleArray[index].gameObject.transform.localPosition = _particleArray[index].gameObject.transform.localPosition + (Vector3)_flow.normalized * Random.Range(0, _fieldSize);

    }
}
