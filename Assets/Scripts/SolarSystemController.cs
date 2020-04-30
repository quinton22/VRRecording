using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemController : MonoBehaviour
{
    //==========================================================================
    [System.Serializable]
    private class Planet
    {
        public string m_Name;
        
        [Tooltip("Choose a prefab or a material")]
        public GameObject prefab;
        
        [Tooltip("Planet material")]
        public Material m_Material;

        [Tooltip("Time (sec) it takes planet to orbit center")]
        public float m_OrbitTime;

        [Tooltip("Time (sec) it takes planet to spin in a full circle")]
        public float m_SpinTime;

        [Tooltip("Scale of planet")]
        public float m_PlanetScale = 1;

        [Tooltip("Only applicable if UniformDistancesFromCenter is false")]
        public float m_DistanceFromCenter;

        [System.NonSerialized]
        public PlanetHolder m_PlanetHolder;

        [System.NonSerialized]
        public bool m_ShowBaseArms = false;

        [System.NonSerialized]
        public bool isCenterPlanet = false;

        public void InitializePlanet(GameObject basePlanet, Transform parent, float height)
        {
            prefab = Instantiate(prefab == null ? basePlanet : prefab, parent);
            prefab.transform.localPosition = new Vector3(m_DistanceFromCenter, height, 0); // TODO
            prefab.transform.localScale = new Vector3(1, 1, 1) * m_PlanetScale;

            if (m_Material != null)
            {
                prefab.GetComponent<Renderer>().material = m_Material;
            }
        }

        public void InitializeArm(GameObject baseArm, Transform parent, float vertPos, float vertScale=1)
        {
            if (isCenterPlanet) return;
            SetArmObject(Instantiate(baseArm, parent), vertPos, vertScale);
        }

        public void SetArmObject(GameObject obj, float vertPos, float vertScale)
        {
            m_PlanetHolder = new PlanetHolder(obj, vertPos, vertScale, m_DistanceFromCenter);
        }

        public void RotatePlanet(float angle)
        {
            if (isCenterPlanet) return;

            float planetY = prefab.transform.localPosition.y;
            Vector3 rotationPoint = prefab.transform.parent.TransformPoint(0, planetY, 0);
            prefab.transform.RotateAround(rotationPoint, prefab.transform.parent.up, angle);

            if (m_ShowBaseArms)
                m_PlanetHolder.RotateArm(angle);
        }

        public void SpinPlanet(float angle, Vector3 axis)
        {
            prefab.transform.Rotate(axis, angle);
        }

        public void SpinPlanet(float angle)
        {
            SpinPlanet(angle, prefab.transform.up);
        }
    }

    //==========================================================================
    private struct PlanetHolder
    {
        public GameObject arm;
        public GameObject horizontalArm;
        public GameObject verticalArm;
        public GameObject elbow;

        public PlanetHolder(GameObject _arm, float vertPos, float vertScale = 1, float horzScale = 1)
        {
            arm = _arm;
            horizontalArm = arm.transform.Find("HorizontalArm").gameObject;
            verticalArm = arm.transform.Find("VerticalArm").gameObject;
            elbow = arm.transform.Find("Elbow").gameObject;

            SetScaleAndPos(vertPos, vertScale, horzScale);
        }

        private void SetScaleAndPos(float vertPos, float vertScale = 1, float horzScale = 1)
        {
            SetVerticalPosition(vertPos);
            SetVerticalScale(vertScale);
            SetHorizontalScale(horzScale);
        }

        public void RotateArm(float angle)
        {
            arm.transform.RotateAround(arm.transform.position, arm.transform.parent.up, angle);
        }

        private void SetVerticalPosition(float pos)
        {
            Vector3 armPos = arm.transform.localPosition;
            armPos.y = pos;
            arm.transform.localPosition = armPos;
        }

        private void SetVerticalScale(float scale)
        {
            Vector3 vertScale = verticalArm.transform.localScale;
            vertScale.y = scale;
            verticalArm.transform.localScale = vertScale;
        }

        private void SetHorizontalScale(float scale)
        {
            Vector3 horzScale = horizontalArm.transform.localScale;
            horzScale.x = scale;
            horizontalArm.transform.localScale = horzScale;

            Vector3 elbowPos = elbow.transform.localPosition;
            elbowPos.x = scale;
            elbow.transform.localPosition = elbowPos;

            Vector3 vertPos = verticalArm.transform.localPosition;
            vertPos.x = scale;
            verticalArm.transform.localPosition = vertPos;
        }
    }

    //==========================================================================
    [SerializeField]
    private bool m_RandomizeStartPositions = false;

    [SerializeField]
    [Tooltip("Height of the base stand")]
    private float m_BaseStandHeight = 3;

    [SerializeField]
    [Range(0, 3153600)]
    [Tooltip("How many times faster than realtime")]
    private float m_TimeScale = 1;

    [SerializeField]
    private List<Planet> m_Planets;

    [SerializeField]
    [Tooltip("Should the planets be evenly spaced from center")]
    private bool m_UniformDistancesFromCenter = true;

    [SerializeField]
    [Tooltip("Only applicable if UniformDistancesFromCenter is true")]
    private float m_PlanetSpacing = 2;

    [SerializeField]
    [Tooltip("Should the arms be shown")]
    private bool m_ShowBaseArms = true;


    //==========================================================================
    void Awake()
    {
        Transform baseStand = transform.Find("BaseStand");
        Vector3 baseStandScale = baseStand.localScale;
        baseStandScale.y = m_BaseStandHeight;
        baseStand.localScale = baseStandScale;

        // set up each planet game objects
        InitializePlanets();
        
    }

    //--------------------------------------------------------------------------
    void Update()
    {
        foreach (Planet planet in m_Planets)
        {
            // rotate planets (revolve)
            planet.RotatePlanet(360 / planet.m_OrbitTime * Time.deltaTime);

            // spin planets
            planet.SpinPlanet(360 / planet.m_SpinTime * Time.deltaTime);
        }
    }

    //--------------------------------------------------------------------------
    void InitializePlanets()
    {
        GameObject planetGO = transform.Find("Planet").gameObject;
        GameObject armGO = transform.Find("PlanetHolder").gameObject;
        Transform baseStand = transform.Find("BaseStand");

        float baseHeight = 0.4f; //* transform.localScale.y;

        float totalVerticalSpace = baseStand.localScale.y;
        float armSpacing = totalVerticalSpace / m_Planets.Count;
      
        // set the distances of each planet
        for (int i = 0; i < m_Planets.Count; ++i)
        {
            m_Planets[i].isCenterPlanet = i == 0;

            if (m_UniformDistancesFromCenter)
            {
                m_Planets[i].m_DistanceFromCenter = m_PlanetSpacing * i;
            }

            // Create game objects
            m_Planets[i].InitializePlanet(planetGO, transform, baseHeight + totalVerticalSpace);


            if (m_ShowBaseArms)
            {
                m_Planets[i].m_ShowBaseArms = m_ShowBaseArms;

                // create arms
                m_Planets[i].InitializeArm(armGO, transform, baseHeight + totalVerticalSpace - i * armSpacing - .6f, i * armSpacing + .6f);

            }

            if (m_RandomizeStartPositions)
            {
                m_Planets[i].RotatePlanet(Random.Range(0, 360)); // generate random planet pos
                m_Planets[i].SpinPlanet(Random.Range(0, 360)); // generate random planet spin
            }
        }

        planetGO.SetActive(false); // hide original planet game object
        armGO.gameObject.SetActive(false);

        // TODO
        // set materials
    
    }
}
