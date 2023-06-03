// https://github.com/begedin/UniqueRNG/blob/master/URNG/LinearCongruentialGenerator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace URNG
{
    /// <summary>
    /// A linear congruence generator using a large prime number close to, but smaller than 2^32. 
    /// A single instance returns pseudo random unique numbers up to a maximum sequence length of 4294967291.
    /// Once the maximum length is reached, the exact same sequence repeats.
    /// </summary>
    public class LinearCongruentialGenerator
    {
        #region Private fields and constants

        // used to generate initial value - seed
        private Random random;

        // fixed parameters for default constructor
        const uint M = 4294967291;  // large prime modulus - period of the generator, maxUInt32 - 5
        const uint A = M + 1;       // multiplier

        // current state and parameters
        private uint x, m, a, c, range = 0;

        private int min = 0;

        //stored state and parameters in case sequence is used;
        private uint old_x, old_m, old_a, old_c;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new linear congruential generator with parameters set to generate numbers within the unsigned int range.
        /// </summary>
        public LinearCongruentialGenerator(int seed)
        {
            init(seed);
        }

        /// <summary>
        /// Creates a new linear congruential generator with parameters set to generate numbers from the specified range.
        /// 
        /// Generated numbers will be within range [0, maxValue>
        /// </summary>
        /// <param name="maxValue"></param>
        public LinearCongruentialGenerator(int seed, uint maxValue)
        {
            init(seed, maxValue);
        }

        /// <summary>
        /// Creates a new linear congruential generator with parameters set to generate numbers from the specified range
        /// 
        /// Generated numbers will be within range [minValue, maxValue>
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public LinearCongruentialGenerator(int seed, int minValue, int maxValue)
        {
            this.min = minValue;
            init(seed, (uint)(maxValue - minValue));
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the generator with values set so it generates unsigned integers from the entire unsigned integer range.
        /// </summary>
        private void init(int seed)
        {
            this.random = new Random(seed);
            this.m = M;
            this.x = (uint)random.Next();
            this.a = m + 1;
            this.c = (uint)random.Next();
        }

        /// <summary>
        /// Initializes the generator with values set so it generates unsigned integers in a specific, desired range.
        /// </summary>
        /// <param name="desiredRange"></param>
        private void init(int seed, uint desiredRange)
        {
            this.random = new Random(seed);
            this.m = findPrime(desiredRange);
            this.x = (uint)random.Next(1, (int)desiredRange);
            this.a = m + 1;
            this.c = (uint)random.Next(1, (int)desiredRange);
            this.range = desiredRange;

            //System.Diagnostics.Debug.WriteLine("x: " + x.ToString() + ", m: " + m.ToString() + ", a: " + a.ToString() + ", c: " + c.ToString());
        }

        /// <summary>
        /// Finds and returns nearest prime larger than specified number.
        /// </summary>
        private uint findPrime(uint number)
        {
            long prime;

            // bool isPrime = true;
            for (long i = number; ; i++)
            {
                bool isPrime = true; // Move initialization to here
                for (long j = 2; j < Math.Sqrt(i); j++) // you actually only need to check up to sqrt(i)
                {
                    if (i % j == 0) // you don't need the first condition
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    prime = i;
                    break;
                }
            }

            return (uint)prime;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// generates a random number on [0, 4294967291] interval
        /// </summary>
        /// <returns></returns>
        protected uint generateRandomInt32()
        {
            // compute and return next generator value
            while (true)
            {
                // this loop runs more than once only for numbers range < x < m, which is rarely
                x = (uint)((a * (ulong)x + c) % m);
                if ((range == 0) || x < range) return x;
            }
        }

        /// <summary>
        /// Generates a random number on [0,1]-real-interval
        /// </summary>
        /// <returns>Random number on [0,1]-real-interval (includes 0, includes 1)</returns>
        private double generateRandomRealIncluding1()
        {
            return generateRandomInt32() * (1.0 / 4294967295.0);
            // divided by 2^32-1
        }

        /// <summary>
        /// Generates a random number on [0,1)-real-interval
        /// </summary>
        /// <returns>Random number on [0,1)-real-interval (includes 0, excludes 1)</returns>
        private double generateRandomRealExcluding1()
        {
            return generateRandomInt32() * (1.0 / 4294967296.0);
            // divided by 2^32
        }

        /// <summary>
        /// Generates a random number on (0,1)-real-interval
        /// </summary>
        /// <returns>Random number on (0,1)-real-interval (excludes 0, excludes 1)</returns>
        private double generateRandomRealExcluding01()
        {
            return (((double)generateRandomInt32()) + 0.5) * (1.0 / 4294967296.0);
            // divided by 2^32
        }

        /// <summary>
        /// Stores the current state of the generator
        /// </summary>
        private void storeState()
        {
            old_x = x;
            old_m = m;
            old_a = a;
            old_c = c;
        }

        /// <summary>
        /// Restores the previous state of the generator
        /// </summary>
        private void restoreState()
        {
            x = old_x;
            m = old_m;
            a = old_a;
            c = old_c;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns a random unsigned integer
        /// 
        /// If default constructor was used, the result will be from the interval [0, M>
        /// If range was specified in the constructor, the result will be from the interval [0, range>
        /// If min and max were specified in the constructor, the result will be from the interval [0, max - min>
        /// </summary>
        /// <returns>The next random integer.</returns>
        public uint Next()
        {
            return generateRandomInt32();
        }

        /// <summary>
        /// Returns a random signed integer
        /// 
        /// Used for when we use the generator to generate numbers within a min-max range, where min is less than 0
        /// 
        /// If default constructor was used, the result will be from the interval [minInt32 + 5, maxInt32>
        /// If range was specified in the constructor, the result will be from the interval [0, range>, unless range > maxInt32, in which case it will overflow so usage is not recommended.
        /// If min and max were specified in the constructor, the result will be from the interval [min, max>
        /// </summary>
        /// <returns></returns>
        public int NextSigned()
        {
            return unchecked((int)generateRandomInt32()) + min;
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// 
        /// Intended for use in instances where default constructor was used. There could be unexpected behavior in other cases
        /// </summary>
        /// <returns>A double-precision floating point number greater than or equal to 0.0, 
        /// and less than 1.0.</returns>
        public double NextDouble()
        {
            return generateRandomRealExcluding1();
        }

        /// <summary>
        /// Returns a random number greater than or equal to zero, and either strictly
        /// less than one, or less than or equal to one, depending on the value of the
        /// given boolean parameter.
        /// 
        /// Intended for use in instances where default constructor was used. There could be unexpected behavior in other cases
        /// </summary>
        /// <param name="includeOne">
        /// If <c>true</c>, the random number returned will be 
        /// less than or equal to one; otherwise, the random number returned will
        /// be strictly less than one.
        /// </param>
        /// <returns>
        /// If <c>includeOne</c> is <c>true</c>, this method returns a
        /// single-precision random number greater than or equal to zero, and less
        /// than or equal to one. If <c>includeOne</c> is <c>false</c>, this method
        /// returns a single-precision random number greater than or equal to zero and
        /// strictly less than one.
        /// </returns>
        public double NextDouble(bool includeOne)
        {
            if (includeOne)
            {
                return generateRandomRealIncluding1();
            }
            return NextDouble();
        }

        /// <summary>
        /// Returns a random number greater than 0.0 and less than 1.0.
        ///         
        /// Intended for use in instances where default constructor was used. There could be unexpected behavior in other cases
        /// </summary>
        /// <returns>A random number greater than 0.0 and less than 1.0.</returns>
        public double NextDoublePositive()
        {
            return generateRandomRealExcluding01();
        }

        #endregion
    }
}