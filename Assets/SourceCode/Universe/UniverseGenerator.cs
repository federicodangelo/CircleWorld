using System;

namespace Universe
{
    public class UniverseGenerator
    {
        protected Thing[] things;
        protected Random random;
        protected ushort thingsAmount;
        protected ushort currentThing;

        public ushort Generate(int seed, Thing[] things)
        {
            this.things = things;
            random = new Random(seed);

            things[0].type = (ushort)ThingType.Galaxy;
            things[0].safeRadius = ushort.MaxValue;

            currentThing = 0;
            thingsAmount = 1;

            AddGalaxy();

            UpdateBrothers(0);

            this.things = null;
            this.random = null;

            return thingsAmount;
        }

        protected virtual void AddGalaxy()
        {

        }

        private ushort UpdateBrothers(ushort index)
        {
            int childs = things[index].childs;
            index++;

            ushort prevBrother = 0;

            for (int i = 0; i < childs; i++)
            {
                prevBrother = index;

                index = UpdateBrothers(index);

                if (i + 1 < childs)
                    things[prevBrother].nextBrother = index;
            }

            return index;
        }

        protected void PushThing(ThingType type, ushort angle, ushort distance, short rotationPeriod, short orbitalPeriod, ushort radius, ushort safeRadius, int seed)
        {
            things[currentThing].childs++;

            things[thingsAmount].parent = currentThing;
            things[thingsAmount].type = (ushort)type;

            things[thingsAmount].angle = angle;
            things[thingsAmount].distance = distance;
            things[thingsAmount].rotationPeriod = rotationPeriod;
            things[thingsAmount].orbitalPeriod = orbitalPeriod;
            things[thingsAmount].radius = radius;
            things[thingsAmount].safeRadius = safeRadius;
            things[thingsAmount].seed = seed;

            currentThing = thingsAmount;
            thingsAmount++;
        }

        protected void PopThing()
        {
            currentThing = things[currentThing].parent;
        }
    }
}




