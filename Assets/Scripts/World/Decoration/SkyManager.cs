using System;
using System.Collections.Generic;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public List<DimensionSky> skyForDimension = new List<DimensionSky>();

    private void Update()
    {
        //Every second, update active sky object
        if (Time.time % 1f - Time.deltaTime <= 0)
        {
            Dimension dim = Dimension.Overworld;
            if (Player.LocalEntity != null)
                dim = Player.LocalEntity.Location.dimension;

            foreach (DimensionSky sky in skyForDimension)
                sky.skyObject.SetActive(sky.dimension == dim);
        }

        Vector3 cameraPosition = CameraController.instance.transform.position;
        transform.position = new Vector3(cameraPosition.x, transform.position.y);
    }
}

[Serializable]
public struct DimensionSky
{
    public Dimension dimension;
    public GameObject skyObject;
}