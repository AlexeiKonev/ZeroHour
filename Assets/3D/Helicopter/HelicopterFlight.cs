using System.Collections;
using UnityEngine;

public class HelicopterFlight : MonoBehaviour
{
    public float takeoffSpeed = 10f; // �������� ������ ���������
    public float flightSpeed = 20f; // �������� ������ ���������
    public float rotationSpeed = 5f; // �������� �������� ���������
    public float delayBeforeFlight = 2f; // �������� ����� ������� ������

    public Transform basePosition; // ������� ����
    public Light selectionLight; // ��������� ����� ��� ���������� ���������
    public Light commandLight; // ��������� ����� ��� ��������� ��� ��������� �������

    public GameObject missilePrefab; // ������ ������
    public Transform missileSpawnPoint; // ������� ����� ������� ������
    public float missileCooldown = 2f; // ����� ����������� ����� ����������
    public int maxAmmo = 3; // ������������ ���������� �����������
    public float returnToBaseDelay = 5f; // �������� ����� ��������� �� ���� ����� ���������� �����������

    private bool isTakingOff = false; // ����, �����������, ���������� �� �����
    private Vector3 targetPosition; // �������, � ������� �������� ������ ������

    private bool isSelected = false; // ����, �����������, ��� �� ������ ��������

    [SerializeField] private int currentAmmo; // ������� ���������� �����������
    [SerializeField] private bool isReloading = false; // ����, �����������, ���������� �� �����������
    public AudioSource selectionAudio;
    private float enemyCheckField = 7f;
   
   

    private void Start()
    {
        currentAmmo = maxAmmo;
        selectionLight.enabled = false;
        commandLight.enabled = false;
        currentAmmo = maxAmmo;
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

        if (isSelected && !isReloading && currentAmmo > 0)
        {
            CheckForEnemy();
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
        ReloadAmmo();
        StartCoroutine(FlightCoroutine());
    }

    private void ReloadAmmo()
    {
        currentAmmo = maxAmmo;
    }

    private void CheckForEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, enemyCheckField); // ��������� ������� � ������� 10 ������

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                AttackEnemy(hitCollider.transform.position);
                break;
            }
        }
    }

    private void AttackEnemy(Vector3 enemyPosition)
    {
        if (isReloading || currentAmmo <= 0)
            return;

        StartCoroutine(ShootMissile(enemyPosition));

        currentAmmo--;
        if (currentAmmo <= 0)
        {
            ReturnToBaseAfterAmmoDepletion();
        }
    }

    private IEnumerator ShootMissile(Vector3 targetPosition)
    {
        isReloading = true;

        Quaternion initialRotation = missileSpawnPoint.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - missileSpawnPoint.position, Vector3.up);

        float t = 0f;
        while (t < 1f)
        {
            t += rotationSpeed * Time.deltaTime;
            missileSpawnPoint.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            yield return null;
        }

        Instantiate(missilePrefab, missileSpawnPoint.position, missileSpawnPoint.rotation);

        yield return new WaitForSeconds(missileCooldown);

        isReloading = false;
    }

    private void ReturnToBaseAfterAmmoDepletion()
    {
        StartCoroutine(ReturnToBaseDelayed());
    }

    private IEnumerator ReturnToBaseDelayed()
    {
        yield return new WaitForSeconds(returnToBaseDelay);

        ReturnToBase();
    }

    private Vector3 GetMouseWorldPositionWithZeroY()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 hitPoint = ray.GetPoint(rayDistance);
            hitPoint.y = 5f;
            return hitPoint;
        }

        return transform.position;
    }
}
