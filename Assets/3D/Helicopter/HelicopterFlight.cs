using System.Collections;
using UnityEngine;

public class HelicopterFlight : MonoBehaviour
{
    public float takeoffSpeed = 10f; // �������� ������ ���������
    public float flightSpeed = 20f; // �������� ������ ���������
    public float rotationSpeed = 5f; // �������� �������� ���������
    public float delayBeforeFlight = 0.06f; // �������� ����� ������� ������

    public Transform basePosition; // ������� ����
    public Light selectionLight; // ��������� ����� ��� ���������� ���������
    public AudioSource selectionAudio; // ��������� ����� ��� ���������� ���������
    public Light commandLight; // ��������� ����� ��� ��������� ��� ��������� �������

    private bool isTakingOff = false; // ����, �����������, ���������� �� �����
    private Vector3 targetPosition; // �������, � ������� �������� ������ ������

    private bool isSelected = false; // ����, �����������, ��� �� ������ ��������

    private void Start()
    {
        selectionLight.enabled = false;
        selectionAudio.enabled = false;
        commandLight.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Helicopter"))
                {
                    if (!isSelected)
                    {
                        SelectHelicopter();
                    }
                    else
                    {
                        DeselectHelicopter();
                    }
                    return;
                }
                else if (hit.collider.CompareTag("Base") && isSelected)
                {
                    ReturnToBase();
                    return;
                }
            }

            if (isSelected && !isTakingOff)
            {
                isTakingOff = true;
                targetPosition = GetMouseWorldPositionWithZeroY();
                StartCoroutine(FlightCoroutine());
            }
        }
    }

    private IEnumerator FlightCoroutine()
    {
        Vector3 takeoffTarget = new Vector3(transform.position.x, targetPosition.y, transform.position.z);

        while (transform.position.y < targetPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, takeoffTarget, takeoffSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(delayBeforeFlight);

        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);

        float t = 0f;
        while (t < 1f)
        {
            t += rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return null;
        }

        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, flightSpeed * Time.deltaTime);
            yield return null;
        }

        isTakingOff = false;
        commandLight.enabled = false;
    }

    private void SelectHelicopter()
    {
        isSelected = true;
        selectionLight.enabled = true;
        selectionAudio.enabled = true;
    }

    private void DeselectHelicopter()
    {
        isSelected = false;
        selectionLight.enabled = false;
        selectionAudio.enabled = false;
    }

    private void ReturnToBase()
    {
        targetPosition = basePosition.position;
        StartCoroutine(FlightCoroutine());
    }

    private Vector3 GetMouseWorldPositionWithZeroY()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 hitPoint = ray.GetPoint(rayDistance);
            hitPoint.y = 0f;
            return hitPoint;
        }

        return transform.position;
    }
}
