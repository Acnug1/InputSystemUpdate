using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _takeDistance; // дистанция на которой мы можем взять объект
    [SerializeField] private float _holdDistance; // дистанция на которой мы держим объект
    [SerializeField] private float _throwForce; // сила, с которой мы бросаем поднятый объект
    private PlayerInput _input;

    private Vector2 _direction; // направление движения
    private Vector2 _rotate; // направление поворота мыши

    private Vector2 _rotation;

    private GameObject _currentObject; // храним в данном поле объект попадания Raycast

    private void Awake()
    {
        _input = new PlayerInput();
    }

    private void OnEnable()
    {
        _input.Enable(); // включаем компонент PlayerInput

        _input.Player.PickUp.performed += ctx => TryPickUp(); // подписываемся на событие: "попытаться поднять предмет"
        _input.Player.Throw.performed += ctx => Throw(); // подписываемся на событие: "бросить предмет"
        _input.Player.Drop.performed += ctx => Throw(true); // подписываемся на событие: "положить предмет"
        _input.Player.Click.performed += ctx => // есть 3 вида события started, performed и canceled
        {
            if (ctx.interaction is MultiTapInteraction) // если наш interaction является MultiTapInteraction (is - вернет bool). Сравниваем наш interaction
                DropWeapon(); // вызываем метод  DropWeapon

            if (ctx.interaction is SlowTapInteraction)
                Shoot(); // вызываем метод Shoot
        };
        _input.Player.Boat.performed += ctx => // если комбинация клавиш для раскачивания лодки нажата верно (вправо и менее, чем через 0.2 сек влево)
        {
            if (ctx.interaction is SimpleInteraction) // и если наш interaction является SimpleInteraction
                Boat(); // то вызываем метод для раскачивания лодки на веслах
        };
    }

    private void OnDisable()
    {
        _input.Disable(); // отключаем компонент PlayerInput

        _input.Player.PickUp.performed -= ctx => TryPickUp(); // отписываемся от события: "попытаться поднять предмет"
        _input.Player.Throw.performed -= ctx => Throw(); // отписываемся от события: "бросить предмет"
        _input.Player.Drop.performed -= ctx => Throw(true); // отписываемся от события: "положить предмет"

        _input.Player.Click.performed -= ctx => // есть 3 вида события started, performed и canceled
        {
            if (ctx.interaction is MultiTapInteraction) // если наш interaction является MultiTapInteraction (is - вернет bool). Сравниваем наш interaction
                DropWeapon(); // вызываем метод  DropWeapon

            if (ctx.interaction is SlowTapInteraction)
                Shoot(); // вызываем метод Shoot
        };
        _input.Player.Boat.performed -= ctx =>
        {
            if (ctx.interaction is SimpleInteraction)
                Boat();
        };
    }

    private void Update()
    {
        // Значения типа Value нужно считывать через Update, а не систему событий, так как иначе событием будет передано одно изменение значения при нажатии на клавишу движения игроком 
        // (например, 1 или -1 без сброса в 0 по окончанию нажатия на клавишу), что приведет к бесконечному зацикливанию движения игрока в последнем заданном направлении
        _rotate = _input.Player.Look.ReadValue<Vector2>(); // считываем значение поворота и записываем в Vector2
        _direction = _input.Player.Move.ReadValue<Vector2>(); // считываем значение движения и записываем в Vector2

        Look(_rotate);
        Move(_direction);
    }

    private void Look(Vector2 rotate)
    {
        if (rotate.sqrMagnitude < 0.1) // если квадрат длины вектора меньше 0.1. Берем именно квадрат длины, чтобы учитывать отрицательные значения (минимальное отклонение стика на геймпаде)
            return; // то наш метод не выполняется (движение не происходит)

        float scaledRotateSpeed = _rotateSpeed * Time.deltaTime; // задаем масштаб скорости поворота
        _rotation.y += rotate.x * scaledRotateSpeed; // поворот по оси X (изменяется на значение: вправо 0+1*10 или влево 0+(-1)*10)
        _rotation.x = Mathf.Clamp(_rotation.x - rotate.y * scaledRotateSpeed, -90, 90); // поворот по оси Y, залоченный при взгляде вверх 0-(1*10) и вниз 0-(-1*10) на углах -90 и 90 соответственно
        transform.localEulerAngles = _rotation; // поворот игрока в локальной системе координат (в локальных углах Эйлера от -180 до 180 градусов, относительно себя) равен заданному повороту (используем localEulerAngles, а не transform.localrotation для работы с Vector2 в градусах)
    }

    private void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.1)
            return;

        float scaledMoveSpeed = _moveSpeed * Time.deltaTime; // задаем масштаб скорости движения
        Vector3 move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(_direction.x, 0, _direction.y); // задаем направление движения игрока относительно (глобальной координаты угла Эйлера) поворота мыши по оси X (в углах Эйлера это ось Y, так как оси там перепутаны местами)
        transform.position += move * scaledMoveSpeed; // движение игрока будет принимать значение направления нормализованного вектора в пространстве * скорость игрока * время кадра
    }

    private void TryPickUp()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, _takeDistance) && !hitInfo.collider.gameObject.isStatic && _currentObject == null) // делаем рейкаст и записываем значения по попаданию по объектам (проверяем также, чтобы коллайдер объекта в который мы попали был нестатический)
        {
            _currentObject = hitInfo.collider.gameObject; // записываем в _currentObject ссылку на объект столкновения Raycast

            _currentObject.transform.position = default; // нашему текущему объекту, который мы держим, мы обнуляем позицию
            _currentObject.transform.SetParent(transform, worldPositionStays: false); // после чего устанавливаем ему родителя (делаем доп параметр, чтобы объект двигался, относительно родителя в его координатах)
            _currentObject.transform.localPosition = new Vector3(0, 0, _holdDistance); // сдвинем ему немного позицию на _holdDistance в локальных координатах игрока, чтобы он находился на небольшой дистанции по оси Z (спереди) от игрока 

            _currentObject.GetComponent<Rigidbody>().isKinematic = true; // делаем его Rigidbody isKinematic (чтобы он не падал на землю при подъеме)
        }
    }

    private void Throw(bool drop = false) // делаем необязательный параметр "положить (Drop)", по умолчанию равный false, в методе "бросание (Throw)"
    {
        if (_currentObject != null) // если объект поднят игроком
        {
            _currentObject.transform.parent = null; // оставляем текущий объект без родителя

            var rigidbody = _currentObject.GetComponent<Rigidbody>(); // берем Rigidbody у _currentObject
            rigidbody.isKinematic = false; // отключаем isKinematic у rigidbody нашего объекта (чтобы на него снова действовала обычная физика)

            if (!drop) // если нужно бросить объект
            {
                rigidbody.AddForce(transform.forward * _throwForce, ForceMode.Impulse); // бросаем наш объект вперед с силой равной _throwForce
            }

            _currentObject = null; // обнуляем текущий объект, подобранный игроком
        }
    }

    private void Shoot()
    {
        Debug.Log("Shoot");
    }

    private void DropWeapon()
    {
        Debug.Log("Вы выбросили свое оружие");
    }

    private void Boat()
    {
        Debug.Log("Лодка поплыла");
    }
}
