using System.Collections;
using UnityEngine;

public class HelicopterTakeoff: MonoBehaviour
{
    public float takeoffSpeed = 10f; // �������� ������ ���������
    public float takeoffHeight = 100f; // ������ ������ ���������

    private bool isTakingOff = false; // ����, �����������, ���������� �� �����

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTakingOff)
        {
            isTakingOff = true;
            StartCoroutine(TakeoffCoroutine());
        }
    }

    private IEnumerator TakeoffCoroutine()
    {
        while (transform.position.y < takeoffHeight)
        {
            transform.Translate(Vector3.up * takeoffSpeed * Time.deltaTime);
            yield return null;
        }

        isTakingOff = false;
    }
}
