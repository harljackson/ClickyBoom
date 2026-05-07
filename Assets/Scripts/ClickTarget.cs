using UnityEngine;

public class ClickTarget : MonoBehaviour
{
    [SerializeField] private int points = 1;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private AudioClip destroySound;

    private TargetSpawner spawner;
    private bool destroyed;
    private Vector3 startScale;

    public void Initialise(TargetSpawner owner, int scoreValue, float secondsAlive)
    {
        spawner = owner;
        points = scoreValue;
        lifeTime = secondsAlive;
    }

    private void Update()
    {
        if (startScale == Vector3.zero)
        {
            startScale = transform.localScale;
        }

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        transform.localScale = startScale * (1f + Mathf.Sin(Time.time * 6f) * 0.04f);

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            RemoveWithoutScoring();
        }
    }

    public void DestroyTarget()
    {
        if (destroyed)
        {
            return;
        }

        destroyed = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
        }

        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        NotifySpawner();
        Destroy(gameObject);
    }

    private void RemoveWithoutScoring()
    {
        if (destroyed)
        {
            return;
        }

        destroyed = true;
        NotifySpawner();
        Destroy(gameObject);
    }

    private void NotifySpawner()
    {
        if (spawner != null)
        {
            spawner.TargetRemoved(this);
        }
    }
}
