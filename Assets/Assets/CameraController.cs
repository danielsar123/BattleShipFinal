using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerView;
    public Transform enemyView;

    public float transitionDuration = 1.0f;

    private Transform targetView;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isTransitioning = false;

    private void Start()
    {
        // Set the initial view to the enemy's view
        targetView = enemyView;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // Call this method when it's the player's turn to choose
    public void SwitchToPlayerView()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCamera(playerView));
        }
    }

    // Call this method when it's not the player's turn
    public void SwitchToEnemyView()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCamera(enemyView));
        }
    }

    private IEnumerator TransitionCamera(Transform target)
    {
        isTransitioning = true;

        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;
        Quaternion initialRotation = transform.rotation;

        while (elapsedTime < transitionDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, target.position, elapsedTime / transitionDuration);
            transform.rotation = Quaternion.Slerp(initialRotation, target.rotation, elapsedTime / transitionDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;

        isTransitioning = false;
    }
}
