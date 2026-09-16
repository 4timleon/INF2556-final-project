using UnityEngine;

public static class GameControls
{
    private static InputSystem_Actions inputActions;

    public static InputSystem_Actions GetInputActions()
    {
        if (inputActions == null)
        {
            inputActions = GeneralInputHandler.Instance.GetInputActions();
        }

        return inputActions;
    }
}