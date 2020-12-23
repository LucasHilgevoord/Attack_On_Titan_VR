using UnityEngine;

public static class TimeFunctions
{
    /// <summary>
    /// Use this instead of rb.velocity for full compatability with TimeController.
    /// TimeController will still be an optional component.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="rb"></param>
    /// <returns></returns>
    public static Vector3 GetRealVelocity(TimeController controller, Rigidbody rb)
    {
        //Debug.Log(rb.velocity);
        //Debug.Log(controller.GetRealVelocity());
        if (controller == null)
            return rb.velocity;
        else
            return rb.velocity / controller.TimeSpeed;
    }

    public static void SetRealVelocity(Vector3 velocity, TimeController controller, Rigidbody rb)
    {
        if (controller == null)
            rb.velocity = velocity;
        else
            rb.velocity = velocity * controller.TimeSpeed;
    }

    public static void AddRealVelocity(Vector3 velocity, TimeController controller, Rigidbody rb)
    {
        TimeFunctions.SetRealVelocity(TimeFunctions.GetRealVelocity(controller, rb) + velocity, controller, rb);
    }

    public static float DeltaTime(TimeController controller)
    {
        if (controller == null)
            return Time.deltaTime;
        else
            return Time.deltaTime * controller.TimeSpeed;
    }
}
