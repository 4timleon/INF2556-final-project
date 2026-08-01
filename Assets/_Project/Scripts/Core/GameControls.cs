using UnityEngine;

public static class GameControls
{
    private static InputSystem_Actions inputActions;

    public static InputSystem_Actions GetInputActions()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();
        }

        return inputActions;
    }
}