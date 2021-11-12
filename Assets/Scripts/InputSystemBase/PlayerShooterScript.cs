using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooterScript : MonoBehaviour
{
    private PlayerInputScript _playerInput; // PlayerInputScript будет находится в качестве скрипта а не отдельной библиотеки (компонента в инспекторе)

    private void Awake()
    {
        _playerInput = new PlayerInputScript(); // в Awake создаем (инициализируем) наш объект (экземпляр класса) _playerInput

        _playerInput.Player.Shoot.performed += ctx => OnShoot(); // у _playerInput вызываем нужную схему - Player, дальше выбираем нужное действие Shoot,
        // указываем состояние, что оно завершено успешно - performed (все остальные состояния работают уже с модификаторами)
        // и подписываемся на событие через контекст CallbackContext ctx и через лямбда выражение указываем, чем мы подписываемся, в данном случае это метод OnShoot();

        _playerInput.Player.Move.performed += ctx => OnMove(); // обращаемся к перемещению игрока (подписываемся на событие движения)
    }

    private void OnEnable() // включаем наше управление
    {
        _playerInput.Enable(); // включить компонент _playerInput
    }

    private void OnDisable() // отключаем наше управление
    {
        _playerInput.Disable(); // отключить компонент _playerInput
    }

    public void OnShoot() // вызываем с помощью системы событий обработчик стрельбы и передаем в параметрах InputAction.CallbackContext contex
    {
        Debug.Log("Shoot");
    }

    public void OnMove()
    {
        Vector2 moveDirection = _playerInput.Player.Move.ReadValue<Vector2>(); // считываем значение по событию при нажатии на кнопку и передаем его в Vector2
        Debug.Log(moveDirection); // помещаем полученное направление Vector2 в Update для плавного движения
    }
}
