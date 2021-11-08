using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IATClient
{
    class BigInteger
    {
        /**
        * The signum of this BigInteger: -1 for negative, 0 for zero, or
        * 1 for positive. Note that the BigInteger zero <i>must</i> have
        * a signum of 0. This is necessary to ensures that there is exactly one
        * representation for each BigInteger value. 
        *
        * @serial
        */
        private int signum;
   
       /**
        * The magnitude of this BigInteger, in <i>big-endian</i> order: the
        * zeroth element of this array is the most-significant int of the
        * magnitude. The magnitude must be "minimal" in that the most-significant
        * int (<tt>mag[0]</tt>) must be non-zero. This is necessary to
        * ensure that there is exactly one representation for each BigInteger
        * value. Note that this implies that the BigInteger zero has a
        * zero-length mag array.
        */
        private int[] mag;
 
        // These "redundant fields" are initialized with recognizable nonsense
        // values, and cached the first time they are needed (or never, if they
        // aren't needed).
 
        /**
         * The bitCount of this BigInteger, as returned by bitCount(), or -1
         * (either value is acceptable).
         *
         * @serial
         * @see #bitCount
         */
        private int bitCount = -1;
 
        /**
         * The bitLength of this BigInteger, as returned by bitLength(), or -1
         * (either value is acceptable).
         *
         * @serial
         * @see #bitLength
         */
        private int bitLength = -1;

        /**
         * The lowest set bit of this BigInteger, as returned by getLowestSetBit(),
         * or -2 (either value is acceptable).
         *
         * @serial
         * @see #getLowestSetBit
         */
        private int lowestSetBit = -2;
    
        /**
         * The index of the lowest-order byte in the magnitude of this BigInteger
         * that contains a nonzero byte, or -2 (either value is acceptable). The
         * least significant byte has int-number 0, the next byte in order of
         * increasing significance has byte-number 1, and so forth.
         *
         * @serial
         */
        private int firstNonzeroByteNum = -2;
   
        /**
         * The index of the lowest-order int in the magnitude of this BigInteger
         * that contains a nonzero int, or -2 (either value is acceptable). The
         * least significant int has int-number 0, the next int in order of
         * increasing significance has int-number 1, and so forth.
         */
        private int firstNonzeroIntNum = -2;
 
        /**
         * This mask is used to obtain the value of an int as if it were unsigned.
         */
        private const long LONG_MASK = 0xffffffffL;
 
        //Constructors
 
        /**
         * Translates a byte array containing the two's-complement binary
         * representation of a BigInteger into a BigInteger. The input array is
         * assumed to be in <i>big-endian</i> byte-order: the most significant
         * byte is in the zeroth element.
         *
         * @param val big-endian two's-complement binary representation of
         * BigInteger.
         * @throws NumberFormatException <tt>val</tt> is zero bytes long.
         */
        public BigInteger(byte[] val) {
        if (val.length == 0)
            throw new Exception("Zero length BigInteger");
 
        if (val[0] < 0) {
                mag = makePositive(val);
            signum = -1;
        } else {
            mag = stripLeadingZeroBytes(val);
            signum = (mag.length == 0 ? 0 : 1);
        }
        }
    
        /**
         * This private constructor translates an int array containing the
         * two's-complement binary representation of a BigInteger into a
         * BigInteger. The input array is assumed to be in <i>big-endian</i>
         * int-order: the most significant int is in the zeroth element.
         */
        private BigInteger(int[] val) {
        if (val.length == 0)
            throw new NumberFormatException  ("Zero length BigInteger");
    
        if (val[0] < 0) {
                mag = makePositive(val);
            signum = -1;
        } else {
            mag = trustedStripLeadingZeroInts(val);
            signum = (mag.length == 0 ? 0 : 1);
        }
        }
 
        /**
         * Translates the sign-magnitude representation of a BigInteger into a
         * BigInteger. The sign is represented as an integer signum value: -1 for
         * negative, 0 for zero, or 1 for positive. The magnitude is a byte array
         * in <i>big-endian</i> byte-order: the most significant byte is in the
         * zeroth element. A zero-length magnitude array is permissible, and will
         * result inin a BigInteger value of 0, whether signum is -1, 0 or 1.
         *
         * @param signum signum of the number (-1 for negative, 0 for zero, 1
         * for positive).
         * @param magnitude big-endian binary representation of the magnitude of
         * the number.
         * @throws NumberFormatException <tt>signum</tt> is not one of the three
         * legal values (-1, 0, and 1), or <tt>signum</tt> is 0 and
         * <tt>magnitude</tt> contains one or more non-zero bytes.
         */
        public BigInteger(int signum, byte[] magnitude) {
        this.mag = stripLeadingZeroBytes(magnitude);
    
        if (signum < -1 || signum > 1)
            throw(new Exception("Invalid signum value"));
 
        if (this.mag.length==0) {
            this.signum = 0;
        } else {
            if (signum == 0)
            throw(new Exception("signum-magnitude mismatch"));
            this.signum = signum;
        }
        }
 
        /**
         * A constructor for internal use that translates the sign-magnitude
         * representation of a BigInteger into a BigInteger. It checks the
         * arguments and copies the magnitude so this constructor would be
         * safe for external use.
         */
        private BigInteger(int signum, int[] magnitude) {
        this.mag = stripLeadingZeroInts(magnitude);
    
        if (signum < -1 || signum > 1)
            throw(new Exception("Invalid signum value"));
 
        if (this.mag.length==0) {
            this.signum = 0;
        } else {
            if (signum == 0)
            throw(new Exception("signum-magnitude mismatch"));
            this.signum = signum;
        }
        }
 
        /**
         * Translates the String representation of a BigInteger in the specified
         * radix into a BigInteger. The String representation consists of an
         * optional minus sign followed by a sequence of one or more digits in the
         * specified radix. The character-to-digit mapping is provided by
         * <tt>Character.digit</tt>. The String may not contain any extraneous
         * characters (whitespace, for example).
         *
         * @param val String representation of BigInteger.
         * @param radix radix to be used in interpreting <tt>val</tt>.
         * @throws NumberFormatException <tt>val</tt> is not a valid representation
         * of a BigInteger in the specified radix, or <tt>radix</tt> is
         * outside the range from {@link Character#MIN_RADIX} to
         * {@link Character#MAX_RADIX}, inclusive.
         * @see Character#digit
         */
        public BigInteger(String   val, int radix) {
        int cursor = 0, numDigits;
            int len = val.length();
    
        if (radix < Character.MIN_RADIX || radix > Character.MAX_RADIX)
            throw new NumberFormatException  ("Radix out of range");
        if (val.length() == 0)
           throw new NumberFormatException  ("Zero length BigInteger");
 
        // Check for minus sign
            signum = 1;
            int index = val.lastIndexOf("-");
            if (index != -1) {
                if (index == 0) {
                    if (val.length() == 1)
                        throw new Exception("Zero length BigInteger");
                    signum = -1;
                    cursor = 1;
                } else {
                    throw new NumberFormatException  ("Illegal embedded minus sign");
                }
            }
 
            // Skip leading zeros and compute number of digits in magnitude
            while ((cursor < len) && (Character.digit(val.charAt(cursor),radix) == 0))
                cursor++;
            if (cursor == len) {
                signum = 0;
                mag = ZERO.mag;
                return;
            } else {
                numDigits = len - cursor;
            }
    
            // Pre-allocate array of expected size. May be too large but can
            // never be too small. Typically exact.
            int numBits = (int)(((numDigits * bitsPerDigit[radix]) >>> 10) + 1);
            int numWords = (numBits + 31) /32;
            mag = new int[numWords];

            // Process first (potentially short) digit group
            int firstGroupLen = numDigits % digitsPerInt[radix];
            if (firstGroupLen == 0)
                firstGroupLen = digitsPerInt[radix];
            String group = val.substring(cursor, cursor += firstGroupLen);
            mag[mag.length - 1] = Integer.parseInt(group, radix);
            if (mag[mag.length - 1] < 0)
                throw new NumberFormatException  ("Illegal digit");

            // Process remaining digit groups
            int superRadix = intRadix[radix];
            int groupVal = 0;
            while (cursor < val.length()) {
                group = val.substring(cursor, cursor += digitsPerInt[radix]);
                groupVal = Integer.parseInt(group, radix);
                if (groupVal < 0)
                    throw new Exception("Illegal digit");
                destructiveMulAdd(mag, superRadix, groupVal);
            }
            
            // Required for cases where the array was overallocated.
            mag = trustedStripLeadingZeroInts(mag);
        }

            // Constructs a new BigInteger using a char array with radix=10
            BigInteger(char[] val) {
                int cursor = 0, numDigits;
                int len = val.length;

                // Check for leading minus sign
                signum = 1;
                if (val[0] == '-') {
                    if (len == 1)
                        throw new Exception("Zero length BigInteger");
                    signum = -1;
                    cursor = 1;
                }

                // Skip leading zeros and compute number of digits in magnitude
                while (cursor < len && Character.digit(val[cursor], 10) == 0)
                    cursor++;
                if (cursor == len) {
                    signum = 0;
                    mag = ZERO.mag;
                    return;
                } else {
                    numDigits = len - cursor;
                }

                // Pre-allocate array of expected size
                int numWords;
                if (len < 10) {
                    numWords = 1;
                } else { 
                    int numBits = (int)(((numDigits * bitsPerDigit[10]) >>> 10) + 1);
                    numWords = (numBits + 31) /32;
                }
                mag = new int[numWords];

                // Process first (potentially short) digit group
                int firstGroupLen = numDigits % digitsPerInt[10];
                if (firstGroupLen == 0)
                    firstGroupLen = digitsPerInt[10];
                mag[mag.length-1] = parseInt(val, cursor, cursor += firstGroupLen);

                // Process remaining digit groups
                while (cursor < len) {
                    int groupVal = parseInt(val, cursor, cursor += digitsPerInt[10]);
                    destructiveMulAdd(mag, intRadix[10], groupVal);
                }
                mag = trustedStripLeadingZeroInts(mag);
            }

            // Create an integer with the digits between the two indexes
            // Assumes start < end. The result may be negative, but it
            // is to be treated as an unsigned value.
            private int parseInt(char[] source, int start, int end) {
                int result = Character.digit(source[start++], 10);
                if (result == -1)
                    throw new Exception(new String(source));

                for (int index = start; index<end; index++) {
                    int nextVal = Character.digit(source[index], 10);
                    if (nextVal == -1)
                        throw new Exception(new String  (source));
                    result = 10*result + nextVal;
                }

                return result;
            }

            // bitsPerDigit in the given radix times 1024
            // Rounded up to avoid underallocation.
            private static long []bitsPerDigit = { 0, 0,
                1024, 1624, 2048, 2378, 2648, 2875, 3072, 3247, 3402, 3543, 3672,
                3790, 3899, 4001, 4096, 4186, 4271, 4350, 4426, 4498, 4567, 4633,
                4696, 4756, 4814, 4870, 4923, 4975, 5025, 5074, 5120, 5166, 5210,
                5253, 5295};

            // Multiply x array times word y in place, and add word z
            private static void destructiveMulAdd(int[] x, int y, int z) {
                // Perform the multiplication word by word
                long ylong = y & LONG_MASK;
                long zlong = z & LONG_MASK;
                int len = x.length;

                long product = 0;
                long carry = 0;
                for (int i = len-1; i >= 0; i--) {
                    product = ylong * (x[i] & LONG_MASK) + carry;
                    x[i] = (int)product;
                    carry = product >>> 32;
                }

            // Perform the addition
            long sum = (x[len-1] & LONG_MASK) + zlong;
            x[len-1] = (int)sum;
            carry = sum >>> 32;
            for (int i = len-2; i >= 0; i--) {
                sum = (x[i] & LONG_MASK) + carry;
                x[i] = (int)sum;
                carry = sum >>> 32;
            }
        }

        /**
         * Translates the decimal String representation of a BigInteger into a
         * BigInteger. The String representation consists of an optional minus
         * sign followed by a sequence of one or more decimal digits. The
         * character-to-digit mapping is provided by <tt>Character.digit</tt>.
         * The String may not contain any extraneous characters (whitespace, for
         * example).
         *
         * @param val decimal String representation of BigInteger.
         * @throws NumberFormatException <tt>val</tt> is not a valid representation
         * of a BigInteger.
         * @see Character#digit
         */
        public BigInteger(String val) {
            this(val, 10);
        }

       /**
        * Constructs a randomly generated BigInteger, uniformly distributed over
        * the range <tt>0</tt> to <tt>(2<sup>numBits</sup> - 1)</tt>, inclusive.
        * The uniformity of the distribution assumes that a fair source of random
        * bits is provided in <tt>rnd</tt>. Note that this constructor always
        * constructs a non-negative BigInteger.
        *
        * @param numBits maximum bitLength of the new BigInteger.
        * @param rnd source of randomness to be used in computing the new
        * BigInteger.
        * @throws IllegalArgumentException <tt>numBits</tt> is negative.
        * @see #bitLength
        */
        public BigInteger(int numBits, Random rnd) {
            this(1, randomBits(numBits, rnd));
        }

        private static byte[] randomBits(int numBits, Random   rnd) {
            if (numBits < 0)
                throw new Exception("numBits must be non-negative");
            int numBytes = (numBits+7)/8;
            byte[] randomBits = new byte[numBytes];

            // Generate random bytes and mask out any excess bits
            if (numBytes > 0) {
                rnd.nextBytes(randomBits);
                int excessBits = 8*numBytes - numBits;
                randomBits[0] &= (1 << (8-excessBits)) - 1;
            }
            return randomBits;
        }

       /**
        * Constructs a randomly generated positive BigInteger that is probably
        * prime, with the specified bitLength.<p>
        *
        * It is recommended that the {@link #probablePrime probablePrime}
        * method be used in preference to this constructor unless there
        * is a compelling need to specify a certainty.
        *
        * @param bitLength bitLength of the returned BigInteger.
        * @param certainty a measure of the uncertainty that the caller is
        * willing to tolerate. The probability that the new BigInteger
        * represents a prime number will exceed
        * <tt>(1 - 1/2<sup>certainty</sup></tt>). The execution time of
        * this constructor is proportional to the value of this parameter.
        * @param rnd source of random bits used to select candidates to be
        * tested for primality.
        * @throws ArithmeticException <tt>bitLength &lt; 2</tt>.
        * @see #bitLength
        */
        public BigInteger(int bitLength, int certainty, Random rnd) {
            BigInteger prime;

            if (bitLength < 2)
                throw new Exception("bitLength < 2");
            
            prime = (bitLength < 95 ? smallPrime(bitLength, certainty, rnd)
                        : largePrime(bitLength, certainty, rnd));
            signum = 1;
            mag = prime.mag;
        }

        // Minimum size in bits that the requested prime number has
        // before we use the large prime number generating algorithms
        private static final int SMALL_PRIME_THRESHOLD = 95;

        // Certainty required to meet the spec of probablePrime
        private static final int DEFAULT_PRIME_CERTAINTY = 100;

        /**
         * Returns a positive BigInteger that is probably prime, with the
         * specified bitLength. The probability that a BigInteger returned
         * by this method is composite does not exceed 2<sup>-100</sup>.
         *
         * @param bitLength bitLength of the returned BigInteger.
         * @param rnd source of random bits used to select candidates to be
         * tested for primality.
         * @return a BigInteger of <tt>bitLength</tt> bits that is probably prime
         * @throws ArithmeticException <tt>bitLength &lt; 2</tt>.
         * @see #bitLength
         */
        public static BigInteger   probablePrime(int bitLength, Random   rnd) 
        {
            if (bitLength < 2)
                throw new ArithmeticException  ("bitLength < 2");

            // The cutoff of 95 was chosen empirically for best performance
            return (bitLength < SMALL_PRIME_THRESHOLD ?
                smallPrime(bitLength, DEFAULT_PRIME_CERTAINTY, rnd) :
                largePrime(bitLength, DEFAULT_PRIME_CERTAINTY, rnd));
        }

        /**
         * Find a random number of the specified bitLength that is probably prime.
         * This method is used for smaller primes, its performance degrades on
         * larger bitlengths.
         *
         * This method assumes bitLength > 1.
         */
        private static BigInteger   smallPrime(int bitLength, int certainty, Random   rnd) {
            int magLen = (bitLength + 31) >>> 5;
            int temp[] = new int[magLen];
            int highBit = 1 << ((bitLength+31) & 0x1f); // High bit of high int
            int highMask = (highBit << 1) - 1; // Bits to keep in high int

            while(true) {
                // Construct a candidate
            for (int i=0; i<magLen; i++)
558                 temp[i] = rnd.nextInt();
559             temp[0] = (temp[0] & highMask) | highBit; // Ensure exact length
560 if (bitLength > 2)
561                 temp[magLen-1] |= 1; // Make odd if bitlen > 2
562 
563             BigInteger   p = new BigInteger  (temp, 1);
564 
565             // Do cheap "pre-test" if applicable
566 if (bitLength > 6) {
567                 long r = p.remainder(SMALL_PRIME_PRODUCT).longValue();
568                 if ((r%3==0) || (r%5==0) || (r%7==0) || (r%11==0) || 
569                     (r%13==0) || (r%17==0) || (r%19==0) || (r%23==0) || 
570                     (r%29==0) || (r%31==0) || (r%37==0) || (r%41==0))
571                     continue; // Candidate is composite; try another
572 }
573             
574             // All candidates of bitLength 2 and 3 are prime by this point
575 if (bitLength < 4)
576                 return p;
577 
578             // Do expensive test if we survive pre-test (or it's inapplicable)
579 if (p.primeToCertainty(certainty))
580                 return p;
581         }
582     }
583 
584     private static final BigInteger   SMALL_PRIME_PRODUCT
585                        = valueOf(3L*5*7*11*13*17*19*23*29*31*37*41);
586 
587     /**
588      * Find a random number of the specified bitLength that is probably prime.
589      * This method is more appropriate for larger bitlengths since it uses
590      * a sieve to eliminate most composites before using a more expensive
591      * test.
592      */
593     private static BigInteger   largePrime(int bitLength, int certainty, Random   rnd) {
594         BigInteger   p;
595         p = new BigInteger  (bitLength, rnd).setBit(bitLength-1);
596         p.mag[p.mag.length-1] &= 0xfffffffe;
597 
598         // Use a sieve length likely to contain the next prime number
599 int searchLen = (bitLength / 20) * 64;
600         BitSieve   searchSieve = new BitSieve  (p, searchLen);
601         BigInteger   candidate = searchSieve.retrieve(p, certainty);
602 
603         while ((candidate == null) || (candidate.bitLength() != bitLength)) {
604             p = p.add(BigInteger.valueOf(2*searchLen));
605             if (p.bitLength() != bitLength)
606                 p = new BigInteger  (bitLength, rnd).setBit(bitLength-1);
607             p.mag[p.mag.length-1] &= 0xfffffffe;
608             searchSieve = new BitSieve  (p, searchLen);
609             candidate = searchSieve.retrieve(p, certainty);
610         }
611         return candidate;
612     }
613 
614    /**
615     * Returns the first integer greater than this <code>BigInteger</code> that
616     * is probably prime. The probability that the number returned by this
617     * method is composite does not exceed 2<sup>-100</sup>. This method will
618     * never skip over a prime when searching: if it returns <tt>p</tt>, there
619     * is no prime <tt>q</tt> such that <tt>this &lt; q &lt; p</tt>.
620     *
621     * @return the first integer greater than this <code>BigInteger</code> that
622     * is probably prime.
623     * @throws ArithmeticException <tt>this &lt; 0</tt>.
624     * @since 1.5
625     */
626     public BigInteger   nextProbablePrime() {
627         if (this.signum < 0)
628             throw new ArithmeticException  ("start < 0: " + this);
629         
630         // Handle trivial cases
631 if ((this.signum == 0) || this.equals(ONE))
632             return TWO;
633 
634         BigInteger   result = this.add(ONE);
635 
636         // Fastpath for small numbers
637 if (result.bitLength() < SMALL_PRIME_THRESHOLD) {
638  
639             // Ensure an odd number
640 if (!result.testBit(0))
641                 result = result.add(ONE);
642 
643             while(true) {
644                 // Do cheap "pre-test" if applicable
645 if (result.bitLength() > 6) {
646                     long r = result.remainder(SMALL_PRIME_PRODUCT).longValue();
647                     if ((r%3==0) || (r%5==0) || (r%7==0) || (r%11==0) || 
648                         (r%13==0) || (r%17==0) || (r%19==0) || (r%23==0) || 
649                         (r%29==0) || (r%31==0) || (r%37==0) || (r%41==0)) {
650                         result = result.add(TWO);
651                         continue; // Candidate is composite; try another
652 }
653                 }
654             
655                 // All candidates of bitLength 2 and 3 are prime by this point
656 if (result.bitLength() < 4)
657                     return result;
658 
659                 // The expensive test
660 if (result.primeToCertainty(DEFAULT_PRIME_CERTAINTY))
661                     return result;
662 
663                 result = result.add(TWO);
664             }
665         }
666 
667         // Start at previous even number
668 if (result.testBit(0))
669             result = result.subtract(ONE);
670 
671         // Looking for the next large prime
672 int searchLen = (result.bitLength() / 20) * 64;
673 
674         while(true) {
675            BitSieve   searchSieve = new BitSieve  (result, searchLen);
676            BigInteger   candidate = searchSieve.retrieve(result,
677                                                      DEFAULT_PRIME_CERTAINTY);
678            if (candidate != null)
679                return candidate;
680            result = result.add(BigInteger.valueOf(2 * searchLen));
681         }
682     }
683 
684     /**
685      * Returns <tt>true</tt> if this BigInteger is probably prime,
686      * <tt>false</tt> if it's definitely composite.
687      *
688      * This method assumes bitLength > 2.
689      *
690      * @param certainty a measure of the uncertainty that the caller is
691      * willing to tolerate: if the call returns <tt>true</tt>
692      * the probability that this BigInteger is prime exceeds
693      * <tt>(1 - 1/2<sup>certainty</sup>)</tt>. The execution time of
694      * this method is proportional to the value of this parameter.
695      * @return <tt>true</tt> if this BigInteger is probably prime,
696      * <tt>false</tt> if it's definitely composite.
697      */
698     boolean primeToCertainty(int certainty) {
699         int rounds = 0;
700         int n = (Math.min(certainty, Integer.MAX_VALUE-1)+1)/2;
701 
702         // The relationship between the certainty and the number of rounds
703 // we perform is given in the draft standard ANSI X9.80, "PRIME
704 // NUMBER GENERATION, PRIMALITY TESTING, AND PRIMALITY CERTIFICATES".
705 int sizeInBits = this.bitLength();
706         if (sizeInBits < 100) {
707             rounds = 50;
708             rounds = n < rounds ? n : rounds;
709             return passesMillerRabin(rounds);
710         }
711 
712         if (sizeInBits < 256) {
713             rounds = 27;
714         } else if (sizeInBits < 512) {
715             rounds = 15;
716         } else if (sizeInBits < 768) {
717             rounds = 8;
718         } else if (sizeInBits < 1024) {
719             rounds = 4;
720         } else {
721             rounds = 2;
722         }
723         rounds = n < rounds ? n : rounds;
724 
725         return passesMillerRabin(rounds) && passesLucasLehmer();
726     }
727 
728     /**
729      * Returns true iff this BigInteger is a Lucas-Lehmer probable prime.
730      *
731      * The following assumptions are made:
732      * This BigInteger is a positive, odd number.
733      */
734     private boolean passesLucasLehmer() {
735         BigInteger   thisPlusOne = this.add(ONE);
736 
737         // Step 1
738 int d = 5;
739         while (jacobiSymbol(d, this) != -1) {
740             // 5, -7, 9, -11, ...
741 d = (d<0) ? Math.abs(d)+2 : -(d+2);
742         }
743         
744         // Step 2
745 BigInteger   u = lucasLehmerSequence(d, thisPlusOne, this);
746 
747         // Step 3
748 return u.mod(this).equals(ZERO);
749     }
750 
751     /**
752      * Computes Jacobi(p,n).
753      * Assumes n positive, odd, n>=3.
754      */
755     private static int jacobiSymbol(int p, BigInteger   n) {
756         if (p == 0)
757             return 0;
758 
759         // Algorithm and comments adapted from Colin Plumb's C library.
760 int j = 1;
761     int u = n.mag[n.mag.length-1];
762 
763         // Make p positive
764 if (p < 0) {
765             p = -p;
766             int n8 = u & 7;
767             if ((n8 == 3) || (n8 == 7))
768                 j = -j; // 3 (011) or 7 (111) mod 8
769 }
770 
771     // Get rid of factors of 2 in p
772 while ((p & 3) == 0)
773             p >>= 2;
774     if ((p & 1) == 0) {
775             p >>= 1;
776             if (((u ^ (u>>1)) & 2) != 0)
777                 j = -j; // 3 (011) or 5 (101) mod 8
778 }
779     if (p == 1)
780         return j;
781     // Then, apply quadratic reciprocity
782 if ((p & u & 2) != 0) // p = u = 3 (mod 4)?
783 j = -j;
784     // And reduce u mod p
785 u = n.mod(BigInteger.valueOf(p)).intValue();
786 
787     // Now compute Jacobi(u,p), u < p
788 while (u != 0) {
789             while ((u & 3) == 0)
790                 u >>= 2;
791             if ((u & 1) == 0) {
792                 u >>= 1;
793                 if (((p ^ (p>>1)) & 2) != 0)
794                     j = -j; // 3 (011) or 5 (101) mod 8
795 }
796             if (u == 1)
797                 return j;
798             // Now both u and p are odd, so use quadratic reciprocity
799 assert (u < p);
800             int t = u; u = p; p = t;
801             if ((u & p & 2) != 0) // u = p = 3 (mod 4)?
802 j = -j;
803             // Now u >= p, so it can be reduced
804 u %= p;
805     }
806     return 0;
807     }
808 
809     private static BigInteger   lucasLehmerSequence(int z, BigInteger   k, BigInteger   n) {
810         BigInteger   d = BigInteger.valueOf(z);
811         BigInteger   u = ONE; BigInteger   u2;
812         BigInteger   v = ONE; BigInteger   v2;
813 
814         for (int i=k.bitLength()-2; i>=0; i--) {
815             u2 = u.multiply(v).mod(n);
816 
817             v2 = v.square().add(d.multiply(u.square())).mod(n);
818             if (v2.testBit(0)) {
819                 v2 = n.subtract(v2);
820                 v2.signum = - v2.signum;
821             }
822             v2 = v2.shiftRight(1);
823 
824             u = u2; v = v2;
825             if (k.testBit(i)) {
826                 u2 = u.add(v).mod(n);
827                 if (u2.testBit(0)) {
828                     u2 = n.subtract(u2);
829                     u2.signum = - u2.signum;
830                 }
831                 u2 = u2.shiftRight(1);
832                 
833                 v2 = v.add(d.multiply(u)).mod(n);
834                 if (v2.testBit(0)) {
835                     v2 = n.subtract(v2);
836                     v2.signum = - v2.signum;
837                 }
838                 v2 = v2.shiftRight(1);
839 
840                 u = u2; v = v2;
841             }
842         }
843         return u;
844     }
845 
846     /**
847      * Returns true iff this BigInteger passes the specified number of
848      * Miller-Rabin tests. This test is taken from the DSA spec (NIST FIPS
849      * 186-2).
850      *
851      * The following assumptions are made:
852      * This BigInteger is a positive, odd number greater than 2.
853      * iterations<=50.
854      */
855     private boolean passesMillerRabin(int iterations) {
856     // Find a and m such that m is odd and this == 1 + 2**a * m
857 BigInteger   thisMinusOne = this.subtract(ONE);
858     BigInteger   m = thisMinusOne;
859     int a = m.getLowestSetBit();
860     m = m.shiftRight(a);
861 
862     // Do the tests
863 Random   rnd = new Random  ();
864     for (int i=0; i<iterations; i++) {
865         // Generate a uniform random on (1, this)
866 BigInteger   b;
867         do {
868         b = new BigInteger  (this.bitLength(), rnd);
869         } while (b.compareTo(ONE) <= 0 || b.compareTo(this) >= 0);
870 
871         int j = 0;
872         BigInteger   z = b.modPow(m, this);
873         while(!((j==0 && z.equals(ONE)) || z.equals(thisMinusOne))) {
874         if (j>0 && z.equals(ONE) || ++j==a)
875             return false;
876         z = z.modPow(TWO, this);
877         }
878     }
879     return true;
880     }
881 
882     /**
883      * This private constructor differs from its public cousin
884      * with the arguments reversed in two ways: it assumes that its
885      * arguments are correct, and it doesn't copy the magnitude array.
886      */
887     private BigInteger(int[] magnitude, int signum) {
888     this.signum = (magnitude.length==0 ? 0 : signum);
889     this.mag = magnitude;
890     }
891 
892     /**
893      * This private constructor is for internal use and assumes that its
894      * arguments are correct.
895      */
896     private BigInteger(byte[] magnitude, int signum) {
897     this.signum = (magnitude.length==0 ? 0 : signum);
898         this.mag = stripLeadingZeroBytes(magnitude);
899     }
900 
901     /**
902      * This private constructor is for internal use in converting
903      * from a MutableBigInteger object into a BigInteger.
904      */
905     BigInteger(MutableBigInteger   val, int sign) {
906         if (val.offset > 0 || val.value.length != val.intLen) {
907             mag = new int[val.intLen];
908             for(int i=0; i<val.intLen; i++)
909                 mag[i] = val.value[val.offset+i];
910         } else {
911             mag = val.value;
912         }
913 
914     this.signum = (val.intLen == 0) ? 0 : sign;
915     }
916 
917     //Static Factory Methods
918 
919     /**
920      * Returns a BigInteger whose value is equal to that of the
921      * specified <code>long</code>. This "static factory method" is
922      * provided in preference to a (<code>long</code>) constructor
923      * because it allows for reuse of frequently used BigIntegers.
924      *
925      * @param val value of the BigInteger to return.
926      * @return a BigInteger with the specified value.
927      */
928     public static BigInteger   valueOf(long val) {
929     // If -MAX_CONSTANT < val < MAX_CONSTANT, return stashed constant
930 if (val == 0)
931         return ZERO;
932     if (val > 0 && val <= MAX_CONSTANT)
933         return posConst[(int) val];
934     else if (val < 0 && val >= -MAX_CONSTANT)
935         return negConst[(int) -val];
936 
937     return new BigInteger  (val);
938     }
939 
940     /**
941      * Constructs a BigInteger with the specified value, which may not be zero.
942      */
943     private BigInteger(long val) {
944         if (val < 0) {
945             signum = -1;
946             val = -val;
947         } else {
948             signum = 1;
949         }
950 
951         int highWord = (int)(val >>> 32);
952         if (highWord==0) {
953             mag = new int[1];
954             mag[0] = (int)val;
955         } else {
956             mag = new int[2];
957             mag[0] = highWord;
958             mag[1] = (int)val;
959         }
960     }
961 
962     /**
963      * Returns a BigInteger with the given two's complement representation.
964      * Assumes that the input array will not be modified (the returned
965      * BigInteger will reference the input array if feasible).
966      */
967     private static BigInteger   valueOf(int val[]) {
968         return (val[0]>0 ? new BigInteger  (val, 1) : new BigInteger  (val));
969     }
970 
971     // Constants
972 
973     /**
974      * Initialize static constant array when class is loaded.
975      */
976     private final static int MAX_CONSTANT = 16;
977     private static BigInteger   posConst[] = new BigInteger  [MAX_CONSTANT+1];
978     private static BigInteger   negConst[] = new BigInteger  [MAX_CONSTANT+1];
979     static {
980     for (int i = 1; i <= MAX_CONSTANT; i++) {
981         int[] magnitude = new int[1];
982         magnitude[0] = (int) i;
983         posConst[i] = new BigInteger  (magnitude, 1);
984         negConst[i] = new BigInteger  (magnitude, -1);
985     }
986     }
987 
988     /**
989      * The BigInteger constant zero.
990      *
991      * @since 1.2
992      */
993     public static final BigInteger   ZERO = new BigInteger  (new int[0], 0);
994 
995     /**
996      * The BigInteger constant one.
997      *
998      * @since 1.2
999      */
1000    public static final BigInteger   ONE = valueOf(1);
1001
1002    /**
1003     * The BigInteger constant two. (Not exported.)
1004     */
1005    private static final BigInteger   TWO = valueOf(2);
1006
1007    /**
1008     * The BigInteger constant ten.
1009     *
1010     * @since 1.5
1011     */
1012    public static final BigInteger   TEN = valueOf(10);
1013
1014    // Arithmetic Operations
1015
1016    /**
1017     * Returns a BigInteger whose value is <tt>(this + val)</tt>.
1018     *
1019     * @param val value to be added to this BigInteger.
1020     * @return <tt>this + val</tt>
1021     */
1022    public BigInteger   add(BigInteger   val) {
1023        int[] resultMag;
1024    if (val.signum == 0)
1025            return this;
1026    if (signum == 0)
1027        return val;
1028    if (val.signum == signum)
1029            return new BigInteger  (add(mag, val.mag), signum);
1030
1031        int cmp = intArrayCmp(mag, val.mag);
1032        if (cmp==0)
1033            return ZERO;
1034        resultMag = (cmp>0 ? subtract(mag, val.mag)
1035                           : subtract(val.mag, mag));
1036        resultMag = trustedStripLeadingZeroInts(resultMag);
1037
1038        return new BigInteger  (resultMag, cmp*signum);
1039    }
1040
1041    /**
1042     * Adds the contents of the int arrays x and y. This method allocates
1043     * a new int array to hold the answer and returns a reference to that
1044     * array.
1045     */
1046    private static int[] add(int[] x, int[] y) {
1047        // If x is shorter, swap the two arrays
1048 if (x.length < y.length) {
1049            int[] tmp = x;
1050            x = y;
1051            y = tmp;
1052        }
1053
1054        int xIndex = x.length;
1055        int yIndex = y.length;
1056        int result[] = new int[xIndex];
1057        long sum = 0;
1058
1059        // Add common parts of both numbers
1060 while(yIndex > 0) {
1061            sum = (x[--xIndex] & LONG_MASK) + 
1062                  (y[--yIndex] & LONG_MASK) + (sum >>> 32);
1063            result[xIndex] = (int)sum;
1064        }
1065
1066        // Copy remainder of longer number while carry propagation is required
1067 boolean carry = (sum >>> 32 != 0);
1068        while (xIndex > 0 && carry)
1069            carry = ((result[--xIndex] = x[xIndex] + 1) == 0);
1070
1071        // Copy remainder of longer number
1072 while (xIndex > 0)
1073            result[--xIndex] = x[xIndex];
1074
1075        // Grow result if necessary
1076 if (carry) {
1077            int newLen = result.length + 1;
1078            int temp[] = new int[newLen];
1079            for (int i = 1; i<newLen; i++)
1080                temp[i] = result[i-1];
1081            temp[0] = 0x01;
1082            result = temp;
1083        }
1084        return result;
1085    }
1086
1087    /**
1088     * Returns a BigInteger whose value is <tt>(this - val)</tt>.
1089     *
1090     * @param val value to be subtracted from this BigInteger.
1091     * @return <tt>this - val</tt>
1092     */
1093    public BigInteger   subtract(BigInteger   val) {
1094        int[] resultMag;
1095    if (val.signum == 0)
1096            return this;
1097    if (signum == 0)
1098        return val.negate();
1099    if (val.signum != signum)
1100            return new BigInteger  (add(mag, val.mag), signum);
1101
1102        int cmp = intArrayCmp(mag, val.mag);
1103        if (cmp==0)
1104            return ZERO;
1105        resultMag = (cmp>0 ? subtract(mag, val.mag)
1106                           : subtract(val.mag, mag));
1107        resultMag = trustedStripLeadingZeroInts(resultMag);
1108        return new BigInteger  (resultMag, cmp*signum);
1109    }
1110
1111    /**
1112     * Subtracts the contents of the second int arrays (little) from the
1113     * first (big). The first int array (big) must represent a larger number
1114     * than the second. This method allocates the space necessary to hold the
1115     * answer.
1116     */
1117    private static int[] subtract(int[] big, int[] little) {
1118        int bigIndex = big.length;
1119        int result[] = new int[bigIndex];
1120        int littleIndex = little.length;
1121        long difference = 0;
1122
1123        // Subtract common parts of both numbers
1124 while(littleIndex > 0) {
1125            difference = (big[--bigIndex] & LONG_MASK) - 
1126                         (little[--littleIndex] & LONG_MASK) +
1127                         (difference >> 32);
1128            result[bigIndex] = (int)difference;
1129        }
1130
1131        // Subtract remainder of longer number while borrow propagates
1132 boolean borrow = (difference >> 32 != 0);
1133        while (bigIndex > 0 && borrow)
1134            borrow = ((result[--bigIndex] = big[bigIndex] - 1) == -1);
1135
1136        // Copy remainder of longer number
1137 while (bigIndex > 0)
1138            result[--bigIndex] = big[bigIndex];
1139
1140        return result;
1141    }
1142
1143    /**
1144     * Returns a BigInteger whose value is <tt>(this * val)</tt>.
1145     *
1146     * @param val value to be multiplied by this BigInteger.
1147     * @return <tt>this * val</tt>
1148     */
1149    public BigInteger   multiply(BigInteger   val) {
1150        if (signum == 0 || val.signum==0)
1151        return ZERO;
1152        
1153        int[] result = multiplyToLen(mag, mag.length, 
1154                                     val.mag, val.mag.length, null);
1155        result = trustedStripLeadingZeroInts(result);
1156        return new BigInteger  (result, signum*val.signum);
1157    }
1158
1159    /**
1160     * Multiplies int arrays x and y to the specified lengths and places
1161     * the result into z.
1162     */
1163    private int[] multiplyToLen(int[] x, int xlen, int[] y, int ylen, int[] z) {
1164        int xstart = xlen - 1;
1165        int ystart = ylen - 1;
1166
1167        if (z == null || z.length < (xlen+ ylen))
1168            z = new int[xlen+ylen];
1169
1170        long carry = 0;
1171        for (int j=ystart, k=ystart+1+xstart; j>=0; j--, k--) {
1172            long product = (y[j] & LONG_MASK) *
1173                           (x[xstart] & LONG_MASK) + carry;
1174            z[k] = (int)product;
1175            carry = product >>> 32;
1176        }
1177        z[xstart] = (int)carry;
1178
1179        for (int i = xstart-1; i >= 0; i--) {
1180            carry = 0;
1181            for (int j=ystart, k=ystart+1+i; j>=0; j--, k--) {
1182                long product = (y[j] & LONG_MASK) * 
1183                               (x[i] & LONG_MASK) + 
1184                               (z[k] & LONG_MASK) + carry;
1185                z[k] = (int)product;
1186                carry = product >>> 32;
1187            }
1188            z[i] = (int)carry;
1189        }
1190        return z;
1191    }
1192
1193    /**
1194     * Returns a BigInteger whose value is <tt>(this<sup>2</sup>)</tt>.
1195     *
1196     * @return <tt>this<sup>2</sup></tt>
1197     */
1198    private BigInteger   square() {
1199        if (signum == 0)
1200        return ZERO;
1201        int[] z = squareToLen(mag, mag.length, null);
1202        return new BigInteger  (trustedStripLeadingZeroInts(z), 1);
1203    }
1204
1205    /**
1206     * Squares the contents of the int array x. The result is placed into the
1207     * int array z. The contents of x are not changed.
1208     */
1209    private static final int[] squareToLen(int[] x, int len, int[] z) {
1210        /*
1211         * The algorithm used here is adapted from Colin Plumb's C library.
1212         * Technique: Consider the partial products in the multiplication
1213         * of "abcde" by itself:
1214         *
1215         * a b c d e
1216         * * a b c d e
1217         * ==================
1218         * ae be ce de ee
1219         * ad bd cd dd de
1220         * ac bc cc cd ce
1221         * ab bb bc bd be
1222         * aa ab ac ad ae
1223         *
1224         * Note that everything above the main diagonal:
1225         * ae be ce de = (abcd) * e
1226         * ad bd cd = (abc) * d
1227         * ac bc = (ab) * c
1228         * ab = (a) * b
1229         *
1230         * is a copy of everything below the main diagonal:
1231         * de
1232         * cd ce
1233         * bc bd be
1234         * ab ac ad ae
1235         *
1236         * Thus, the sum is 2 * (off the diagonal) + diagonal.
1237         *
1238         * This is accumulated beginning with the diagonal (which
1239         * consist of the squares of the digits of the input), which is then
1240         * divided by two, the off-diagonal added, and multiplied by two
1241         * again. The low bit is simply a copy of the low bit of the
1242         * input, so it doesn't need special care.
1243         */
1244        int zlen = len << 1;
1245        if (z == null || z.length < zlen)
1246            z = new int[zlen];
1247        
1248        // Store the squares, right shifted one bit (i.e., divided by 2)
1249 int lastProductLowWord = 0;
1250        for (int j=0, i=0; j<len; j++) {
1251            long piece = (x[j] & LONG_MASK);
1252            long product = piece * piece;
1253            z[i++] = (lastProductLowWord << 31) | (int)(product >>> 33);
1254            z[i++] = (int)(product >>> 1);
1255            lastProductLowWord = (int)product;
1256        }
1257
1258        // Add in off-diagonal sums
1259 for (int i=len, offset=1; i>0; i--, offset+=2) {
1260            int t = x[i-1];
1261            t = mulAdd(z, x, offset, i-1, t);
1262            addOne(z, offset-1, i, t);
1263        }
1264
1265        // Shift back up and set low bit
1266 primitiveLeftShift(z, zlen, 1);
1267        z[zlen-1] |= x[len-1] & 1;
1268
1269        return z;
1270    }
1271
1272    /**
1273     * Returns a BigInteger whose value is <tt>(this / val)</tt>.
1274     *
1275     * @param val value by which this BigInteger is to be divided.
1276     * @return <tt>this / val</tt>
1277     * @throws ArithmeticException <tt>val==0</tt>
1278     */
1279    public BigInteger   divide(BigInteger   val) {
1280        MutableBigInteger   q = new MutableBigInteger  (),
1281                          r = new MutableBigInteger  (),
1282                          a = new MutableBigInteger  (this.mag),
1283                          b = new MutableBigInteger  (val.mag);
1284
1285        a.divide(b, q, r);
1286        return new BigInteger  (q, this.signum * val.signum);
1287    }
1288
1289    /**
1290     * Returns an array of two BigIntegers containing <tt>(this / val)</tt>
1291     * followed by <tt>(this % val)</tt>.
1292     *
1293     * @param val value by which this BigInteger is to be divided, and the
1294     * remainder computed.
1295     * @return an array of two BigIntegers: the quotient <tt>(this / val)</tt>
1296     * is the initial element, and the remainder <tt>(this % val)</tt>
1297     * is the final element.
1298     * @throws ArithmeticException <tt>val==0</tt>
1299     */
1300    public BigInteger  [] divideAndRemainder(BigInteger   val) {
1301        BigInteger  [] result = new BigInteger  [2];
1302        MutableBigInteger   q = new MutableBigInteger  (),
1303                          r = new MutableBigInteger  (),
1304                          a = new MutableBigInteger  (this.mag),
1305                          b = new MutableBigInteger  (val.mag);
1306        a.divide(b, q, r);
1307        result[0] = new BigInteger  (q, this.signum * val.signum);
1308        result[1] = new BigInteger  (r, this.signum);
1309        return result;
1310    }
1311
1312    /**
1313     * Returns a BigInteger whose value is <tt>(this % val)</tt>.
1314     *
1315     * @param val value by which this BigInteger is to be divided, and the
1316     * remainder computed.
1317     * @return <tt>this % val</tt>
1318     * @throws ArithmeticException <tt>val==0</tt>
1319     */
1320    public BigInteger   remainder(BigInteger   val) {
1321        MutableBigInteger   q = new MutableBigInteger  (),
1322                          r = new MutableBigInteger  (),
1323                          a = new MutableBigInteger  (this.mag),
1324                          b = new MutableBigInteger  (val.mag);
1325
1326        a.divide(b, q, r);
1327        return new BigInteger  (r, this.signum);
1328    }
1329
1330    /**
1331     * Returns a BigInteger whose value is <tt>(this<sup>exponent</sup>)</tt>.
1332     * Note that <tt>exponent</tt> is an integer rather than a BigInteger.
1333     *
1334     * @param exponent exponent to which this BigInteger is to be raised.
1335     * @return <tt>this<sup>exponent</sup></tt>
1336     * @throws ArithmeticException <tt>exponent</tt> is negative. (This would
1337     * cause the operation to yield a non-integer value.)
1338     */
1339    public BigInteger   pow(int exponent) {
1340    if (exponent < 0)
1341        throw new ArithmeticException  ("Negative exponent");
1342    if (signum==0)
1343        return (exponent==0 ? ONE : this);
1344
1345    // Perform exponentiation using repeated squaring trick
1346 int newSign = (signum<0 && (exponent&1)==1 ? -1 : 1);
1347    int[] baseToPow2 = this.mag;
1348        int[] result = {1};
1349
1350    while (exponent != 0) {
1351        if ((exponent & 1)==1) {
1352        result = multiplyToLen(result, result.length, 
1353                                       baseToPow2, baseToPow2.length, null);
1354        result = trustedStripLeadingZeroInts(result);
1355        }
1356        if ((exponent >>>= 1) != 0) {
1357                baseToPow2 = squareToLen(baseToPow2, baseToPow2.length, null);
1358        baseToPow2 = trustedStripLeadingZeroInts(baseToPow2);
1359        }
1360    }
1361    return new BigInteger  (result, newSign);
1362    }
1363
1364    /**
1365     * Returns a BigInteger whose value is the greatest common divisor of
1366     * <tt>abs(this)</tt> and <tt>abs(val)</tt>. Returns 0 if
1367     * <tt>this==0 &amp;&amp; val==0</tt>.
1368     *
1369     * @param val value with which the GCD is to be computed.
1370     * @return <tt>GCD(abs(this), abs(val))</tt>
1371     */
1372    public BigInteger   gcd(BigInteger   val) {
1373        if (val.signum == 0)
1374        return this.abs();
1375    else if (this.signum == 0)
1376        return val.abs();
1377
1378        MutableBigInteger   a = new MutableBigInteger  (this);
1379        MutableBigInteger   b = new MutableBigInteger  (val);
1380
1381        MutableBigInteger   result = a.hybridGCD(b);
1382
1383        return new BigInteger  (result, 1);
1384    }
1385
1386    /**
1387     * Left shift int array a up to len by n bits. Returns the array that
1388     * results from the shift since space may have to be reallocated.
1389     */
1390    private static int[] leftShift(int[] a, int len, int n) {
1391        int nInts = n >>> 5;
1392        int nBits = n&0x1F;
1393        int bitsInHighWord = bitLen(a[0]);
1394        
1395        // If shift can be done without recopy, do so
1396 if (n <= (32-bitsInHighWord)) {
1397            primitiveLeftShift(a, len, nBits);
1398            return a;
1399        } else { // Array must be resized
1400 if (nBits <= (32-bitsInHighWord)) {
1401                int result[] = new int[nInts+len];
1402                for (int i=0; i<len; i++)
1403                    result[i] = a[i];
1404                primitiveLeftShift(result, result.length, nBits);
1405                return result;
1406            } else {
1407                int result[] = new int[nInts+len+1];
1408                for (int i=0; i<len; i++)
1409                    result[i] = a[i];
1410                primitiveRightShift(result, result.length, 32 - nBits);
1411                return result;
1412            }
1413        }
1414    }
1415
1416    // shifts a up to len right n bits assumes no leading zeros, 0<n<32
1417 static void primitiveRightShift(int[] a, int len, int n) {
1418        int n2 = 32 - n;
1419        for (int i=len-1, c=a[i]; i>0; i--) {
1420            int b = c;
1421            c = a[i-1];
1422            a[i] = (c << n2) | (b >>> n);
1423        }
1424        a[0] >>>= n;
1425    }
1426
1427    // shifts a up to len left n bits assumes no leading zeros, 0<=n<32
1428 static void primitiveLeftShift(int[] a, int len, int n) {
1429        if (len == 0 || n == 0)
1430            return;
1431
1432        int n2 = 32 - n;
1433        for (int i=0, c=a[i], m=i+len-1; i<m; i++) {
1434            int b = c;
1435            c = a[i+1];
1436            a[i] = (b << n) | (c >>> n2);
1437        }
1438        a[len-1] <<= n;
1439    }
1440
1441    /**
1442     * Calculate bitlength of contents of the first len elements an int array,
1443     * assuming there are no leading zero ints.
1444     */
1445    private static int bitLength(int[] val, int len) {
1446        if (len==0)
1447            return 0;
1448        return ((len-1)<<5) + bitLen(val[0]);
1449    }
1450
1451    /**
1452     * Returns a BigInteger whose value is the absolute value of this
1453     * BigInteger. 
1454     *
1455     * @return <tt>abs(this)</tt>
1456     */
1457    public BigInteger   abs() {
1458    return (signum >= 0 ? this : this.negate());
1459    }
1460
1461    /**
1462     * Returns a BigInteger whose value is <tt>(-this)</tt>.
1463     *
1464     * @return <tt>-this</tt>
1465     */
1466    public BigInteger   negate() {
1467    return new BigInteger  (this.mag, -this.signum);
1468    }
1469
1470    /**
1471     * Returns the signum function of this BigInteger.
1472     *
1473     * @return -1, 0 or 1 as the value of this BigInteger is negative, zero or
1474     * positive.
1475     */
1476    public int signum() {
1477    return this.signum;
1478    }
1479
1480    // Modular Arithmetic Operations
1481
1482    /**
1483     * Returns a BigInteger whose value is <tt>(this mod m</tt>). This method
1484     * differs from <tt>remainder</tt> in that it always returns a
1485     * <i>non-negative</i> BigInteger.
1486     *
1487     * @param m the modulus.
1488     * @return <tt>this mod m</tt>
1489     * @throws ArithmeticException <tt>m &lt;= 0</tt>
1490     * @see #remainder
1491     */
1492    public BigInteger   mod(BigInteger   m) {
1493    if (m.signum <= 0)
1494        throw new ArithmeticException  ("BigInteger: modulus not positive");
1495
1496    BigInteger   result = this.remainder(m);
1497    return (result.signum >= 0 ? result : result.add(m));
1498    }
1499
1500    /**
1501     * Returns a BigInteger whose value is
1502     * <tt>(this<sup>exponent</sup> mod m)</tt>. (Unlike <tt>pow</tt>, this
1503     * method permits negative exponents.)
1504     *
1505     * @param exponent the exponent.
1506     * @param m the modulus.
1507     * @return <tt>this<sup>exponent</sup> mod m</tt>
1508     * @throws ArithmeticException <tt>m &lt;= 0</tt>
1509     * @see #modInverse
1510     */
1511    public BigInteger   modPow(BigInteger   exponent, BigInteger   m) {
1512    if (m.signum <= 0)
1513        throw new ArithmeticException  ("BigInteger: modulus not positive");
1514
1515    // Trivial cases
1516 if (exponent.signum == 0)
1517            return (m.equals(ONE) ? ZERO : ONE);
1518
1519        if (this.equals(ONE))
1520            return (m.equals(ONE) ? ZERO : ONE);
1521
1522        if (this.equals(ZERO) && exponent.signum >= 0)
1523            return ZERO;
1524
1525        if (this.equals(negConst[1]) && (!exponent.testBit(0)))
1526            return (m.equals(ONE) ? ZERO : ONE);
1527            
1528    boolean invertResult;
1529    if ((invertResult = (exponent.signum < 0)))
1530        exponent = exponent.negate();
1531
1532    BigInteger   base = (this.signum < 0 || this.compareTo(m) >= 0
1533               ? this.mod(m) : this);
1534    BigInteger   result;
1535    if (m.testBit(0)) { // odd modulus
1536 result = base.oddModPow(exponent, m);
1537    } else {
1538        /*
1539         * Even modulus. Tear it into an "odd part" (m1) and power of two
1540             * (m2), exponentiate mod m1, manually exponentiate mod m2, and
1541             * use Chinese Remainder Theorem to combine results.
1542         */
1543
1544        // Tear m apart into odd part (m1) and power of 2 (m2)
1545 int p = m.getLowestSetBit(); // Max pow of 2 that divides m
1546
1547        BigInteger   m1 = m.shiftRight(p); // m/2**p
1548 BigInteger   m2 = ONE.shiftLeft(p); // 2**p
1549
1550            // Calculate new base from m1
1551 BigInteger   base2 = (this.signum < 0 || this.compareTo(m1) >= 0
1552                                ? this.mod(m1) : this);
1553
1554            // Caculate (base ** exponent) mod m1.
1555 BigInteger   a1 = (m1.equals(ONE) ? ZERO :
1556                             base2.oddModPow(exponent, m1));
1557
1558        // Calculate (this ** exponent) mod m2
1559 BigInteger   a2 = base.modPow2(exponent, p);
1560
1561        // Combine results using Chinese Remainder Theorem
1562 BigInteger   y1 = m2.modInverse(m1);
1563        BigInteger   y2 = m1.modInverse(m2);
1564
1565        result = a1.multiply(m2).multiply(y1).add
1566             (a2.multiply(m1).multiply(y2)).mod(m);
1567    }
1568
1569    return (invertResult ? result.modInverse(m) : result);
1570    }
1571
1572    static int[] bnExpModThreshTable = {7, 25, 81, 241, 673, 1793,
1573                                                Integer.MAX_VALUE}; // Sentinel
1574
1575    /**
1576     * Returns a BigInteger whose value is x to the power of y mod z.
1577     * Assumes: z is odd && x < z.
1578     */
1579    private BigInteger   oddModPow(BigInteger   y, BigInteger   z) {
1580    /*
1581     * The algorithm is adapted from Colin Plumb's C library.
1582     *
1583     * The window algorithm:
1584     * The idea is to keep a running product of b1 = n^(high-order bits of exp)
1585     * and then keep appending exponent bits to it. The following patterns
1586     * apply to a 3-bit window (k = 3):
1587     * To append 0: square
1588     * To append 1: square, multiply by n^1
1589     * To append 10: square, multiply by n^1, square
1590     * To append 11: square, square, multiply by n^3
1591     * To append 100: square, multiply by n^1, square, square
1592     * To append 101: square, square, square, multiply by n^5
1593     * To append 110: square, square, multiply by n^3, square
1594     * To append 111: square, square, square, multiply by n^7
1595     *
1596     * Since each pattern involves only one multiply, the longer the pattern
1597     * the better, except that a 0 (no multiplies) can be appended directly.
1598     * We precompute a table of odd powers of n, up to 2^k, and can then
1599     * multiply k bits of exponent at a time. Actually, assuming random
1600     * exponents, there is on average one zero bit between needs to
1601     * multiply (1/2 of the time there's none, 1/4 of the time there's 1,
1602     * 1/8 of the time, there's 2, 1/32 of the time, there's 3, etc.), so
1603     * you have to do one multiply per k+1 bits of exponent.
1604     *
1605     * The loop walks down the exponent, squaring the result buffer as
1606     * it goes. There is a wbits+1 bit lookahead buffer, buf, that is
1607     * filled with the upcoming exponent bits. (What is read after the
1608     * end of the exponent is unimportant, but it is filled with zero here.)
1609     * When the most-significant bit of this buffer becomes set, i.e.
1610     * (buf & tblmask) != 0, we have to decide what pattern to multiply
1611     * by, and when to do it. We decide, remember to do it in future
1612     * after a suitable number of squarings have passed (e.g. a pattern
1613     * of "100" in the buffer requires that we multiply by n^1 immediately;
1614     * a pattern of "110" calls for multiplying by n^3 after one more
1615     * squaring), clear the buffer, and continue.
1616     *
1617     * When we start, there is one more optimization: the result buffer
1618     * is implcitly one, so squaring it or multiplying by it can be
1619     * optimized away. Further, if we start with a pattern like "100"
1620     * in the lookahead window, rather than placing n into the buffer
1621     * and then starting to square it, we have already computed n^2
1622     * to compute the odd-powers table, so we can place that into
1623     * the buffer and save a squaring.
1624     *
1625     * This means that if you have a k-bit window, to compute n^z,
1626     * where z is the high k bits of the exponent, 1/2 of the time
1627     * it requires no squarings. 1/4 of the time, it requires 1
1628     * squaring, ... 1/2^(k-1) of the time, it reqires k-2 squarings.
1629     * And the remaining 1/2^(k-1) of the time, the top k bits are a
1630     * 1 followed by k-1 0 bits, so it again only requires k-2
1631     * squarings, not k-1. The average of these is 1. Add that
1632     * to the one squaring we have to do to compute the table,
1633     * and you'll see that a k-bit window saves k-2 squarings
1634     * as well as reducing the multiplies. (It actually doesn't
1635     * hurt in the case k = 1, either.)
1636     */
1637        // Special case for exponent of one
1638 if (y.equals(ONE))
1639            return this;
1640
1641        // Special case for base of zero
1642 if (signum==0)
1643            return ZERO;
1644
1645        int[] base = (int[])mag.clone();
1646        int[] exp = y.mag;
1647        int[] mod = z.mag;
1648        int modLen = mod.length;
1649
1650        // Select an appropriate window size
1651 int wbits = 0;
1652        int ebits = bitLength(exp, exp.length);
1653    // if exponent is 65537 (0x10001), use minimum window size
1654 if ((ebits != 17) || (exp[0] != 65537)) {
1655        while (ebits > bnExpModThreshTable[wbits]) {
1656        wbits++;
1657        }
1658    }
1659
1660        // Calculate appropriate table size
1661 int tblmask = 1 << wbits;
1662        
1663        // Allocate table for precomputed odd powers of base in Montgomery form
1664 int[][] table = new int[tblmask][];
1665        for (int i=0; i<tblmask; i++)
1666            table[i] = new int[modLen];
1667
1668        // Compute the modular inverse
1669 int inv = -MutableBigInteger.inverseMod32(mod[modLen-1]);
1670
1671        // Convert base to Montgomery form
1672 int[] a = leftShift(base, base.length, modLen << 5);
1673
1674        MutableBigInteger   q = new MutableBigInteger  (),
1675                          r = new MutableBigInteger  (),
1676                          a2 = new MutableBigInteger  (a),
1677                          b2 = new MutableBigInteger  (mod);
1678
1679        a2.divide(b2, q, r);
1680        table[0] = r.toIntArray();
1681
1682        // Pad table[0] with leading zeros so its length is at least modLen
1683 if (table[0].length < modLen) {
1684           int offset = modLen - table[0].length;
1685           int[] t2 = new int[modLen];
1686           for (int i=0; i<table[0].length; i++)
1687               t2[i+offset] = table[0][i];
1688           table[0] = t2;
1689        }
1690
1691        // Set b to the square of the base
1692 int[] b = squareToLen(table[0], modLen, null);
1693        b = montReduce(b, mod, modLen, inv);
1694
1695        // Set t to high half of b
1696 int[] t = new int[modLen];
1697        for(int i=0; i<modLen; i++)
1698            t[i] = b[i];
1699
1700        // Fill in the table with odd powers of the base 
1701 for (int i=1; i<tblmask; i++) {
1702            int[] prod = multiplyToLen(t, modLen, table[i-1], modLen, null);
1703            table[i] = montReduce(prod, mod, modLen, inv);
1704        }
1705
1706        // Pre load the window that slides over the exponent
1707 int bitpos = 1 << ((ebits-1) & (32-1));
1708        
1709        int buf = 0;
1710        int elen = exp.length;
1711        int eIndex = 0;
1712    for (int i = 0; i <= wbits; i++) {
1713            buf = (buf << 1) | (((exp[eIndex] & bitpos) != 0)?1:0);
1714            bitpos >>>= 1;
1715            if (bitpos == 0) {
1716                eIndex++;
1717                bitpos = 1 << (32-1);
1718                elen--;
1719            }
1720    }
1721
1722        int multpos = ebits;
1723
1724        // The first iteration, which is hoisted out of the main loop
1725 ebits--;
1726        boolean isone = true;
1727
1728    multpos = ebits - wbits;
1729    while ((buf & 1) == 0) {
1730            buf >>>= 1;
1731            multpos++;
1732    }
1733
1734    int[] mult = table[buf >>> 1];
1735
1736    buf = 0;
1737        if (multpos == ebits)
1738        isone = false;
1739
1740        // The main loop
1741 while(true) {
1742            ebits--;
1743            // Advance the window
1744 buf <<= 1;
1745
1746            if (elen != 0) {
1747                buf |= ((exp[eIndex] & bitpos) != 0) ? 1 : 0;
1748                bitpos >>>= 1;
1749                if (bitpos == 0) {
1750                    eIndex++;
1751                    bitpos = 1 << (32-1);
1752                    elen--;
1753                }
1754            }
1755
1756            // Examine the window for pending multiplies
1757 if ((buf & tblmask) != 0) {
1758                multpos = ebits - wbits;
1759                while ((buf & 1) == 0) {
1760                    buf >>>= 1;
1761                    multpos++;
1762                }
1763                mult = table[buf >>> 1];
1764                buf = 0;
1765            }
1766
1767            // Perform multiply
1768 if (ebits == multpos) {
1769                if (isone) {
1770                    b = (int[])mult.clone();
1771                    isone = false;
1772                } else {
1773                    t = b;
1774                    a = multiplyToLen(t, modLen, mult, modLen, a);
1775                    a = montReduce(a, mod, modLen, inv);
1776                    t = a; a = b; b = t;
1777                }
1778            }
1779
1780            // Check if done
1781 if (ebits == 0)
1782                break;
1783
1784            // Square the input
1785 if (!isone) {
1786                t = b;
1787                a = squareToLen(t, modLen, a);
1788                a = montReduce(a, mod, modLen, inv);
1789                t = a; a = b; b = t;
1790            }
1791    }
1792
1793        // Convert result out of Montgomery form and return
1794 int[] t2 = new int[2*modLen];
1795        for(int i=0; i<modLen; i++)
1796            t2[i+modLen] = b[i];
1797
1798        b = montReduce(t2, mod, modLen, inv);
1799
1800        t2 = new int[modLen];
1801        for(int i=0; i<modLen; i++)
1802            t2[i] = b[i];
1803           
1804        return new BigInteger  (1, t2);
1805    }
1806
1807    /**
1808     * Montgomery reduce n, modulo mod. This reduces modulo mod and divides
1809     * by 2^(32*mlen). Adapted from Colin Plumb's C library.
1810     */
1811    private static int[] montReduce(int[] n, int[] mod, int mlen, int inv) {
1812        int c=0;
1813        int len = mlen;
1814        int offset=0;
1815
1816        do {
1817            int nEnd = n[n.length-1-offset];
1818            int carry = mulAdd(n, mod, offset, mlen, inv * nEnd);
1819            c += addOne(n, offset, mlen, carry);
1820            offset++;
1821        } while(--len > 0);
1822        
1823        while(c>0)
1824            c += subN(n, mod, mlen);
1825
1826        while (intArrayCmpToLen(n, mod, mlen) >= 0)
1827            subN(n, mod, mlen);
1828
1829        return n;
1830    }
1831
1832
1833    /*
1834     * Returns -1, 0 or +1 as big-endian unsigned int array arg1 is less than,
1835     * equal to, or greater than arg2 up to length len.
1836     */
1837    private static int intArrayCmpToLen(int[] arg1, int[] arg2, int len) {
1838    for (int i=0; i<len; i++) {
1839        long b1 = arg1[i] & LONG_MASK;
1840        long b2 = arg2[i] & LONG_MASK;
1841        if (b1 < b2)
1842        return -1;
1843        if (b1 > b2)
1844        return 1;
1845    }
1846    return 0;
1847    }
1848
1849    /**
1850     * Subtracts two numbers of same length, returning borrow.
1851     */
1852    private static int subN(int[] a, int[] b, int len) {
1853        long sum = 0;
1854
1855        while(--len >= 0) {
1856            sum = (a[len] & LONG_MASK) - 
1857                 (b[len] & LONG_MASK) + (sum >> 32);
1858            a[len] = (int)sum;
1859        }
1860
1861        return (int)(sum >> 32);
1862    }
1863
1864    /**
1865     * Multiply an array by one word k and add to result, return the carry
1866     */
1867    static int mulAdd(int[] out, int[] in, int offset, int len, int k) {
1868        long kLong = k & LONG_MASK;
1869        long carry = 0;
1870
1871        offset = out.length-offset - 1;
1872        for (int j=len-1; j >= 0; j--) {
1873            long product = (in[j] & LONG_MASK) * kLong +
1874                           (out[offset] & LONG_MASK) + carry;
1875            out[offset--] = (int)product;
1876            carry = product >>> 32;
1877        }
1878        return (int)carry;
1879    }
1880
1881    /**
1882     * Add one word to the number a mlen words into a. Return the resulting
1883     * carry.
1884     */
1885    static int addOne(int[] a, int offset, int mlen, int carry) {
1886        offset = a.length-1-mlen-offset;
1887        long t = (a[offset] & LONG_MASK) + (carry & LONG_MASK);
1888        
1889        a[offset] = (int)t;
1890        if ((t >>> 32) == 0)
1891            return 0;
1892        while (--mlen >= 0) {
1893            if (--offset < 0) { // Carry out of number
1894 return 1;
1895            } else {
1896                a[offset]++;
1897                if (a[offset] != 0)
1898                    return 0;
1899            }
1900        }
1901        return 1;
1902    }
1903
1904    /**
1905     * Returns a BigInteger whose value is (this ** exponent) mod (2**p)
1906     */
1907    private BigInteger   modPow2(BigInteger   exponent, int p) {
1908    /*
1909     * Perform exponentiation using repeated squaring trick, chopping off
1910     * high order bits as indicated by modulus.
1911     */
1912    BigInteger   result = valueOf(1);
1913    BigInteger   baseToPow2 = this.mod2(p);
1914        int expOffset = 0;
1915
1916        int limit = exponent.bitLength();
1917
1918        if (this.testBit(0))
1919           limit = (p-1) < limit ? (p-1) : limit;
1920
1921    while (expOffset < limit) {
1922        if (exponent.testBit(expOffset))
1923        result = result.multiply(baseToPow2).mod2(p);
1924            expOffset++;
1925        if (expOffset < limit)
1926                baseToPow2 = baseToPow2.square().mod2(p);
1927    }
1928
1929    return result;
1930    }
1931
1932    /**
1933     * Returns a BigInteger whose value is this mod(2**p).
1934     * Assumes that this BigInteger &gt;= 0 and p &gt; 0.
1935     */
1936    private BigInteger   mod2(int p) {
1937    if (bitLength() <= p)
1938        return this;
1939
1940    // Copy remaining ints of mag
1941 int numInts = (p+31)/32;
1942    int[] mag = new int[numInts];
1943    for (int i=0; i<numInts; i++)
1944        mag[i] = this.mag[i + (this.mag.length - numInts)];
1945
1946    // Mask out any excess bits
1947 int excessBits = (numInts << 5) - p;
1948    mag[0] &= (1L << (32-excessBits)) - 1;
1949
1950    return (mag[0]==0 ? new BigInteger  (1, mag) : new BigInteger  (mag, 1));
1951    }
1952
1953    /**
1954     * Returns a BigInteger whose value is <tt>(this<sup>-1</sup> mod m)</tt>.
1955     *
1956     * @param m the modulus.
1957     * @return <tt>this<sup>-1</sup> mod m</tt>.
1958     * @throws ArithmeticException <tt> m &lt;= 0</tt>, or this BigInteger
1959     * has no multiplicative inverse mod m (that is, this BigInteger
1960     * is not <i>relatively prime</i> to m).
1961     */
1962    public BigInteger   modInverse(BigInteger   m) {
1963    if (m.signum != 1)
1964        throw new ArithmeticException  ("BigInteger: modulus not positive");
1965
1966        if (m.equals(ONE))
1967            return ZERO;
1968
1969    // Calculate (this mod m)
1970 BigInteger   modVal = this;
1971        if (signum < 0 || (intArrayCmp(mag, m.mag) >= 0))
1972            modVal = this.mod(m);
1973
1974        if (modVal.equals(ONE))
1975            return ONE;
1976
1977        MutableBigInteger   a = new MutableBigInteger  (modVal);
1978        MutableBigInteger   b = new MutableBigInteger  (m);
1979  
1980        MutableBigInteger   result = a.mutableModInverse(b); 
1981        return new BigInteger  (result, 1);
1982    }
1983
1984    // Shift Operations
1985
1986    /**
1987     * Returns a BigInteger whose value is <tt>(this &lt;&lt; n)</tt>.
1988     * The shift distance, <tt>n</tt>, may be negative, in which case
1989     * this method performs a right shift.
1990     * (Computes <tt>floor(this * 2<sup>n</sup>)</tt>.)
1991     *
1992     * @param n shift distance, in bits.
1993     * @return <tt>this &lt;&lt; n</tt>
1994     * @see #shiftRight
1995     */
1996    public BigInteger   shiftLeft(int n) {
1997        if (signum == 0)
1998            return ZERO;
1999        if (n==0)
2000            return this;
2001        if (n<0)
2002            return shiftRight(-n);
2003
2004        int nInts = n >>> 5;
2005        int nBits = n & 0x1f;
2006        int magLen = mag.length;
2007        int newMag[] = null;
2008
2009        if (nBits == 0) {
2010            newMag = new int[magLen + nInts];
2011            for (int i=0; i<magLen; i++)
2012                newMag[i] = mag[i];
2013        } else {
2014            int i = 0;
2015            int nBits2 = 32 - nBits;
2016            int highBits = mag[0] >>> nBits2;
2017            if (highBits != 0) {
2018                newMag = new int[magLen + nInts + 1];
2019                newMag[i++] = highBits;
2020            } else {
2021                newMag = new int[magLen + nInts];
2022            }
2023            int j=0;
2024            while (j < magLen-1)
2025                newMag[i++] = mag[j++] << nBits | mag[j] >>> nBits2;
2026            newMag[i] = mag[j] << nBits;
2027        }
2028
2029        return new BigInteger  (newMag, signum);
2030    }
2031
2032    /**
2033     * Returns a BigInteger whose value is <tt>(this &gt;&gt; n)</tt>. Sign
2034     * extension is performed. The shift distance, <tt>n</tt>, may be
2035     * negative, in which case this method performs a left shift.
2036     * (Computes <tt>floor(this / 2<sup>n</sup>)</tt>.) 
2037     *
2038     * @param n shift distance, in bits.
2039     * @return <tt>this &gt;&gt; n</tt>
2040     * @see #shiftLeft
2041     */
2042    public BigInteger   shiftRight(int n) {
2043        if (n==0)
2044            return this;
2045        if (n<0)
2046            return shiftLeft(-n);
2047
2048        int nInts = n >>> 5;
2049        int nBits = n & 0x1f;
2050        int magLen = mag.length;
2051        int newMag[] = null;
2052
2053        // Special case: entire contents shifted off the end
2054 if (nInts >= magLen)
2055            return (signum >= 0 ? ZERO : negConst[1]);
2056
2057        if (nBits == 0) {
2058            int newMagLen = magLen - nInts;
2059            newMag = new int[newMagLen];
2060            for (int i=0; i<newMagLen; i++)
2061                newMag[i] = mag[i];
2062        } else {
2063            int i = 0;
2064            int highBits = mag[0] >>> nBits;
2065            if (highBits != 0) {
2066                newMag = new int[magLen - nInts];
2067                newMag[i++] = highBits;
2068            } else {
2069                newMag = new int[magLen - nInts -1];
2070            }
2071
2072            int nBits2 = 32 - nBits;
2073            int j=0;
2074            while (j < magLen - nInts - 1)
2075                newMag[i++] = (mag[j++] << nBits2) | (mag[j] >>> nBits);
2076        }
2077
2078        if (signum < 0) {
2079            // Find out whether any one-bits were shifted off the end.
2080 boolean onesLost = false;
2081            for (int i=magLen-1, j=magLen-nInts; i>=j && !onesLost; i--)
2082                onesLost = (mag[i] != 0);
2083            if (!onesLost && nBits != 0)
2084                onesLost = (mag[magLen - nInts - 1] << (32 - nBits) != 0);
2085
2086            if (onesLost)
2087                newMag = javaIncrement(newMag);
2088        }
2089
2090        return new BigInteger  (newMag, signum);
2091    }
2092
2093    int[] javaIncrement(int[] val) {
2094        boolean done = false;
2095        int lastSum = 0;
2096        for (int i=val.length-1; i >= 0 && lastSum == 0; i--)
2097            lastSum = (val[i] += 1);
2098        if (lastSum == 0) {
2099            val = new int[val.length+1];
2100            val[0] = 1;
2101        }
2102        return val;
2103    }
2104
2105    // Bitwise Operations
2106
2107    /**
2108     * Returns a BigInteger whose value is <tt>(this &amp; val)</tt>. (This
2109     * method returns a negative BigInteger if and only if this and val are
2110     * both negative.)
2111     *
2112     * @param val value to be AND'ed with this BigInteger.
2113     * @return <tt>this &amp; val</tt>
2114     */
2115    public BigInteger   and(BigInteger   val) {
2116    int[] result = new int[Math.max(intLength(), val.intLength())];
2117    for (int i=0; i<result.length; i++)
2118        result[i] = (int) (getInt(result.length-i-1)
2119                & val.getInt(result.length-i-1));
2120
2121    return valueOf(result);
2122    }
2123
2124    /**
2125     * Returns a BigInteger whose value is <tt>(this | val)</tt>. (This method
2126     * returns a negative BigInteger if and only if either this or val is
2127     * negative.) 
2128     *
2129     * @param val value to be OR'ed with this BigInteger.
2130     * @return <tt>this | val</tt>
2131     */
2132    public BigInteger   or(BigInteger   val) {
2133    int[] result = new int[Math.max(intLength(), val.intLength())];
2134    for (int i=0; i<result.length; i++)
2135        result[i] = (int) (getInt(result.length-i-1)
2136                | val.getInt(result.length-i-1));
2137
2138    return valueOf(result);
2139    }
2140
2141    /**
2142     * Returns a BigInteger whose value is <tt>(this ^ val)</tt>. (This method
2143     * returns a negative BigInteger if and only if exactly one of this and
2144     * val are negative.)
2145     *
2146     * @param val value to be XOR'ed with this BigInteger.
2147     * @return <tt>this ^ val</tt>
2148     */
2149    public BigInteger   xor(BigInteger   val) {
2150    int[] result = new int[Math.max(intLength(), val.intLength())];
2151    for (int i=0; i<result.length; i++)
2152        result[i] = (int) (getInt(result.length-i-1)
2153                ^ val.getInt(result.length-i-1));
2154
2155    return valueOf(result);
2156    }
2157
2158    /**
2159     * Returns a BigInteger whose value is <tt>(~this)</tt>. (This method
2160     * returns a negative value if and only if this BigInteger is
2161     * non-negative.)
2162     *
2163     * @return <tt>~this</tt>
2164     */
2165    public BigInteger   not() {
2166    int[] result = new int[intLength()];
2167    for (int i=0; i<result.length; i++)
2168        result[i] = (int) ~getInt(result.length-i-1);
2169
2170    return valueOf(result);
2171    }
2172
2173    /**
2174     * Returns a BigInteger whose value is <tt>(this &amp; ~val)</tt>. This
2175     * method, which is equivalent to <tt>and(val.not())</tt>, is provided as
2176     * a convenience for masking operations. (This method returns a negative
2177     * BigInteger if and only if <tt>this</tt> is negative and <tt>val</tt> is
2178     * positive.)
2179     *
2180     * @param val value to be complemented and AND'ed with this BigInteger.
2181     * @return <tt>this &amp; ~val</tt>
2182     */
2183    public BigInteger   andNot(BigInteger   val) {
2184    int[] result = new int[Math.max(intLength(), val.intLength())];
2185    for (int i=0; i<result.length; i++)
2186        result[i] = (int) (getInt(result.length-i-1)
2187                & ~val.getInt(result.length-i-1));
2188
2189    return valueOf(result);
2190    }
2191
2192
2193    // Single Bit Operations
2194
2195    /**
2196     * Returns <tt>true</tt> if and only if the designated bit is set.
2197     * (Computes <tt>((this &amp; (1&lt;&lt;n)) != 0)</tt>.)
2198     *
2199     * @param n index of bit to test.
2200     * @return <tt>true</tt> if and only if the designated bit is set.
2201     * @throws ArithmeticException <tt>n</tt> is negative.
2202     */
2203    public boolean testBit(int n) {
2204    if (n<0)
2205        throw new ArithmeticException  ("Negative bit address");
2206
2207    return (getInt(n/32) & (1 << (n%32))) != 0;
2208    }
2209
2210    /**
2211     * Returns a BigInteger whose value is equivalent to this BigInteger
2212     * with the designated bit set. (Computes <tt>(this | (1&lt;&lt;n))</tt>.)
2213     *
2214     * @param n index of bit to set.
2215     * @return <tt>this | (1&lt;&lt;n)</tt>
2216     * @throws ArithmeticException <tt>n</tt> is negative.
2217     */
2218    public BigInteger   setBit(int n) {
2219    if (n<0)
2220        throw new ArithmeticException  ("Negative bit address");
2221
2222    int intNum = n/32;
2223    int[] result = new int[Math.max(intLength(), intNum+2)];
2224
2225    for (int i=0; i<result.length; i++)
2226        result[result.length-i-1] = getInt(i);
2227
2228    result[result.length-intNum-1] |= (1 << (n%32));
2229
2230    return valueOf(result);
2231    }
2232
2233    /**
2234     * Returns a BigInteger whose value is equivalent to this BigInteger
2235     * with the designated bit cleared.
2236     * (Computes <tt>(this &amp; ~(1&lt;&lt;n))</tt>.)
2237     *
2238     * @param n index of bit to clear.
2239     * @return <tt>this & ~(1&lt;&lt;n)</tt>
2240     * @throws ArithmeticException <tt>n</tt> is negative.
2241     */
2242    public BigInteger   clearBit(int n) {
2243    if (n<0)
2244        throw new ArithmeticException  ("Negative bit address");
2245
2246    int intNum = n/32;
2247    int[] result = new int[Math.max(intLength(), (n+1)/32+1)];
2248
2249    for (int i=0; i<result.length; i++)
2250        result[result.length-i-1] = getInt(i);
2251
2252    result[result.length-intNum-1] &= ~(1 << (n%32));
2253
2254    return valueOf(result);
2255    }
2256
2257    /**
2258     * Returns a BigInteger whose value is equivalent to this BigInteger
2259     * with the designated bit flipped.
2260     * (Computes <tt>(this ^ (1&lt;&lt;n))</tt>.)
2261     *
2262     * @param n index of bit to flip.
2263     * @return <tt>this ^ (1&lt;&lt;n)</tt>
2264     * @throws ArithmeticException <tt>n</tt> is negative.
2265     */
2266    public BigInteger   flipBit(int n) {
2267    if (n<0)
2268        throw new ArithmeticException  ("Negative bit address");
2269
2270    int intNum = n/32;
2271    int[] result = new int[Math.max(intLength(), intNum+2)];
2272
2273    for (int i=0; i<result.length; i++)
2274        result[result.length-i-1] = getInt(i);
2275
2276    result[result.length-intNum-1] ^= (1 << (n%32));
2277
2278    return valueOf(result);
2279    }
2280
2281    /**
2282     * Returns the index of the rightmost (lowest-order) one bit in this
2283     * BigInteger (the number of zero bits to the right of the rightmost
2284     * one bit). Returns -1 if this BigInteger contains no one bits.
2285     * (Computes <tt>(this==0? -1 : log<sub>2</sub>(this &amp; -this))</tt>.)
2286     *
2287     * @return index of the rightmost one bit in this BigInteger.
2288     */
2289    public int getLowestSetBit() {
2290    /*
2291     * Initialize lowestSetBit field the first time this method is
2292     * executed. This method depends on the atomicity of int modifies;
2293     * without this guarantee, it would have to be synchronized.
2294     */
2295    if (lowestSetBit == -2) {
2296        if (signum == 0) {
2297        lowestSetBit = -1;
2298        } else {
2299        // Search for lowest order nonzero int
2300 int i,b;
2301        for (i=0; (b = getInt(i))==0; i++)
2302            ;
2303        lowestSetBit = (i << 5) + trailingZeroCnt(b);
2304        }
2305    }
2306    return lowestSetBit;
2307    }
2308
2309
2310    // Miscellaneous Bit Operations
2311
2312    /**
2313     * Returns the number of bits in the minimal two's-complement
2314     * representation of this BigInteger, <i>excluding</i> a sign bit.
2315     * For positive BigIntegers, this is equivalent to the number of bits in
2316     * the ordinary binary representation. (Computes
2317     * <tt>(ceil(log<sub>2</sub>(this &lt; 0 ? -this : this+1)))</tt>.)
2318     *
2319     * @return number of bits in the minimal two's-complement
2320     * representation of this BigInteger, <i>excluding</i> a sign bit.
2321     */
2322    public int bitLength() {
2323    /*
2324     * Initialize bitLength field the first time this method is executed.
2325     * This method depends on the atomicity of int modifies; without
2326     * this guarantee, it would have to be synchronized.
2327     */
2328    if (bitLength == -1) {
2329        if (signum == 0) {
2330        bitLength = 0;
2331        } else {
2332        // Calculate the bit length of the magnitude
2333 int magBitLength = ((mag.length-1) << 5) + bitLen(mag[0]);
2334
2335        if (signum < 0) {
2336            // Check if magnitude is a power of two
2337 boolean pow2 = (bitCnt(mag[0]) == 1);
2338            for(int i=1; i<mag.length && pow2; i++)
2339            pow2 = (mag[i]==0);
2340
2341            bitLength = (pow2 ? magBitLength-1 : magBitLength);
2342        } else {
2343            bitLength = magBitLength;
2344        }
2345        }
2346    }
2347    return bitLength;
2348    }
2349
2350    /**
2351     * bitLen(val) is the number of bits in val.
2352     */
2353    static int bitLen(int w) {
2354        // Binary search - decision tree (5 tests, rarely 6)
2355 return
2356         (w < 1<<15 ?
2357          (w < 1<<7 ?
2358           (w < 1<<3 ?
2359            (w < 1<<1 ? (w < 1<<0 ? (w<0 ? 32 : 0) : 1) : (w < 1<<2 ? 2 : 3)) :
2360            (w < 1<<5 ? (w < 1<<4 ? 4 : 5) : (w < 1<<6 ? 6 : 7))) :
2361           (w < 1<<11 ?
2362            (w < 1<<9 ? (w < 1<<8 ? 8 : 9) : (w < 1<<10 ? 10 : 11)) :
2363            (w < 1<<13 ? (w < 1<<12 ? 12 : 13) : (w < 1<<14 ? 14 : 15)))) :
2364          (w < 1<<23 ?
2365           (w < 1<<19 ?
2366            (w < 1<<17 ? (w < 1<<16 ? 16 : 17) : (w < 1<<18 ? 18 : 19)) :
2367            (w < 1<<21 ? (w < 1<<20 ? 20 : 21) : (w < 1<<22 ? 22 : 23))) :
2368           (w < 1<<27 ?
2369            (w < 1<<25 ? (w < 1<<24 ? 24 : 25) : (w < 1<<26 ? 26 : 27)) :
2370            (w < 1<<29 ? (w < 1<<28 ? 28 : 29) : (w < 1<<30 ? 30 : 31)))));
2371    }
2372
2373    /*
2374     * trailingZeroTable[i] is the number of trailing zero bits in the binary
2375     * representation of i.
2376     */
2377    final static byte trailingZeroTable[] = {
2378      -25, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2379    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2380    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2381    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2382    6, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2383    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2384    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2385    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2386    7, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2387    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2388    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2389    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2390    6, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2391    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2392    5, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0,
2393    4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0};
2394
2395    /**
2396     * Returns the number of bits in the two's complement representation
2397     * of this BigInteger that differ from its sign bit. This method is
2398     * useful when implementing bit-vector style sets atop BigIntegers.
2399     *
2400     * @return number of bits in the two's complement representation
2401     * of this BigInteger that differ from its sign bit.
2402     */
2403    public int bitCount() {
2404    /*
2405     * Initialize bitCount field the first time this method is executed.
2406     * This method depends on the atomicity of int modifies; without
2407     * this guarantee, it would have to be synchronized.
2408     */
2409    if (bitCount == -1) {
2410        // Count the bits in the magnitude
2411 int magBitCount = 0;
2412        for (int i=0; i<mag.length; i++)
2413        magBitCount += bitCnt(mag[i]);
2414
2415        if (signum < 0) {
2416        // Count the trailing zeros in the magnitude
2417 int magTrailingZeroCount = 0, j;
2418        for (j=mag.length-1; mag[j]==0; j--)
2419            magTrailingZeroCount += 32;
2420        magTrailingZeroCount +=
2421                            trailingZeroCnt(mag[j]);
2422
2423        bitCount = magBitCount + magTrailingZeroCount - 1;
2424        } else {
2425        bitCount = magBitCount;
2426        }
2427    }
2428    return bitCount;
2429    }
2430
2431    static int bitCnt(int val) {
2432        val -= (0xaaaaaaaa & val) >>> 1;
2433        val = (val & 0x33333333) + ((val >>> 2) & 0x33333333);
2434        val = val + (val >>> 4) & 0x0f0f0f0f;
2435        val += val >>> 8;
2436        val += val >>> 16;
2437        return val & 0xff;
2438    }
2439
2440    static int trailingZeroCnt(int val) {
2441        // Loop unrolled for performance
2442 int byteVal = val & 0xff;
2443        if (byteVal != 0)
2444            return trailingZeroTable[byteVal];
2445
2446        byteVal = (val >>> 8) & 0xff;
2447        if (byteVal != 0)
2448            return trailingZeroTable[byteVal] + 8;
2449
2450        byteVal = (val >>> 16) & 0xff;
2451        if (byteVal != 0)
2452            return trailingZeroTable[byteVal] + 16;
2453
2454        byteVal = (val >>> 24) & 0xff;
2455        return trailingZeroTable[byteVal] + 24;
2456    }
2457
2458    // Primality Testing
2459
2460    /**
2461     * Returns <tt>true</tt> if this BigInteger is probably prime,
2462     * <tt>false</tt> if it's definitely composite. If
2463     * <tt>certainty</tt> is <tt> &lt;= 0</tt>, <tt>true</tt> is
2464     * returned.
2465     *
2466     * @param certainty a measure of the uncertainty that the caller is
2467     * willing to tolerate: if the call returns <tt>true</tt>
2468     * the probability that this BigInteger is prime exceeds
2469     * <tt>(1 - 1/2<sup>certainty</sup>)</tt>. The execution time of
2470     * this method is proportional to the value of this parameter.
2471     * @return <tt>true</tt> if this BigInteger is probably prime,
2472     * <tt>false</tt> if it's definitely composite.
2473     */
2474    public boolean isProbablePrime(int certainty) {
2475    if (certainty <= 0)
2476        return true;
2477    BigInteger   w = this.abs();
2478    if (w.equals(TWO))
2479        return true;
2480    if (!w.testBit(0) || w.equals(ONE))
2481        return false;
2482
2483        return w.primeToCertainty(certainty);
2484    }
2485
2486    // Comparison Operations
2487
2488    /**
2489     * Compares this BigInteger with the specified BigInteger. This method is
2490     * provided in preference to individual methods for each of the six
2491     * boolean comparison operators (&lt;, ==, &gt;, &gt;=, !=, &lt;=). The
2492     * suggested idiom for performing these comparisons is:
2493     * <tt>(x.compareTo(y)</tt> &lt;<i>op</i>&gt; <tt>0)</tt>,
2494     * where &lt;<i>op</i>&gt; is one of the six comparison operators.
2495     *
2496     * @param val BigInteger to which this BigInteger is to be compared.
2497     * @return -1, 0 or 1 as this BigInteger is numerically less than, equal
2498     * to, or greater than <tt>val</tt>.
2499     */
2500    public int compareTo(BigInteger   val) {
2501    return (signum==val.signum
2502        ? signum*intArrayCmp(mag, val.mag)
2503        : (signum>val.signum ? 1 : -1));
2504    }
2505
2506    /*
2507     * Returns -1, 0 or +1 as big-endian unsigned int array arg1 is
2508     * less than, equal to, or greater than arg2.
2509     */
2510    private static int intArrayCmp(int[] arg1, int[] arg2) {
2511    if (arg1.length < arg2.length)
2512        return -1;
2513    if (arg1.length > arg2.length)
2514        return 1;
2515
2516    // Argument lengths are equal; compare the values
2517 for (int i=0; i<arg1.length; i++) {
2518        long b1 = arg1[i] & LONG_MASK;
2519        long b2 = arg2[i] & LONG_MASK;
2520        if (b1 < b2)
2521        return -1;
2522        if (b1 > b2)
2523        return 1;
2524    }
2525    return 0;
2526    }
2527
2528    /**
2529     * Compares this BigInteger with the specified Object for equality.
2530     *
2531     * @param x Object to which this BigInteger is to be compared.
2532     * @return <tt>true</tt> if and only if the specified Object is a
2533     * BigInteger whose value is numerically equal to this BigInteger.
2534     */
2535    public boolean equals(Object   x) {
2536    // This test is just an optimization, which may or may not help
2537 if (x == this)
2538        return true;
2539
2540    if (!(x instanceof BigInteger  ))
2541        return false;
2542    BigInteger   xInt = (BigInteger  ) x;
2543
2544    if (xInt.signum != signum || xInt.mag.length != mag.length)
2545        return false;
2546
2547    for (int i=0; i<mag.length; i++)
2548        if (xInt.mag[i] != mag[i])
2549        return false;
2550
2551    return true;
2552    }
2553
2554    /**
2555     * Returns the minimum of this BigInteger and <tt>val</tt>.
2556     *
2557     * @param val value with which the minimum is to be computed.
2558     * @return the BigInteger whose value is the lesser of this BigInteger and 
2559     * <tt>val</tt>. If they are equal, either may be returned.
2560     */
2561    public BigInteger   min(BigInteger   val) {
2562    return (compareTo(val)<0 ? this : val);
2563    }
2564
2565    /**
2566     * Returns the maximum of this BigInteger and <tt>val</tt>.
2567     *
2568     * @param val value with which the maximum is to be computed.
2569     * @return the BigInteger whose value is the greater of this and
2570     * <tt>val</tt>. If they are equal, either may be returned.
2571     */
2572    public BigInteger   max(BigInteger   val) {
2573    return (compareTo(val)>0 ? this : val);
2574    }
2575
2576
2577    // Hash Function
2578
2579    /**
2580     * Returns the hash code for this BigInteger.
2581     *
2582     * @return hash code for this BigInteger.
2583     */
2584    public int hashCode() {
2585    int hashCode = 0;
2586
2587    for (int i=0; i<mag.length; i++)
2588        hashCode = (int)(31*hashCode + (mag[i] & LONG_MASK));
2589
2590    return hashCode * signum;
2591    }
2592
2593    /**
2594     * Returns the String representation of this BigInteger in the
2595     * given radix. If the radix is outside the range from {@link
2596     * Character#MIN_RADIX} to {@link Character#MAX_RADIX} inclusive,
2597     * it will default to 10 (as is the case for
2598     * <tt>Integer.toString</tt>). The digit-to-character mapping
2599     * provided by <tt>Character.forDigit</tt> is used, and a minus
2600     * sign is prepended if appropriate. (This representation is
2601     * compatible with the {@link #BigInteger(String, int) (String,
2602     * <code>int</code>)} constructor.)
2603     *
2604     * @param radix radix of the String representation.
2605     * @return String representation of this BigInteger in the given radix.
2606     * @see Integer#toString
2607     * @see Character#forDigit
2608     * @see #BigInteger(java.lang.String, int)
2609     */
2610    public String   toString(int radix) {
2611    if (signum == 0)
2612        return "0";
2613    if (radix < Character.MIN_RADIX || radix > Character.MAX_RADIX)
2614        radix = 10;
2615
2616    // Compute upper bound on number of digit groups and allocate space
2617 int maxNumDigitGroups = (4*mag.length + 6)/7;
2618    String   digitGroup[] = new String  [maxNumDigitGroups];
2619        
2620    // Translate number to string, a digit group at a time
2621 BigInteger   tmp = this.abs();
2622    int numGroups = 0;
2623    while (tmp.signum != 0) {
2624            BigInteger   d = longRadix[radix];
2625
2626            MutableBigInteger   q = new MutableBigInteger  (),
2627                              r = new MutableBigInteger  (),
2628                              a = new MutableBigInteger  (tmp.mag),
2629                              b = new MutableBigInteger  (d.mag);
2630            a.divide(b, q, r);
2631            BigInteger   q2 = new BigInteger  (q, tmp.signum * d.signum);
2632            BigInteger   r2 = new BigInteger  (r, tmp.signum * d.signum);
2633
2634            digitGroup[numGroups++] = Long.toString(r2.longValue(), radix);
2635            tmp = q2;
2636    }
2637
2638    // Put sign (if any) and first digit group into result buffer
2639 StringBuilder   buf = new StringBuilder  (numGroups*digitsPerLong[radix]+1);
2640    if (signum<0)
2641        buf.append('-');
2642    buf.append(digitGroup[numGroups-1]);
2643
2644    // Append remaining digit groups padded with leading zeros
2645 for (int i=numGroups-2; i>=0; i--) {
2646        // Prepend (any) leading zeros for this digit group
2647 int numLeadingZeros = digitsPerLong[radix]-digitGroup[i].length();
2648        if (numLeadingZeros != 0)
2649        buf.append(zeros[numLeadingZeros]);
2650        buf.append(digitGroup[i]);
2651    }
2652    return buf.toString();
2653    }
2654
2655    /* zero[i] is a string of i consecutive zeros. */
2656    private static String   zeros[] = new String  [64];
2657    static {
2658    zeros[63] =
2659        "000000000000000000000000000000000000000000000000000000000000000";
2660    for (int i=0; i<63; i++)
2661        zeros[i] = zeros[63].substring(0, i);
2662    }
2663
2664    /**
2665     * Returns the decimal String representation of this BigInteger.
2666     * The digit-to-character mapping provided by
2667     * <tt>Character.forDigit</tt> is used, and a minus sign is
2668     * prepended if appropriate. (This representation is compatible
2669     * with the {@link #BigInteger(String) (String)} constructor, and
2670     * allows for String concatenation with Java's + operator.)
2671     *
2672     * @return decimal String representation of this BigInteger.
2673     * @see Character#forDigit
2674     * @see #BigInteger(java.lang.String)
2675     */
2676    public String   toString() {
2677    return toString(10);
2678    }
2679
2680    /**
2681     * Returns a byte array containing the two's-complement
2682     * representation of this BigInteger. The byte array will be in
2683     * <i>big-endian</i> byte-order: the most significant byte is in
2684     * the zeroth element. The array will contain the minimum number
2685     * of bytes required to represent this BigInteger, including at
2686     * least one sign bit, which is <tt>(ceil((this.bitLength() +
2687     * 1)/8))</tt>. (This representation is compatible with the
2688     * {@link #BigInteger(byte[]) (byte[])} constructor.)
2689     *
2690     * @return a byte array containing the two's-complement representation of
2691     * this BigInteger.
2692     * @see #BigInteger(byte[])
2693     */
2694    public byte[] toByteArray() {
2695        int byteLen = bitLength()/8 + 1;
2696        byte[] byteArray = new byte[byteLen];
2697
2698        for (int i=byteLen-1, bytesCopied=4, nextInt=0, intIndex=0; i>=0; i--) {
2699            if (bytesCopied == 4) {
2700                nextInt = getInt(intIndex++);
2701                bytesCopied = 1;
2702            } else {
2703                nextInt >>>= 8;
2704                bytesCopied++;
2705            }
2706            byteArray[i] = (byte)nextInt;
2707        }
2708        return byteArray;
2709    }
2710
2711    /**
2712     * Converts this BigInteger to an <code>int</code>. This
2713     * conversion is analogous to a <a
2714     * HREF="http://java.sun.com/docs/books/jls/second_edition/html/conversions.doc.html#25363"><i>narrowing
2715     * primitive conversion</i></a> from <code>long</code> to
2716     * <code>int</code> as defined in the <a
2717     * HREF="http://java.sun.com/docs/books/jls/html/">Java Language
2718     * Specification</a>: if this BigInteger is too big to fit in an
2719     * <code>int</code>, only the low-order 32 bits are returned.
2720     * Note that this conversion can lose information about the
2721     * overall magnitude of the BigInteger value as well as return a
2722     * result with the opposite sign.
2723     *
2724     * @return this BigInteger converted to an <code>int</code>.
2725     */
2726    public int intValue() {
2727    int result = 0;
2728    result = getInt(0);
2729    return result;
2730    }
2731
2732    /**
2733     * Converts this BigInteger to a <code>long</code>. This
2734     * conversion is analogous to a <a
2735     * HREF="http://java.sun.com/docs/books/jls/second_edition/html/conversions.doc.html#25363"><i>narrowing
2736     * primitive conversion</i></a> from <code>long</code> to
2737     * <code>int</code> as defined in the <a
2738     * HREF="http://java.sun.com/docs/books/jls/html/">Java Language
2739     * Specification</a>: if this BigInteger is too big to fit in a
2740     * <code>long</code>, only the low-order 64 bits are returned.
2741     * Note that this conversion can lose information about the
2742     * overall magnitude of the BigInteger value as well as return a
2743     * result with the opposite sign.
2744     *
2745     * @return this BigInteger converted to a <code>long</code>.
2746     */
2747    public long longValue() {
2748    long result = 0;
2749
2750    for (int i=1; i>=0; i--)
2751        result = (result << 32) + (getInt(i) & LONG_MASK);
2752    return result;
2753    }
2754
2755    /**
2756     * Converts this BigInteger to a <code>float</code>. This
2757     * conversion is similar to the <a
2758     * HREF="http://java.sun.com/docs/books/jls/second_edition/html/conversions.doc.html#25363"><i>narrowing
2759     * primitive conversion</i></a> from <code>double</code> to
2760     * <code>float</code> defined in the <a
2761     * HREF="http://java.sun.com/docs/books/jls/html/">Java Language
2762     * Specification</a>: if this BigInteger has too great a magnitude
2763     * to represent as a <code>float</code>, it will be converted to
2764     * {@link Float#NEGATIVE_INFINITY} or {@link
2765     * Float#POSITIVE_INFINITY} as appropriate. Note that even when
2766     * the return value is finite, this conversion can lose
2767     * information about the precision of the BigInteger value.
2768     *
2769     * @return this BigInteger converted to a <code>float</code>.
2770     */
2771    public float floatValue() {
2772    // Somewhat inefficient, but guaranteed to work.
2773 return Float.parseFloat(this.toString());
2774    }
2775
2776    /**
2777     * Converts this BigInteger to a <code>double</code>. This
2778     * conversion is similar to the <a
2779     * HREF="http://java.sun.com/docs/books/jls/second_edition/html/conversions.doc.html#25363"><i>narrowing
2780     * primitive conversion</i></a> from <code>double</code> to
2781     * <code>float</code> defined in the <a
2782     * HREF="http://java.sun.com/docs/books/jls/html/">Java Language
2783     * Specification</a>: if this BigInteger has too great a magnitude
2784     * to represent as a <code>double</code>, it will be converted to
2785     * {@link Double#NEGATIVE_INFINITY} or {@link
2786     * Double#POSITIVE_INFINITY} as appropriate. Note that even when
2787     * the return value is finite, this conversion can lose
2788     * information about the precision of the BigInteger value.
2789     *
2790     * @return this BigInteger converted to a <code>double</code>.
2791     */
2792    public double doubleValue() {
2793    // Somewhat inefficient, but guaranteed to work.
2794 return Double.parseDouble(this.toString());
2795    }
2796
2797    /**
2798     * Returns a copy of the input array stripped of any leading zero bytes.
2799     */
2800    private static int[] stripLeadingZeroInts(int val[]) {
2801        int byteLength = val.length;
2802    int keep;
2803
2804    // Find first nonzero byte
2805 for (keep=0; keep<val.length && val[keep]==0; keep++)
2806            ;
2807
2808        int result[] = new int[val.length - keep];
2809        for(int i=0; i<val.length - keep; i++)
2810            result[i] = val[keep+i];
2811
2812        return result;
2813    }
2814
2815    /**
2816     * Returns the input array stripped of any leading zero bytes.
2817     * Since the source is trusted the copying may be skipped.
2818     */
2819    private static int[] trustedStripLeadingZeroInts(int val[]) {
2820        int byteLength = val.length;
2821    int keep;
2822
2823    // Find first nonzero byte
2824 for (keep=0; keep<val.length && val[keep]==0; keep++)
2825            ;
2826
2827        // Only perform copy if necessary
2828 if (keep > 0) {
2829            int result[] = new int[val.length - keep];
2830            for(int i=0; i<val.length - keep; i++)
2831               result[i] = val[keep+i];
2832            return result; 
2833        }
2834        return val;
2835    }
2836
2837    /**
2838     * Returns a copy of the input array stripped of any leading zero bytes.
2839     */
2840    private static int[] stripLeadingZeroBytes(byte a[]) {
2841        int byteLength = a.length;
2842    int keep;
2843
2844    // Find first nonzero byte
2845 for (keep=0; keep<a.length && a[keep]==0; keep++)
2846        ;
2847
2848    // Allocate new array and copy relevant part of input array
2849 int intLength = ((byteLength - keep) + 3)/4;
2850    int[] result = new int[intLength];
2851        int b = byteLength - 1;
2852        for (int i = intLength-1; i >= 0; i--) {
2853            result[i] = a[b--] & 0xff;
2854            int bytesRemaining = b - keep + 1;
2855            int bytesToTransfer = Math.min(3, bytesRemaining);
2856            for (int j=8; j <= 8*bytesToTransfer; j += 8)
2857                result[i] |= ((a[b--] & 0xff) << j);
2858        }
2859        return result;
2860    }
2861
2862    /**
2863     * Takes an array a representing a negative 2's-complement number and
2864     * returns the minimal (no leading zero bytes) unsigned whose value is -a.
2865     */
2866    private static int[] makePositive(byte a[]) {
2867    int keep, k;
2868        int byteLength = a.length;
2869
2870    // Find first non-sign (0xff) byte of input
2871 for (keep=0; keep<byteLength && a[keep]==-1; keep++)
2872        ;
2873
2874        
2875    /* Allocate output array. If all non-sign bytes are 0x00, we must
2876     * allocate space for one extra output byte. */
2877    for (k=keep; k<byteLength && a[k]==0; k++)
2878        ;
2879
2880    int extraByte = (k==byteLength) ? 1 : 0;
2881        int intLength = ((byteLength - keep + extraByte) + 3)/4;
2882    int result[] = new int[intLength];
2883
2884    /* Copy one's complement of input into output, leaving extra
2885     * byte (if it exists) == 0x00 */
2886        int b = byteLength - 1;
2887        for (int i = intLength-1; i >= 0; i--) {
2888            result[i] = a[b--] & 0xff;
2889            int numBytesToTransfer = Math.min(3, b-keep+1);
2890            if (numBytesToTransfer < 0)
2891                numBytesToTransfer = 0;
2892            for (int j=8; j <= 8*numBytesToTransfer; j += 8)
2893                result[i] |= ((a[b--] & 0xff) << j);
2894
2895            // Mask indicates which bits must be complemented
2896 int mask = -1 >>> (8*(3-numBytesToTransfer));
2897            result[i] = ~result[i] & mask;
2898        }
2899
2900    // Add one to one's complement to generate two's complement
2901 for (int i=result.length-1; i>=0; i--) {
2902            result[i] = (int)((result[i] & LONG_MASK) + 1);
2903        if (result[i] != 0)
2904                break;
2905        }
2906
2907    return result;
2908    }
2909
2910    /**
2911     * Takes an array a representing a negative 2's-complement number and
2912     * returns the minimal (no leading zero ints) unsigned whose value is -a.
2913     */
2914    private static int[] makePositive(int a[]) {
2915    int keep, j;
2916
2917    // Find first non-sign (0xffffffff) int of input
2918 for (keep=0; keep<a.length && a[keep]==-1; keep++)
2919        ;
2920
2921    /* Allocate output array. If all non-sign ints are 0x00, we must
2922     * allocate space for one extra output int. */
2923    for (j=keep; j<a.length && a[j]==0; j++)
2924        ;
2925    int extraInt = (j==a.length ? 1 : 0);
2926    int result[] = new int[a.length - keep + extraInt];
2927
2928    /* Copy one's complement of input into output, leaving extra
2929     * int (if it exists) == 0x00 */
2930    for (int i = keep; i<a.length; i++)
2931        result[i - keep + extraInt] = ~a[i];
2932
2933    // Add one to one's complement to generate two's complement
2934 for (int i=result.length-1; ++result[i]==0; i--)
2935        ;
2936
2937    return result;
2938    }
2939
2940    /*
2941     * The following two arrays are used for fast String conversions. Both
2942     * are indexed by radix. The first is the number of digits of the given
2943     * radix that can fit in a Java long without "going negative", i.e., the
2944     * highest integer n such that radix**n < 2**63. The second is the
2945     * "long radix" that tears each number into "long digits", each of which
2946     * consists of the number of digits in the corresponding element in
2947     * digitsPerLong (longRadix[i] = i**digitPerLong[i]). Both arrays have
2948     * nonsense values in their 0 and 1 elements, as radixes 0 and 1 are not
2949     * used.
2950     */
2951    private static int digitsPerLong[] = {0, 0,
2952    62, 39, 31, 27, 24, 22, 20, 19, 18, 18, 17, 17, 16, 16, 15, 15, 15, 14,
2953    14, 14, 14, 13, 13, 13, 13, 13, 13, 12, 12, 12, 12, 12, 12, 12, 12};
2954
2955    private static BigInteger   longRadix[] = {null, null,
2956        valueOf(0x4000000000000000L), valueOf(0x383d9170b85ff80bL),
2957    valueOf(0x4000000000000000L), valueOf(0x6765c793fa10079dL),
2958    valueOf(0x41c21cb8e1000000L), valueOf(0x3642798750226111L),
2959        valueOf(0x1000000000000000L), valueOf(0x12bf307ae81ffd59L),
2960    valueOf( 0xde0b6b3a7640000L), valueOf(0x4d28cb56c33fa539L),
2961    valueOf(0x1eca170c00000000L), valueOf(0x780c7372621bd74dL),
2962    valueOf(0x1e39a5057d810000L), valueOf(0x5b27ac993df97701L),
2963    valueOf(0x1000000000000000L), valueOf(0x27b95e997e21d9f1L),
2964    valueOf(0x5da0e1e53c5c8000L), valueOf( 0xb16a458ef403f19L),
2965    valueOf(0x16bcc41e90000000L), valueOf(0x2d04b7fdd9c0ef49L),
2966    valueOf(0x5658597bcaa24000L), valueOf( 0x6feb266931a75b7L),
2967    valueOf( 0xc29e98000000000L), valueOf(0x14adf4b7320334b9L),
2968    valueOf(0x226ed36478bfa000L), valueOf(0x383d9170b85ff80bL),
2969    valueOf(0x5a3c23e39c000000L), valueOf( 0x4e900abb53e6b71L),
2970    valueOf( 0x7600ec618141000L), valueOf( 0xaee5720ee830681L),
2971    valueOf(0x1000000000000000L), valueOf(0x172588ad4f5f0981L),
2972    valueOf(0x211e44f7d02c1000L), valueOf(0x2ee56725f06e5c71L),
2973    valueOf(0x41c21cb8e1000000L)};
2974
2975    /*
2976     * These two arrays are the integer analogue of above.
2977     */
2978    private static int digitsPerInt[] = {0, 0, 30, 19, 15, 13, 11,
2979        11, 10, 9, 9, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6,
2980        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 5};
2981
2982    private static int intRadix[] = {0, 0,
2983        0x40000000, 0x4546b3db, 0x40000000, 0x48c27395, 0x159fd800,
2984        0x75db9c97, 0x40000000, 0x17179149, 0x3b9aca00, 0xcc6db61,
2985        0x19a10000, 0x309f1021, 0x57f6c100, 0xa2f1b6f, 0x10000000,
2986        0x18754571, 0x247dbc80, 0x3547667b, 0x4c4b4000, 0x6b5a6e1d,
2987        0x6c20a40, 0x8d2d931, 0xb640000, 0xe8d4a51, 0x1269ae40,
2988        0x17179149, 0x1cb91000, 0x23744899, 0x2b73a840, 0x34e63b41,
2989        0x40000000, 0x4cfa3cc1, 0x5c13d840, 0x6d91b519, 0x39aa400
2990    };
2991
2992    /**
2993     * These routines provide access to the two's complement representation
2994     * of BigIntegers.
2995     */
2996
2997    /**
2998     * Returns the length of the two's complement representation in ints,
2999     * including space for at least one sign bit.
3000     */
3001    private int intLength() {
3002    return bitLength()/32 + 1;
3003    }
3004
3005    /* Returns sign bit */
3006    private int signBit() {
3007    return (signum < 0 ? 1 : 0);
3008    }
3009
3010    /* Returns an int of sign bits */
3011    private int signInt() {
3012    return (int) (signum < 0 ? -1 : 0);
3013    }
3014
3015    /**
3016     * Returns the specified int of the little-endian two's complement
3017     * representation (int 0 is the least significant). The int number can
3018     * be arbitrarily high (values are logically preceded by infinitely many
3019     * sign ints).
3020     */
3021    private int getInt(int n) {
3022        if (n < 0)
3023            return 0;
3024    if (n >= mag.length)
3025        return signInt();
3026
3027    int magInt = mag[mag.length-n-1];
3028
3029    return (int) (signum >= 0 ? magInt :
3030               (n <= firstNonzeroIntNum() ? -magInt : ~magInt));
3031    }
3032
3033    /**
3034     * Returns the index of the int that contains the first nonzero int in the
3035     * little-endian binary representation of the magnitude (int 0 is the
3036     * least significant). If the magnitude is zero, return value is undefined.
3037     */
3038     private int firstNonzeroIntNum() {
3039    /*
3040     * Initialize firstNonzeroIntNum field the first time this method is
3041     * executed. This method depends on the atomicity of int modifies;
3042     * without this guarantee, it would have to be synchronized.
3043     */
3044    if (firstNonzeroIntNum == -2) {
3045        // Search for the first nonzero int
3046 int i;
3047        for (i=mag.length-1; i>=0 && mag[i]==0; i--)
3048        ;
3049        firstNonzeroIntNum = mag.length-i-1;
3050    }
3051    return firstNonzeroIntNum;
3052    }
3053
3054    /** use serialVersionUID from JDK 1.1. for interoperability */
3055    private static final long serialVersionUID = -8287574255936472291L;
3056
3057    /**
3058     * Serializable fields for BigInteger.
3059     * 
3060     * @serialField signum int
3061     * signum of this BigInteger.
3062     * @serialField magnitude int[]
3063     * magnitude array of this BigInteger.
3064     * @serialField bitCount int
3065     * number of bits in this BigInteger
3066     * @serialField bitLength int
3067     * the number of bits in the minimal two's-complement
3068     * representation of this BigInteger
3069     * @serialField lowestSetBit int
3070     * lowest set bit in the twos complement representation
3071     */
3072    private static final ObjectStreamField[] serialPersistentFields = { 
3073        new ObjectStreamField("signum", Integer.TYPE), 
3074        new ObjectStreamField("magnitude", byte[].class),
3075        new ObjectStreamField("bitCount", Integer.TYPE),
3076        new ObjectStreamField("bitLength", Integer.TYPE),
3077        new ObjectStreamField("firstNonzeroByteNum", Integer.TYPE),
3078        new ObjectStreamField("lowestSetBit", Integer.TYPE)
3079        };
3080
3081    /**
3082     * Reconstitute the <tt>BigInteger</tt> instance from a stream (that is,
3083     * deserialize it). The magnitude is read in as an array of bytes
3084     * for historical reasons, but it is converted to an array of ints
3085     * and the byte array is discarded.
3086     */
3087    private void readObject(java.io.ObjectInputStream   s)
3088        throws java.io.IOException  , ClassNotFoundException   {
3089        /*
3090         * In order to maintain compatibility with previous serialized forms,
3091         * the magnitude of a BigInteger is serialized as an array of bytes.
3092         * The magnitude field is used as a temporary store for the byte array
3093         * that is deserialized. The cached computation fields should be
3094         * transient but are serialized for compatibility reasons.
3095         */
3096
3097        // prepare to read the alternate persistent fields
3098 ObjectInputStream.GetField fields = s.readFields();
3099            
3100        // Read the alternate persistent fields that we care about
3101 signum = (int)fields.get("signum", -2);
3102        byte[] magnitude = (byte[])fields.get("magnitude", null);
3103
3104        // Validate signum
3105 if (signum < -1 || signum > 1) {
3106            String   message = "BigInteger: Invalid signum value";
3107            if (fields.defaulted("signum"))
3108                message = "BigInteger: Signum not present in stream";
3109        throw new java.io.StreamCorruptedException  (message);
3110        }
3111    if ((magnitude.length==0) != (signum==0)) {
3112            String   message = "BigInteger: signum-magnitude mismatch";
3113            if (fields.defaulted("magnitude"))
3114                message = "BigInteger: Magnitude not present in stream";
3115        throw new java.io.StreamCorruptedException  (message);
3116        }
3117
3118        // Set "cached computation" fields to their initial values
3119 bitCount = bitLength = -1;
3120        lowestSetBit = firstNonzeroByteNum = firstNonzeroIntNum = -2;
3121
3122        // Calculate mag field from magnitude and discard magnitude
3123 mag = stripLeadingZeroBytes(magnitude);
3124    }
3125
3126    /**
3127     * Save the <tt>BigInteger</tt> instance to a stream.
3128     * The magnitude of a BigInteger is serialized as a byte array for
3129     * historical reasons.
3130     * 
3131     * @serialData two necessary fields are written as well as obsolete
3132     * fields for compatibility with older versions.
3133     */
3134    private void writeObject(ObjectOutputStream s) throws IOException {
3135        // set the values of the Serializable fields
3136 ObjectOutputStream.PutField fields = s.putFields();
3137        fields.put("signum", signum);
3138        fields.put("magnitude", magSerializedForm());
3139        fields.put("bitCount", -1);
3140        fields.put("bitLength", -1);
3141        fields.put("lowestSetBit", -2);
3142        fields.put("firstNonzeroByteNum", -2);
3143            
3144        // save them
3145 s.writeFields();
3146}
3147
3148    /**
3149     * Returns the mag array as an array of bytes.
3150     */
3151    private byte[] magSerializedForm() {
3152        int bitLen = (mag.length == 0 ? 0 :
3153                      ((mag.length - 1) << 5) + bitLen(mag[0]));
3154        int byteLen = (bitLen + 7)/8;
3155        byte[] result = new byte[byteLen];
3156
3157        for (int i=byteLen-1, bytesCopied=4, intIndex=mag.length-1, nextInt=0;
3158             i>=0; i--) {
3159            if (bytesCopied == 4) {
3160                nextInt = mag[intIndex--];
3161                bytesCopied = 1;
3162            } else {
3163                nextInt >>>= 8;
3164                bytesCopied++;
3165            }
3166            result[i] = (byte)nextInt;
3167        }
3168        return result;
3169    }




    }
}
