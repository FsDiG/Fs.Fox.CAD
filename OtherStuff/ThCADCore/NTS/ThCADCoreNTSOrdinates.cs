using System;
using System.Linq;
using NetTopologySuite.Geometries;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSOrdinates
    {
        /** The current coordinate */
        private int curr;


        /** The ordinates holder */
        private double[] ordinates; 


        public ThCADCoreNTSOrdinates()
        {
            ordinates = new double[10];
            curr = -1;
        }

        public ThCADCoreNTSOrdinates(int capacity)
        {
            ordinates = new double[capacity];
            curr = -1;
        }

        public CoordinateSequence toCoordinateSequence(CoordinateSequenceFactory csfac)
        {
            CoordinateSequence cs = csfac.Create(size(), 3);
            for (int i = 0; i <= curr; i++)
            {
                cs.SetOrdinate(i, 0, ordinates[i * 2]);
                cs.SetOrdinate(i, 1, ordinates[i * 2 + 1]);
            }

            return cs;
        }

        /** The number of coordinates */
        public int size()
        {
            return curr + 1;
        }

        /** Adds a coordinate to this list */
        public void add(double x, double y)
        {
            curr++;
            if ((curr * 2 + 1) >= ordinates.Count())
            {
                int newSize = ordinates.Count() * 3 / 2;
                if (newSize < 10)
                {
                    newSize = 10;
                }
                double[] resized = new double[newSize];
                Array.Copy(ordinates, 0, resized, 0, ordinates.Count());
                ordinates = resized;
            }
            ordinates[curr * 2] = x;
            ordinates[curr * 2 + 1] = y;
        }

        /** Resets the ordinates */
        public void clear()
        {
            curr = -1;
        }

        public double getOrdinate(int coordinate, int ordinate)
        {
            return ordinates[coordinate * 2 + ordinate];
        }

        public void init(CoordinateSequence cs)
        {
            clear();
            for (int i = 0; i < cs.Count; i++)
            {
                add(cs.GetOrdinate(i, 0), cs.GetOrdinate(i, 1));
            }
        }
    }
}
