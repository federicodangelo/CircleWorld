//#define USE_SIN_TABLE

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
    public class Universe
    {
        public const int MAX_THINGS = 8192;

        private const float HALF_PI = Mathf.PI * 0.5f; //90 degress in radians
        private const float TWO_PI = Mathf.PI * 2.0f; //360 degress in radians
        private const float INV_TWO_PI = 1.0f / (Mathf.PI * 2.0f); 
        private const float DEG_TO_RAD_OVER_100 = 0.000174532925f; //(degrees to radians / 100)

        private Thing[] things = new Thing[MAX_THINGS];
        private ThingPosition[] thingsPositions = new ThingPosition[MAX_THINGS];
        private ushort thingsAmount;

        private ushort[] thingsToRender = new ushort[MAX_THINGS];
        private ushort thingsToRenderAmount;
        
        private ushort startingPlanet;
        
        private List<Planet> planets = new List<Planet>();
        
        private List<UniverseObject> tilemapObjects = new List<UniverseObject>();
        
        private UniverseFactory universeFactory = new UniverseFactory();
        
        private float time;
        
        private Avatar avatar;
        
        private IUniverseListener listener;
        
        public ushort StartingPlanet
        {
            get { return startingPlanet; }
        }
        
        public Avatar Avatar
        {
            get { return avatar; }
        }
        
        public Thing[] Things
        {
            get { return things; }
        }

        public ThingPosition[] ThingsPositions
        {
            get { return thingsPositions; }
        }
        
        public ushort[] ThingsToRender
        {
            get { return thingsToRender; }
        }
        
        public ushort ThingsToRenderAmount
        {
            get { return thingsToRenderAmount; }
        }
        
        public IUniverseListener Listener
        {
            get { return listener; }
            set { listener = value; }
        }
        
        public void Init(int seed, IUniverseListener listener)
        {
            this.listener = listener;
            
            time = 0.0f;
            
            thingsAmount = new UniverseGeneratorDefault().Generate(seed, things);

            UpdateThingsToRender();
            
            startingPlanet = thingsToRender[1];
            
            UpdateUniverse(0);
            
            AddAvatar();
        }

        private void UpdateThingsToRender()
        {
            thingsToRenderAmount = 0;
            for (int i = 0; i < thingsAmount; i++)
            {
                ThingType type = (ThingType)things[i].type;

                if (type == ThingType.Sun || type == ThingType.Planet || type == ThingType.Moon)
                    thingsToRender[thingsToRenderAmount++] = (ushort) i;
            }
        }

        public void UpdateUniverse(float deltaTime)
        {
#if USE_SIN_TABLE
            if (sinTable == null)
                InitSinTable();
#endif

            time += deltaTime;

            Profiler.BeginSample("Universe.UpdatePositions");
            UpdatePositions(time);
            Profiler.EndSample();

            for (int i = 0; i < planets.Count; i++)
                planets[i].Update(deltaTime);
            
            for (int i = 0; i < tilemapObjects.Count; i++)
                tilemapObjects[i].Update(deltaTime);
        }

#if USE_SIN_TABLE
        private const int SIN_TABLE_LEN = 512;
        private const float SIN_TABLE_LEN_F = SIN_TABLE_LEN;
        private const float SIN_TABLE_INDEX = INV_TWO_PI * SIN_TABLE_LEN_F;

        static private float[] sinTable;
        static private float[] cosTable;

        static private void InitSinTable()
        {
            sinTable = new float[SIN_TABLE_LEN];
            cosTable = new float[SIN_TABLE_LEN];

            for (int i = 0; i < sinTable.Length; i++)
            {
                sinTable[i] = Mathf.Sin((i * TWO_PI) / SIN_TABLE_LEN_F);
                cosTable[i] = Mathf.Cos((i * TWO_PI) / SIN_TABLE_LEN_F);

                //Debug.Log(sinTable[i]);
            }
        }

        static private float Sin(float angle)
        {
            return sinTable[(int)(angle * SIN_TABLE_INDEX) % SIN_TABLE_LEN];
        }

        static private float Cos(float angle)
        {
            //return cosTable[(int)(angle * SIN_TABLE_INDEX) % SIN_TABLE_LEN];

            return sinTable[(int)((angle + HALF_PI) * SIN_TABLE_INDEX) % SIN_TABLE_LEN];
        }
#endif

        private void UpdatePositions(float time)
        {
            for (int index = 1; index < thingsAmount; index++)
            {
                Thing thing = things[index];

                float parentX = thingsPositions[thing.parent].x;
                float parentY = thingsPositions[thing.parent].y;

                float angle = thing.angle * DEG_TO_RAD_OVER_100;
                float distance = thing.distance;

                float normalizedOrbitalPeriod = time * thing.orbitalPeriodInv;

                //if (thing.orbitalPeriod != 0)
                //    normalizedOrbitalPeriod = time / thing.orbitalPeriod;
                //else
                //    normalizedOrbitalPeriod = 0;

                normalizedOrbitalPeriod -= (int)normalizedOrbitalPeriod;

                float normalizedRotationPeriod = time * thing.rotationPeriodInv;

                //if (thing.rotationPeriod != 0)
                //    normalizedRotationPeriod = time / thing.rotationPeriod;
                //else
                //    normalizedRotationPeriod = 0;

                normalizedRotationPeriod -= (int)normalizedRotationPeriod;

                angle += TWO_PI * normalizedOrbitalPeriod; //360 degrees to radians

#if USE_SIN_TABLE
                if (angle < 0)
                    angle += TWO_PI;

                thingsPositions[index].x = parentX + Cos(angle) * distance;
                thingsPositions[index].y = parentY + Sin(angle) * distance;
#else
                thingsPositions[index].x = parentX + ((float)Math.Cos(angle)) * distance;
                thingsPositions[index].y = parentY + ((float)Math.Sin(angle)) * distance;
#endif
                thingsPositions[index].rotation = normalizedRotationPeriod * TWO_PI; //360 degrees to radian
                thingsPositions[index].radius = thing.radius;
            }
        }

        public Thing GetThing(ushort thingIndex)
        {
            return things[thingIndex];
        }
        
        public ThingPosition GetThingPosition(ushort thingIndex)
        {
            return thingsPositions[thingIndex];
        }
        
        public Planet GetPlanet(ushort thingIndex)
        {
            for (int i = 0; i < planets.Count; i++)
                if (planets[i].ThingIndex == thingIndex)
                    return planets[i];
            
            Planet planet = universeFactory.GetPlanet(Planet.GetPlanetHeightWithRadius(things[thingIndex].radius));
            
            planet.InitPlanet(this, thingIndex);
            
            planets.Add(planet);
            
            return planet;
        }
        
        public void ReturnPlanet(Planet planet)
        {
            if (planets.Remove(planet))
            {
                if (listener != null)
                    listener.OnPlanetReturned(planet);
                
                universeFactory.ReturnPlanet(planet);
            }
        }
        
        private void AddAvatar()
        {
            Planet planet = GetPlanet(startingPlanet);
            
            avatar = universeFactory.GetAvatar();
            avatar.Init(
                new Vector2(0.75f, 1.05f),
                planet,
                planet.GetPositionFromTileCoordinate(0, planet.Height)
            );
            
            AddUniverseObject(avatar);
        }
        
        public void AddUniverseObject(UniverseObject universeObject)
        {
            tilemapObjects.Add(universeObject);
            
            if (listener != null)
                listener.OnUniverseObjectAdded(universeObject);
        }
    }
}


