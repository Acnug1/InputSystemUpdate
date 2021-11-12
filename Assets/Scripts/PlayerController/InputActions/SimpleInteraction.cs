using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // подключаем InputSystem

public class SimpleInteraction : IInputInteraction // для написания собственных Interaction необходимо реализовать интерфейс IInputInteraction
{
    public float Duration = 0.2f; // public необходим, чтобы значение отображалось в окне InputActions
    
    [UnityEditor.InitializeOnLoadMethod] // делаем так, чтобы каждый раз при загрузке у нас запускался статический метод Register()
    private static void Register() // делаем специальный статический метод, чтобы прописать наш Interaction
    {
        InputSystem.RegisterInteraction<SimpleInteraction>(); // регистрируем наш SimpleInteraction в InputAction
    }

    public void Process(ref InputInteractionContext context) // здесь происходит обработка (контекст, который мы опишем, будет доступен в окне InputActions. Мы передадим туда ссылку на контекст)
    {
        if (context.timerHasExpired) // если таймер истек (проверяем после нажатия на кнопку "вправо" игроком)
        {
            context.Canceled(); // то сбрасываем его и наше состояние (с помощью Reset)
            return; // выходим из метода (начинаем отслеживать состояние нажатия на кнопку заново)
        }    

        switch (context.phase) // мы хотим сравнивать с фазами нашего контекста
        {
            case InputActionPhase.Waiting: // пока ввод ожидается (считывание ввода)
                if (context.ReadValue<float>() == 1) // читаем из контекста значения от -1 до 1, которые нам приходят (если игрок нажал на кнопку "вправо", которая равна значению 1)
                {
                    context.Started(); // запускаем наше состояние (переходим в новое состояние, начинаем выполнение)
                    context.SetTimeout(Duration); // зададим таймер и укажем нашу длительность в качестве параметра
                }    
                break;
            case InputActionPhase.Started: // когда ввод уже считан и мы ждем следующих действий
                if (context.ReadValue<float>() == -1) // если игрок успел нажать кнопку "влево", пока не истек таймер
                {
                    context.Performed(); // контекст выполнен (действие совершено)
                }
                break;
        }
    }

    public void Reset() // всегда остается пустым по большей части, он нужен лишь для того, чтобы сбросить состояние (после окончания выполения или отмены)
    {

    }
}
