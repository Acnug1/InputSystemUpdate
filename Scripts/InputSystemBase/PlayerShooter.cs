using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // подключается для работы с компонентом PlayerInput

[RequireComponent(typeof(PlayerInput))]
public class PlayerShooter : MonoBehaviour
{
    public void OnShoot(InputAction.CallbackContext contex) // вызываем с помощью системы событий обработчик стрельбы и передаем в параметрах InputAction.CallbackContext contex
    {
        Debug.Log("Shoot");
    }
}
